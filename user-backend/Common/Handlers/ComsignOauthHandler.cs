using Common.Hubs;
using Common.Interfaces.DB;
using Common.Interfaces.Files;
using Common.Interfaces;
using Common.Interfaces.Oauth;
using Common.Interfaces.PDF;
using Common.Models;
using Common.Models.Documents.Signers;
using Common.Models.Documents.SplitSignature;
using Common.Models.Settings;
using CTInterfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Common.Extensions;

using Common.Enums.Results;
using CTHashSignerExternal;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.ConstrainedExecution;
using CTHashSignerExternal.Classes;
using Common.Enums;
using Common.Models.Files.PDF;
using Comda.Authentication.Models;
using Microsoft.Extensions.DependencyInjection;
using static System.Formats.Asn1.AsnWriter;
using System.ComponentModel.Design;


namespace Common.Handlers
{
    public class ComsignOauthHandler : IOauth
    {
        private readonly GeneralSettings _generalSettings;
        private readonly ILogger _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IDataUriScheme _dataUriScheme;
        private readonly IDocumentCollectionConnector _documentCollectionConnector;
        

        private readonly IPdfPackage _pdfService;
        private const string SEPERATOR = "_ASEPA_";
        private const string SIGN_ALGO = "1.2.840.113549.1.1.10";
        private readonly IFilesWrapper _filesWrapper;
        private readonly IProgramUtilizationConnector _programUtilizationConnector;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly byte[] Dummy_Sign = Enumerable.Range(0, 256).Select(s => (byte)0x40).ToArray();
        public ComsignOauthHandler(IOptions<GeneralSettings> generalSettings, ILogger logger,
            IMemoryCache memoryCache, IDataUriScheme dataUriScheme, 
        IPdfPackage pdfService, IFilesWrapper filesWrapper,
        IServiceScopeFactory serviceScopeFactory,
        IDocumentCollectionConnector documentCollectionConnector, IProgramUtilizationConnector programUtilizationConnector)
        {
            _generalSettings = generalSettings.Value;
            _logger = logger;
            _memoryCache = memoryCache;
            _dataUriScheme = dataUriScheme;
            _documentCollectionConnector = documentCollectionConnector;
            
            _pdfService = pdfService;
            _filesWrapper = filesWrapper;
            _programUtilizationConnector = programUtilizationConnector;
            _serviceScopeFactory = serviceScopeFactory;
        }

        private int FindBytes(byte[] src, byte[] find)
        {
            int index = -1;
            int matchIndex = 0;
            // handle the complete source array
            for(int i=0; i<src.Length; i++)
            {
                if (src[i] == find[matchIndex])
                {
                    if (matchIndex == (find.Length - 1))
                    {
                        index = i - matchIndex; break;
                    }
                    matchIndex++;
                }
                else if (src[i] == find[0])
                {
                    matchIndex = 1;
                }
                else
                {
                    matchIndex = 0;
                }

            }
            return index;
        }

        private byte[] ReplaceBytes(byte[] src, byte[] search, byte[] repl)
        {
            byte[] dst = null;
            int index = FindBytes(src, search);
            if (index >= 0)
            {
                dst = new byte[src.Length - search.Length + repl.Length];
                // before found array
                Buffer.BlockCopy(src, 0, dst, 0, index);
                // repl copy
                Buffer.BlockCopy(repl, 0, dst, index, repl.Length);
                // rest of src array
                Buffer.BlockCopy(
                    src,
                    index + search.Length,
                    dst,
                    index + repl.Length,
                    src.Length - (index + search.Length));
            }
            return dst;
        }


