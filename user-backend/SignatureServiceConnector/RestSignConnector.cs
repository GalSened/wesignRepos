using Common.Enums;
using Common.Interfaces;
using Common.Models.Configurations;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MimeKit.Encodings;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SignatureServiceConnector
{
    public class RestSignConnector : ISignConnector
    {
        protected const string MEDIA_TYPE = "application/json";
        private readonly IEncryptor _encryptor;
        private readonly SignerOneExtraInfoSettings _signerOneExtraInfoSettings;
        private readonly IDater _dater;
        private readonly GeneralSettings _generalSettings;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public RestSignConnector(IConfiguration configuration, IEncryptor encryptor, IOptions<SignerOneExtraInfoSettings> signerOneExtraInfoSettings,
            IOptions<GeneralSettings> generalSettings,ILogger logger,
            IDater dater)
        {
            _configuration = configuration;
            _encryptor = encryptor;
            _signerOneExtraInfoSettings = signerOneExtraInfoSettings.Value;
            _dater = dater;
            _generalSettings = generalSettings.Value;
            _logger = logger;
        }


        public  Task<(Signer1ResCode ResultCode, byte[] SignedBytes)> SignExcel(string certId, byte[] inputFile, string pincode, string token, CompanySigner1Details companySigner1Details)
        {
            _logger.Debug("Start Signing excel process");
            (certId,pincode) = SetPinCode(pincode, certId, companySigner1Details);
            
            return  SignWordExcelDocument(certId, inputFile, pincode, token, companySigner1Details, "SignExcel_PIN");
        }

      

        public async Task<(Signer1ResCode ResultCode, byte[] SignedBytes)> SignPdf(string certId, byte[] inputFile, string pincode,
            string token, CompanySigner1Details companySigner1Details)
        {
            _logger.Debug("Start Signing PDF process");
            Signer1Configuration signer1Configuration = await _configuration.GetSigner1Configuration(companySigner1Details);
            (certId, pincode) = SetPinCode(pincode, certId, companySigner1Details);
            Dictionary<string, string> headers = GetHeadersInfo();
            using (var client = new HttpClient())
            {


                SignPDFPINEmptyImageModel signPDFPINModel = new SignPDFPINEmptyImageModel
                {
                    CertID = certId,
                    Pincode = pincode,
                    Token = CreateTokenForSigner1(certId, token),
                    InputFile = Convert.ToBase64String(inputFile),
                    Page = 1,
                    Left = 0,
                    Top = 0,
                    Width = 100,
                    Height = 100,
                    
                };
                string content = JsonConvert.SerializeObject(signPDFPINModel).ToString();
                StringContent stringContent = new StringContent(content, Encoding.UTF8, MEDIA_TYPE);

                foreach (var pair in headers)
                {
                    client.DefaultRequestHeaders.Add(pair.Key, pair.Value);
                }
                AddAuth(client, signer1Configuration);
                var serverResponse = await client.PostAsync($"{signer1Configuration.Endpoint}/{_signerOneExtraInfoSettings.RestUrl}SignPDF_PIN", stringContent);
                if (serverResponse.StatusCode == HttpStatusCode.BadRequest ||
                        serverResponse.StatusCode == HttpStatusCode.InternalServerError)
                {
                    return ((await HandleBadErrorCode(serverResponse)).ResultCode, null);
                }
                else
                {
                    string resposnsString = await serverResponse.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<SignResponse>(resposnsString);
                    return (result.Result, Convert.FromBase64String(result.SignedBytes));


                }
            }
        }

        public async Task<(Signer1ResCode ResultCode, byte[] SignedBytes)> SignPdfField(string certId, byte[] inputFile, string fieldName, string pincode, byte[] image, string token, CompanySigner1Details companySigner1Details)
        {
            _logger.Debug("Start Signing PDF field process");
            Signer1Configuration signer1Configuration = await _configuration.GetSigner1Configuration(companySigner1Details);
            Dictionary<string, string> headers = GetHeadersInfo();
            (certId, pincode) = SetPinCode(pincode, certId, companySigner1Details);
            using (var client = new HttpClient())
            {
                SignPDFFieldModel sifnPdfField = new SignPDFFieldModel
                {
                    CertID = certId,
                    Pincode = pincode,
                    Token = CreateTokenForSigner1(certId, token),
                    FieldName= fieldName,
                    Image = Convert.ToBase64String(image),
                    InputFile = Convert.ToBase64String(inputFile)                    
                };
                string content = JsonConvert.SerializeObject(sifnPdfField).ToString();
                StringContent stringContent = new StringContent(content, Encoding.UTF8, MEDIA_TYPE);

                foreach (var pair in headers)
                {
                    client.DefaultRequestHeaders.Add(pair.Key, pair.Value);
                }
                AddAuth(client, signer1Configuration);
                var serverResponse = await client.PostAsync($"{signer1Configuration.Endpoint}/{_signerOneExtraInfoSettings.RestUrl}SignPDF_PIN_FIELD", stringContent    );
                if (serverResponse.StatusCode == HttpStatusCode.BadRequest ||
                        serverResponse.StatusCode == HttpStatusCode.InternalServerError)
                {
                    return ((await HandleBadErrorCode(serverResponse)).ResultCode, null);
                    
                }
                else
                {
                    string resposnsString = await serverResponse.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<SignResponse>(resposnsString);
                    if(result.Result != Signer1ResCode.SUCCESS)
                    {
                        _logger.Error($"return 200 from signer one but result with Error status {result.Result}");
                        return (result.Result, null);
                    }

                    return (result.Result, Convert.FromBase64String(result.SignedBytes));


                }
            }
                
        }

   

        public  Task<(Signer1ResCode ResultCode, byte[] SignedBytes)> SignWord(string certId, byte[] inputFile, string pincode, string token, CompanySigner1Details companySigner1Details)
        {
            _logger.Debug("Start Signing Word process");
            (certId, pincode) = SetPinCode(pincode, certId, companySigner1Details );
            return  SignWordExcelDocument(certId, inputFile, pincode, token,companySigner1Details, "SignWord_PIN");
        }

        public async Task<(Signer1ResCode ResultCode, byte[] SignedBytes)> SignXML(string certId, byte[] inputFile, string pincode, string token, CompanySigner1Details companySigner1Details)
        {
            _logger.Debug("Start Signing XML process");
            Signer1Configuration signer1Configuration = await _configuration.GetSigner1Configuration(companySigner1Details);
            (certId, pincode) = SetPinCode(pincode, certId, companySigner1Details);
            Dictionary<string, string> headers = GetHeadersInfo();
            using (var client = new HttpClient())
            {
                
                SignXmlPINModel signXmlPINModel = new SignXmlPINModel
                {
                    CertID = certId,
                    Pincode = pincode,
                    Token = CreateTokenForSigner1(certId, token),                 
                    InputXML = Convert.ToBase64String(inputFile)
                };
                string content = JsonConvert.SerializeObject(signXmlPINModel).ToString();
                StringContent stringContent = new StringContent(content, Encoding.UTF8, MEDIA_TYPE);

                foreach (var pair in headers)
                {
                    client.DefaultRequestHeaders.Add(pair.Key, pair.Value);
                }
                AddAuth(client, signer1Configuration);
                var serverResponse = await client.PostAsync($"{signer1Configuration.Endpoint}/{_signerOneExtraInfoSettings.RestUrl}SignXml_PIN", stringContent);

                if (serverResponse.StatusCode == HttpStatusCode.BadRequest ||
                        serverResponse.StatusCode == HttpStatusCode.InternalServerError)
                {
                    return ((await HandleBadErrorCode(serverResponse)).ResultCode, null);
                }
                else
                {
                    string resposnsString = await serverResponse.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<SignResponse>(resposnsString);
                    return (result.Result, Convert.FromBase64String(result.SignedBytes));


                }
            }
        }

        public async Task<Signer1ResCode> VerifyCredential(string certId, string pinCode, string token, CompanySigner1Details companySigner1Details)
        {
            _logger.Debug("Start Verify Credential process");
            Signer1Configuration signer1Configuration = await _configuration.GetSigner1Configuration(companySigner1Details);

            (certId, pinCode) = SetPinCode(pinCode, certId, companySigner1Details);
            Dictionary<string, string> headers = GetHeadersInfo();
            using (var client = new HttpClient())
            {
                SignRequestBaseModel verifyCredentialModel = new SignRequestBaseModel
                {
                    CertID = certId,
                    Pincode = pinCode,
                    Token = CreateTokenForSigner1(certId, token)
                };

                string content = JsonConvert.SerializeObject(verifyCredentialModel).ToString();
                StringContent stringContent = new StringContent(content, Encoding.UTF8, MEDIA_TYPE);

                foreach(var pair in headers)
                {
                    client.DefaultRequestHeaders.Add(pair.Key, pair.Value);
                }
                AddAuth(client, signer1Configuration);

                var serverResponse = await client.PostAsync($"{signer1Configuration.Endpoint}/{_signerOneExtraInfoSettings.RestUrl}Cred_Verify", stringContent);
                if (serverResponse.StatusCode == HttpStatusCode.BadRequest ||
                        serverResponse.StatusCode == HttpStatusCode.InternalServerError||
                        serverResponse.StatusCode == HttpStatusCode.ServiceUnavailable)
                {                    
                    return (await HandleBadErrorCode(serverResponse)).ResultCode;
                }
                else
                {
                    string resposnsString = await serverResponse.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Signer1ResCode>(resposnsString);


                }
            }

             
        }
        private async Task<(Signer1ResCode ResultCode, byte[] SignedBytes)> HandleBadErrorCode(HttpResponseMessage serverResponse)
        {

            try
            {
                _logger.Error("Error while trying to sign using Signer1 error Code {StatusCode} ", serverResponse.StatusCode);
                var responseMessage = await serverResponse.Content.ReadAsStringAsync();
                _logger.Error("Signer1 error content {ResponseMessage} ", responseMessage);

            }
            catch
            {
                // do nothing
            }

            return (Signer1ResCode.GENERAL_ERROR, null);
        }
        private string CreateTokenForSigner1(string certId, string token)
        {

            if(_signerOneExtraInfoSettings != null && _signerOneExtraInfoSettings.UseAuthAssertion)
            {
                return token;
            }

                if (_signerOneExtraInfoSettings != null && string.IsNullOrWhiteSpace(_signerOneExtraInfoSettings.CertPath) || string.IsNullOrWhiteSpace(_signerOneExtraInfoSettings.Key1))
            {
                return string.Empty;
            }
            string certPath = $@"{_signerOneExtraInfoSettings.CertPath}";
            string certPass = _encryptor.Decrypt(_signerOneExtraInfoSettings.Key1);

            var collection = new X509Certificate2Collection();
            collection.Import(certPath, certPass, X509KeyStorageFlags.PersistKeySet);

            var certificate = collection[collection.Count -1];
            
            var rsaPrivateKey = certificate.GetRSAPrivateKey();
            var privateSecurityKey = new RsaSecurityKey(rsaPrivateKey);

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _generalSettings.UserFronendApplicationRoute,                
                IssuedAt = _dater.UtcNow(),
                NotBefore = _dater.UtcNow(),
                Expires = _dater.UtcNow().AddMinutes(5),
                Subject = new ClaimsIdentity(new List<Claim> { new Claim("sub", certId) }),
                SigningCredentials = new SigningCredentials(privateSecurityKey, SecurityAlgorithms.RsaSha256Signature)
            };

            var handler = new JsonWebTokenHandler();
            
            return handler.CreateToken(descriptor);

        }
        private (string,string) SetPinCode(string pincode, string certId, CompanySigner1Details companySigner1Details)
        {
            var resultPinCode = pincode;
            var resultCertId = certId;

            if (_signerOneExtraInfoSettings != null)
            {
                if (!string.IsNullOrWhiteSpace(_signerOneExtraInfoSettings.PersistentKey2))
                {
                    string encryptedCode = _encryptor.Decrypt(_signerOneExtraInfoSettings.PersistentKey2);
                    if (!string.IsNullOrWhiteSpace(encryptedCode))
                    {
                        _logger.Debug("Signer1 in Key2 logic");
                        resultPinCode = encryptedCode;
                    }
                }

                else if (_signerOneExtraInfoSettings.CertPinLogic)
                {
                    _logger.Debug("Signer1 in CertPinLogic logic");
                    resultPinCode = certId;
                }
            }

            return (resultCertId, resultPinCode);
        }
        private async Task<(Signer1ResCode ResultCode, byte[] SignedBytes)> SignWordExcelDocument(string certId, byte[] inputFile, string pincode, string token,
            CompanySigner1Details companySigner1Details,string actionName)
        {
            Signer1Configuration signer1Configuration = await _configuration.GetSigner1Configuration(companySigner1Details);
            Dictionary<string, string> headers = GetHeadersInfo();
            using (var client = new HttpClient())
            {
                SignExcelWordPINModel signExcelPINModel = new SignExcelWordPINModel
                {
                    CertID = certId,
                    Pincode = pincode,
                    Token = CreateTokenForSigner1(certId, token),
                    InputFile = Convert.ToBase64String(inputFile),
                    Name = ""
                };
                string content = JsonConvert.SerializeObject(signExcelPINModel).ToString();
                StringContent stringContent = new StringContent(content, Encoding.UTF8, MEDIA_TYPE);

                foreach (var pair in headers)
                {
                    client.DefaultRequestHeaders.Add(pair.Key, pair.Value);
                }
                AddAuth(client, signer1Configuration);

                var serverResponse = await client.PostAsync($"{signer1Configuration.Endpoint}/{_signerOneExtraInfoSettings.RestUrl}{actionName}", stringContent);

                if (serverResponse.StatusCode == HttpStatusCode.BadRequest ||
                        serverResponse.StatusCode == HttpStatusCode.InternalServerError)
                {
                    return ((await HandleBadErrorCode(serverResponse)).ResultCode, null);
                }
                else
                {
                    string resposnsString = await serverResponse.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<SignResponse>(resposnsString);
                    return (result.Result, Convert.FromBase64String(result.SignedBytes));


                }
            }
        }

        private void AddAuth(HttpClient client, Signer1Configuration signer1Configuration)
        {
            if (!string.IsNullOrWhiteSpace(signer1Configuration.User) && !string.IsNullOrWhiteSpace(signer1Configuration.Password))
            {
                string password = _encryptor.Decrypt(signer1Configuration.Password);
                var byteArray = Encoding.ASCII.GetBytes($"{signer1Configuration.User}:{password}");
                var header = new AuthenticationHeaderValue(
                           "Basic", Convert.ToBase64String(byteArray));
                client.DefaultRequestHeaders.Authorization = header;
            }
        }

        private Dictionary<string, string> GetHeadersInfo()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            
            if(_signerOneExtraInfoSettings != null && _signerOneExtraInfoSettings.Headers != null && _signerOneExtraInfoSettings.Headers.Count > 0)
            {
               foreach(var header in _signerOneExtraInfoSettings.Headers)                 
                {
                    headers.Add(header.Name, _encryptor.Decrypt( header.Value));
                }
            }


            return headers;

        }
    }



    public class SignRequestBaseModel
    {
        public string CertID { get; set; }
        public string Pincode { get; set; }
        public string Token { get; set; }
    }

    
    public class SignPDFFieldModel : SignRequestBaseModel
    {
        
        public string InputFile { get; set; }
        public string FieldName { get; set; }        
        public string Image { get; set; }
        
    }


    public class SignPDFPINEmptyImageModel : SignRequestBaseModel
    {
        public string InputFile { get; set; }
        public int Page { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }        


    }

    public class SignXmlPINModel : SignRequestBaseModel
    {
        public string InputXML { get; set; }
    }

    public class SignExcelWordPINModel : SignRequestBaseModel
    {        
        public string InputFile { get; set; }
        public string Name { get; set; }

    }

    public class SignResponse
    {
        public Signer1ResCode Result { get; set; }

        public string SignedBytes { get; set; }

    }
}