        public string GetURLForStartAuthForEIdasFlow(SignerTokenMapping signerTokenMapping, string callBackUrl)
        {

            _logger.Information("ComsignOathHandler - GetURLForStartAuthForEIdasFlow - Start");
            var savedData = _memoryCache.Get($"EIDASSign_{signerTokenMapping.GuidToken}");
            if (savedData == null)
            {
                throw new InvalidOperationException(ResultCode.ExternalFlowInfoNotExist.GetNumericString());
            }

            UriBuilder callbackUriBuilder = new(callBackUrl);
            UriBuilder uriBuilder = new($"{_generalSettings.ComsignIDPURL}/csc/v1/oauth2/authorize");
            QueryBuilder queryBuilder = new()
            {
                { "scope", "service" },
                { "response_type", "code" },
                { "client_id", _generalSettings.ComsignIDPClientId },
                { "redirect_uri", callbackUriBuilder.ToString() },
                { "state", $"{signerTokenMapping.GuidToken}" }
            };
            uriBuilder.Query = queryBuilder.ToQueryString().ToString();
            return uriBuilder.ToString();

        }

        public async Task<SplitDocumentProcess> ProcessAfterSignerAuth(IdentityFlow identityFlow, string callBackUrl)
        {
            SplitDocumentProcess result = new SplitDocumentProcess();
            DocumentCollectionForSplitSignatureProcessInput savedData = _memoryCache.Get<DocumentCollectionForSplitSignatureProcessInput>($"EIDASSign_{identityFlow.SignerToken}");

            if (savedData == null)
            {
                throw new InvalidOperationException(ResultCode.ExternalFlowInfoNotExist.GetNumericString());
            }

            if (string.IsNullOrWhiteSpace(savedData.AccessToken) || string.IsNullOrWhiteSpace(savedData.SignerCredId))
            {
                result = await CreateSignFlowAfterSuccessAuth(identityFlow, result, savedData, callBackUrl);
            }

            else
            {
                DocumentSplitSignatureDataProcessInput documentSplitSignatureDataProcessInput = savedData.Documents.Find(x => x.Id == identityFlow.DocumentId);
                SignatureFieldData selectedSignatureToSign = documentSplitSignatureDataProcessInput.SignatureFields.Find(x => x.Name == identityFlow.FieldName);

                var tokenIdpResponse = await GetToken(identityFlow.Code, callBackUrl);
                var signedHash =  await SignHash(savedData, tokenIdpResponse, savedData.SignerCredId, Convert.ToBase64String( selectedSignatureToSign.Hash));

                selectedSignatureToSign.SignedCms = ReplaceBytes(selectedSignatureToSign.SignedCms, Dummy_Sign,
                    Convert.FromBase64String( signedHash.Signatures.FirstOrDefault()));

                SetResponse response = _pdfService.SetSignature(selectedSignatureToSign.PrepareSignaturePdfResult, selectedSignatureToSign.SignedCms);
              
                if (response.Result.ToString() == CTResult.SUCCESS.ToString())
                {
                    AfterSetSignatureSuccsesfuly(identityFlow, savedData, documentSplitSignatureDataProcessInput, selectedSignatureToSign, response);
                    (selectedSignatureToSign, Guid documetId) = PrepareSignatureNextField(identityFlow.SignerToken.ToString(), savedData);
                    
                    if (selectedSignatureToSign == null)
                    {
                        result = await AfterLastSigntureSignedProcess(identityFlow, result, savedData);
                    }

                    else
                    {
                        _memoryCache.Set<DocumentCollectionForSplitSignatureProcessInput>($"EIDASSign_{identityFlow.SignerToken}", savedData, TimeSpan.FromMinutes(3));
                        result = BuildCredentialFlowForSigning(savedData, identityFlow, selectedSignatureToSign, documetId, identityFlow.SignerToken, callBackUrl);
                    }
                }
            }

            return result;
        }

        private async Task<SignHashResponse> SignHash(DocumentCollectionForSplitSignatureProcessInput savedData, 
            TokenIdpResponse tokenIdpResponse, string signerCred, string hash)
        {
            SignHashResponse result = null;
            
            RestClientOptions clientOptions = new()
            {
                RemoteCertificateValidationCallback = (a, b, c, d) => true
            };

            using RestClient restClient = new(clientOptions);
            RestRequest restRequest = new($"{_generalSettings.ComsignIDPURL}/csc/v1/signatures/signHash", Method.Post);
            
            restRequest.AddJsonBody(new SignHashRequest()
            {
                CredentialID = signerCred,
                Hash = new List<string>() { hash },
                SAD = tokenIdpResponse.access_token,
                SignAlgo = SIGN_ALGO
            });
            restRequest.AddHeader("Authorization", $"Bearer {savedData.AccessToken}");

            RestResponse<SignHashResponse> response = await restClient.ExecutePostAsync<SignHashResponse>(restRequest);
            
            if (response.StatusCode == HttpStatusCode.OK)
            {
                result = JsonConvert.DeserializeObject<SignHashResponse>(response.Content);
            }
            
            else
            {
                throw new InvalidOperationException(ResultCode.ExternalFlowInfoNotExist.GetNumericString());
            }

            return result;
        }

        private async Task<string> ReadCertificateInfo(TokenIdpResponse tokenIdpResponse, string signerCredId)
        {
            GetCredentialInfoRequest getCredInfoRequest = new()
            {
                Certificates = "single",
                CredentialID = signerCredId,
                AuthInfo = false,
                CertInfo = true,
                Lang = "en"
            };

            RestClientOptions clientOptions = new()
            {
                RemoteCertificateValidationCallback = (a, b, c, d) => true
            };

            using RestClient restClient = new(clientOptions);
            RestRequest restRequest = new($"{_generalSettings.ComsignIDPURL}/csc/v1/credentials/info", Method.Post);
            restRequest.AddJsonBody(getCredInfoRequest);
            restRequest.AddHeader("Authorization", $"Bearer {tokenIdpResponse.access_token}");
            RestResponse<GetCredentialInfoResponse> response = await restClient.ExecutePostAsync<GetCredentialInfoResponse>(restRequest);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                GetCredentialInfoResponse getCredentialInfoResponse = JsonConvert.DeserializeObject<GetCredentialInfoResponse>(response.Content!);
                return getCredentialInfoResponse.Cert.Certificates.FirstOrDefault();
            }
            
            else
            {
                throw new InvalidOperationException(ResultCode.ExternalFlowInfoNotExist.GetNumericString());
            }
        }

        private async Task<GetCredentialsEidasResponse> GetCredentinal(TokenIdpResponse tokenIdpResponse)
        {
            GetCredentialsEidasResponse result = null;
            RestClientOptions clientOptions = new()
            {
                RemoteCertificateValidationCallback = (a, b, c, d) => true
            };
            using RestClient restClient = new(clientOptions);
            RestRequest restRequest = new($"{_generalSettings.ComsignIDPURL}/csc/v1/credentials/list", Method.Post);

            restRequest.AddJsonBody(new { MaxResults = 10 });
            restRequest.AddHeader("Authorization", $"Bearer {tokenIdpResponse.access_token}");
            RestResponse<object> response = await restClient.ExecutePostAsync<object>(restRequest);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                result = JsonConvert.DeserializeObject<GetCredentialsEidasResponse>(response.Content);

                if (!result.CredentialIDs.Any())
                {
                    result = null;
                }

            }
            else
            {
                throw new InvalidOperationException(ResultCode.ExternalFlowInfoNotExist.GetNumericString());
            }
            return result;
        }

        private async Task<SplitDocumentProcess> AfterLastSigntureSignedProcess(IdentityFlow identityFlow, SplitDocumentProcess result, DocumentCollectionForSplitSignatureProcessInput savedData)
        {
            _memoryCache.Remove($"EIDASSign_{identityFlow.SignerToken}");
            foreach (var item in savedData.DocumentsData)
            {
                _filesWrapper.Documents.SaveDocument(Enums.DocumentType.Document, item.Id, item.Data);
            }
            var documentCollection = await _documentCollectionConnector.Read(new DocumentCollection { Id = savedData.SignerTokenMapping.DocumentCollectionId });
            var signer = documentCollection.Signers.FirstOrDefault(x => x.Id == savedData.SignerTokenMapping.SignerId);


            using var scope = _serviceScopeFactory.CreateScope();
            IDoneDocuments dependencyService = scope.ServiceProvider.GetService<IDoneDocuments>();
            string url = await dependencyService.DoneProcess(documentCollection, signer);
            await _programUtilizationConnector.AddDocument(documentCollection.User);
            result = new SplitDocumentProcess()
            {
                ProcessStep = Enums.Documents.SplitSignProcessStep.Success,
                Url = url
            };
            return result;
        }

        private async Task<SplitDocumentProcess> CreateSignFlowAfterSuccessAuth(IdentityFlow identityFlow, SplitDocumentProcess result,
            DocumentCollectionForSplitSignatureProcessInput savedData, string callBackUrl)
        {
            var tokenIdpResponse = await GetToken(identityFlow.Code, callBackUrl);
            var signerCred = await GetCredentinal(tokenIdpResponse);
           
            // need to read info the certificate is in the response...
            savedData.AccessToken = tokenIdpResponse.access_token;
            savedData.SignerCredId = signerCred.CredentialIDs.FirstOrDefault();
            var certificateInfo = await ReadCertificateInfo(tokenIdpResponse, savedData.SignerCredId);
            savedData.CertificateInfo = certificateInfo;
            Guid memToken = Guid.NewGuid();
            (SignatureFieldData selectedSignatureToSign, Guid documetId) = PrepareSignatureNextField(identityFlow.SignerToken.ToString(), savedData);
            _memoryCache.Set<DocumentCollectionForSplitSignatureProcessInput>($"EIDASSign_{memToken}", savedData, TimeSpan.FromMinutes(3));
            _memoryCache.Remove($"EIDASSign_{identityFlow.SignerToken}");
            result = BuildCredentialFlowForSigning(savedData, identityFlow, selectedSignatureToSign, documetId, memToken, callBackUrl);
            return result;
        }

     

        private async Task<TokenIdpResponse> GetToken(string code, string callBackUrl )
        {

            TokenIdpResponse result = null;
            RestClientOptions clientOptions = new()
            {
                RemoteCertificateValidationCallback = (a, b, c, d) => true
            };
            using RestClient restClient = new(clientOptions);

            UriBuilder callbackUriBuilder = new(callBackUrl);
            RestRequest restRequest = new($"{_generalSettings.ComsignIDPURL}/csc/v1/oauth2/token", Method.Post);
            restRequest.AddParameter("grant_type", "authorization_code", ParameterType.GetOrPost);
            restRequest.AddParameter("client_id", _generalSettings.ComsignIDPClientId, ParameterType.GetOrPost);
            restRequest.AddParameter("client_secret", _generalSettings.ComsignIDPClientSecret, ParameterType.GetOrPost);
            restRequest.AddParameter("code", code, ParameterType.GetOrPost);
            restRequest.AddParameter("redirect_uri", callbackUriBuilder.ToString(), ParameterType.GetOrPost);

            RestResponse<TokenIdpResponse> response = await restClient.ExecutePostAsync<TokenIdpResponse>(restRequest);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                result = JsonConvert.DeserializeObject<TokenIdpResponse>(response.Content);
            }
            else
            {

                throw new InvalidOperationException(ResultCode.ExternalFlowInfoNotExist.GetNumericString());
            }
            return result;


        }

        private void AfterSetSignatureSuccsesfuly(IdentityFlow identityFlow, DocumentCollectionForSplitSignatureProcessInput savedData, DocumentSplitSignatureDataProcessInput documentSplitSignatureDataProcessInput, SignatureFieldData selectedSignatureToSign, SetResponse response)
        {
            savedData.NumberOfSignaturesSigned++;
            savedData.DocumentsData.Find(x => x.Id == identityFlow.DocumentId).Data = response.PDF;
            documentSplitSignatureDataProcessInput.SignatureFields.Remove(selectedSignatureToSign);
            if (!documentSplitSignatureDataProcessInput.SignatureFields.Any())
            {
                savedData.Documents.Remove(documentSplitSignatureDataProcessInput);
            }
        }

        private (SignatureFieldData, Guid DocumentId ) PrepareSignatureNextField(string signerToken,
            DocumentCollectionForSplitSignatureProcessInput documentCollectionForSplitSignatureProcessInput)
        {
            List<SignatureFieldData> signatureFields = GetNextSignatureField(documentCollectionForSplitSignatureProcessInput);

            if (signatureFields != null)
            {
                PrepResponse res = PrepareSignatureForField(documentCollectionForSplitSignatureProcessInput, signatureFields);
                byte[] hashToSign = res.Hash;
                signatureFields.FirstOrDefault().PrepareSignaturePdfResult = res.PDF;

                byte[] signedCms = SignCMS(documentCollectionForSplitSignatureProcessInput, signatureFields, hashToSign);
                signatureFields.FirstOrDefault().SignedCms = signedCms;
                return (signatureFields.FirstOrDefault(), documentCollectionForSplitSignatureProcessInput.Documents.FirstOrDefault()?.Id ?? Guid.Empty);
            }

            return (null, Guid.Empty);
        }

        //private (SignatureFieldData, Guid DocumentId) PrepareSignatureNextField2(string signerToken,
        //   DocumentCollectionForSplitSignatureProcessInput documentCollectionForSplitSignatureProcessInput)
        //{

        //    SignatureFieldData signatureField = GetNextSignatureField(documentCollectionForSplitSignatureProcessInput);

        //    if (signatureField != null)
        //    {
        //        PrepResponse res = PrepareSignatureForField(documentCollectionForSplitSignatureProcessInput, signatureField);
        //        byte[] hashToSign = res.Hash;
        //        signatureField.PrepareSignaturePdfResult = res.PDF;

        //        byte[] signedCms = SignCMS(documentCollectionForSplitSignatureProcessInput, signatureField, res.PDF, hashToSign);
        //        signatureField.SignedCms = signedCms;
        //        return (signatureField, documentCollectionForSplitSignatureProcessInput.Documents.FirstOrDefault()?.Id ?? Guid.Empty);
        //    }

        //    return (null, Guid.Empty);
        //}

        private byte[] SignCMS(DocumentCollectionForSplitSignatureProcessInput documentCollectionForSplitSignatureProcessInput, List<SignatureFieldData> signatureFields, byte[] hashToSign)
        {
            CSignExternal cSignExternal = new CSignExternal();
            CRED_DATA cred_Data = new CRED_DATA()
            {
                CertId = documentCollectionForSplitSignatureProcessInput.SignerCredId,


            };
            var certPemString = documentCollectionForSplitSignatureProcessInput.CertificateInfo.Replace("-----BEGIN CERTIFICATE-----", null)
                                    .Replace("-----END CERTIFICATE-----", null);
            var cert = new X509Certificate2(Convert.FromBase64String(certPemString));

            CERT_DATA cert_Data = new CERT_DATA()
            {
                BuildChain = true,
                SignCert = cert
            };
            SIGN_DATA sign_Data = new SIGN_DATA()
            {
                DigestAlg = CTDigestAlg.SHA256,
                RsaSigMode = RsaSigMode.PSS
            };

            Func<PKCS7SignInput, byte[], (CTResult, byte[])> signHashRawCallback = (PKCS7SignInput pkcs7Input, byte[] hash) =>
            {
                signatureFields.FirstOrDefault().Hash = hash;
                return (CTResult.SUCCESS, Dummy_Sign);
            };

            (CTResult ctResult, byte[] signedCms) = cSignExternal
                .SignCMS(cred_Data, cert_Data, sign_Data, null, hashToSign, null, signHashRawCallback);
            return signedCms;
        }

        private byte[] SignCMS(DocumentCollectionForSplitSignatureProcessInput documentCollectionForSplitSignatureProcessInput, SignatureFieldData signatureField, byte[] file, byte[] hashToSign)
        {
            CSignExternal cSignExternal = new CSignExternal();
            CRED_DATA cred_Data = new CRED_DATA()
            {
                CertId = documentCollectionForSplitSignatureProcessInput.SignerCredId,


            };
            var certPemString = documentCollectionForSplitSignatureProcessInput.CertificateInfo.Replace("-----BEGIN CERTIFICATE-----", null)
                                    .Replace("-----END CERTIFICATE-----", null);
            var cert = new X509Certificate2(Convert.FromBase64String(certPemString));

            CERT_DATA cert_Data = new CERT_DATA()
            {
                BuildChain = true,
                SignCert = cert
            };
            SIGN_DATA sign_Data = new SIGN_DATA()
            {
                DigestAlg = CTDigestAlg.SHA256,
                RsaSigMode = RsaSigMode.PSS
            };

            Func<PKCS7SignInput, byte[], (CTResult, byte[])> signHashRawCallback = (PKCS7SignInput pkcs7Input, byte[] hash) =>
            {
                signatureField.Hash = hash;
                return (CTResult.SUCCESS, Dummy_Sign);
            };

            (CTResult ctResult, byte[] signedCms) = cSignExternal
                .SignCMS(cred_Data, cert_Data, sign_Data, null, hashToSign, file, signHashRawCallback);
            return signedCms;
        }

        public PrepResponse PrepareSignatureForField(DocumentCollectionForSplitSignatureProcessInput documentCollectionForSplitSignatureProcessInput,
            List<SignatureFieldData> signatureFields)
        {
            var base64image = _dataUriScheme.Getbase64Content(signatureFields.FirstOrDefault().Image);

            byte[] bytes = documentCollectionForSplitSignatureProcessInput.DocumentsData.Find(x => x.Id == 
            documentCollectionForSplitSignatureProcessInput.Documents.FirstOrDefault()?.Id)?.Data;
            var fieldsNames = signatureFields.Select(sf => sf.Name).ToArray();
            PrepResponse res = _pdfService.PrepareSignatureForField(fieldsNames, Convert.FromBase64String(base64image), bytes);
            if (res.Result.ToString() != CTResult.SUCCESS.ToString())
            {
                throw new Exception($"ComsignOathHandler - Failed to PrepareSignatureForField - {res.Result}");
            }

            return res;
        }

        private List<SignatureFieldData> GetNextSignatureField(DocumentCollectionForSplitSignatureProcessInput documentCollectionForSplitSignatureProcessInput)
        {
            var doc = documentCollectionForSplitSignatureProcessInput.Documents.FirstOrDefault();

            if (doc != null)
            {
                return doc.SignatureFields;
            }
            return null;

        }
        private SplitDocumentProcess BuildCredentialFlowForSigning(DocumentCollectionForSplitSignatureProcessInput savedData, IdentityFlow identityFlow,
            SignatureFieldData selectedSignatureToSign, Guid documetId, Guid memToken, string callBackUrl)
        {
            SplitDocumentProcess result = new SplitDocumentProcess()
            {
                ProcessStep = Enums.Documents.SplitSignProcessStep.InProgress
            };
            UriBuilder callbackUriBuilder = new(callBackUrl);
            UriBuilder uriBuilder = new($"{_generalSettings.ComsignIDPURL}/csc/v1/oauth2/authorize");
            QueryBuilder queryBuilder = new()
            {
                { "scope", "credential" },
                { "response_type", "code" },
                { "client_id", _generalSettings.ComsignIDPClientId },
                { "redirect_uri", callbackUriBuilder.ToString() },
                { "state", $"{memToken}{SEPERATOR}{selectedSignatureToSign.Name}{SEPERATOR}{documetId}" },
                { "lang", "En" },
                { "credentialID", savedData.SignerCredId },
                { "numSignatures", "1" },
                { "hash", selectedSignatureToSign.Hash.ToBase64Url() },
                { "description", $"Signature for Document {savedData.CollectionName}: {savedData.NumberOfSignaturesSigned + 1} of {savedData.NumberOfSignatures} " }
            };
            uriBuilder.Query = queryBuilder.ToQueryString().ToString();
            result.Url = uriBuilder.ToString();
            return result;
        }

        public void SaveDataForEidasProcess(SignerTokenMapping signerTokenMapping, DocumentCollection inputDocumentCollection)
        {
            DocumentCollectionForSplitSignatureProcessInput documentCollectionForSplitSignatureProcessInput = new DocumentCollectionForSplitSignatureProcessInput()
            {
                CollectionId = inputDocumentCollection.Id,
                SignerTokenMapping = signerTokenMapping,
                CollectionName = inputDocumentCollection.Name,

            };
            int numberOfSignatures = 0;
            foreach (var document in inputDocumentCollection.Documents)
            {
                documentCollectionForSplitSignatureProcessInput.DocumentsData.Add(new DocumentSplitFileDataProcessInput()
                {
                    Id = document.Id,
                    Data = _filesWrapper.Documents.ReadDocument(DocumentType.Document, document.Id)

                });

                DocumentSplitSignatureDataProcessInput documentSplitSignatureDataProcessInput = new DocumentSplitSignatureDataProcessInput()
                {
                    Id = document.Id,
                    SignatureFields = new List<SignatureFieldData>(),

                };

                foreach (var sigField in document?.Fields?.SignatureFields ?? Enumerable.Empty<SignatureField>())
                {
                    documentSplitSignatureDataProcessInput.SignatureFields.Add(new SignatureFieldData()
                    {
                        Image = sigField.Image,
                        Name = sigField.Name,
                    });
                }

                if (documentSplitSignatureDataProcessInput.SignatureFields.Count > 0)
                {
                    documentCollectionForSplitSignatureProcessInput.Documents.Add(documentSplitSignatureDataProcessInput);
                }
                documentCollectionForSplitSignatureProcessInput.NumberOfSignatures += documentSplitSignatureDataProcessInput.SignatureFields.Count;
            }
            _memoryCache.Set($"EIDASSign_{signerTokenMapping.GuidToken}", documentCollectionForSplitSignatureProcessInput, TimeSpan.FromMinutes(6));
        }
    }

    public class SignHashResponse
    {
        [JsonPropertyName("signatures")]
        public List<string> Signatures { get; set; } = new();
    }
    public class GetCredentialsEidasResponse
    {
        [JsonPropertyName("credentialIDs")]
        public List<string> CredentialIDs { get; set; } = new List<string>();
    }
    public class TokenIdpResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
        public string id_token { get; set; }
    }

    public class SignHashRequest
    {
        [JsonPropertyName("credentialID")]
        public string CredentialID { get; set; } = "";

        [JsonPropertyName("SAD")]
        public string SAD { get; set; } = "";

        [JsonPropertyName("hash")]
        public List<string> Hash { get; set; } = new();

        [JsonPropertyName("hashAlgo")]
        public string HashAlgo { get; set; } = "";

        [JsonPropertyName("signAlgo")]
        public string SignAlgo { get; set; } = "";

        [JsonPropertyName("signAlgoParams")]
        public string SignAlgoParams { get; set; } = "";

        [JsonPropertyName("clientData")]
        public string ClientData { get; set; } = "";
    }


    public class GetCredentialInfoResponse
    {
        public Cert Cert { get; set; }


    }

    public class Cert
    {
        public List<string> Certificates { get; set; }
        public string issuerDN { get; set; }
        public string serialNumber { get; set; }
        public string subjectDN { get; set; }
        public string validFrom { get; set; }
        public string validTo { get; set; }
    }
    public class GetCredentialInfoRequest
    {
        [JsonPropertyName("credentialID")]
        public string? CredentialID { get; set; }
        [JsonPropertyName("certificates")]
        public string? Certificates { get; set; } /* none | single | chain  */
        [JsonPropertyName("certInfo")]
        public bool? CertInfo { get; set; }
        [JsonPropertyName("authInfo")]
        public bool AuthInfo { get; set; }
        [JsonPropertyName("lang")]
        public string? Lang { get; set; }
        [JsonPropertyName("clientData")]
        public string? ClientData { get; set; }
    }

}
