using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.PDF;
using Common.Models.Configurations;
using Common.Models.Files.PDF;
using CTInterfaces;
using Serilog;
using System;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Common.Interfaces.DB;
using System.Net.Http;
using iTextSharp.text;
using Common.Enums;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using System.Text;
using Common.Models.Settings;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace PdfHandler.Signing
{
    class GraphicSigningHandler : ISigning
    {
        private const string PFX_PASSWORD = "123456";

        private readonly IDataUriScheme _dataUriScheme;
        private readonly IConfigurationConnector _configurationConnector;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IEncryptor _encryptor;
        protected readonly ILogger _logger;
        private readonly IImage _image;
        private readonly IPDFSign _pdfSign;
        private readonly ISign _iSign;
        private Configuration _configuration;
        private const string MEDIA_TYPE = "application/json";
        public GraphicSigningHandler(ILogger logger, IPDFSign pdfSign,
            IImage image, ISign sign, IDataUriScheme dataUriScheme, IHttpClientFactory clientFactory, IConfigurationConnector configurationConnector,
            IEncryptor encryptor)
        {

            _logger = logger;
            _image = image;
            _pdfSign = pdfSign;
            _iSign = sign;
            _dataUriScheme = dataUriScheme;
            _configurationConnector = configurationConnector;
            _clientFactory = clientFactory;
            _encryptor = encryptor;

        }

        public async Task<byte[]> Sign(SigningInfo signingInfo, bool useForAllFields = false)
        {
            var fileContent = signingInfo.Data;
            _configuration = await _configurationConnector.Read();

            byte[] signedPDF;
            if (signingInfo.Signatures.Any())
            {
                var signatureList = new List<SignatureField>();
                foreach (var signature in signingInfo.Signatures)
                {
                    //if (useForAllFields)
                    //{
                    //    signatureList.AddRange(signingInfo.Signatures);
                    //}
                    //else
                    //{
                        signatureList.Add(signature);
                    //}
                    // Can be here a bug in group sign mode,
                    //We should lock file while 2 signers trying get to file content
                    if (UseExternalGraphicService())
                    {
                        signedPDF = await SignExternalPFX(fileContent, signingInfo.Certificate, signingInfo.Reason, signatureList);
                    }
                    else
                    {
                        signedPDF = SignInternalPFX(fileContent, signingInfo.Certificate, signingInfo.Reason, signatureList);
                    }
                    if (signedPDF != null)
                    {

                        fileContent = signedPDF;
                    }

                    //if (useForAllFields)
                    //{
                    //    break;
                    //}
                    //else
                    //{
                        signatureList.Clear();
                    //}
                }
            }

            else
            {
                if (UseExternalGraphicService())
                {
                    signedPDF = await SignExternalPFX(fileContent, signingInfo.Certificate, signingInfo.Reason);
                }

                else
                {
                    signedPDF = SignInternalPFX(fileContent, signingInfo.Certificate, signingInfo.Reason);
                }
                if (signedPDF != null)
                {

                    fileContent = signedPDF;
                }
            }
            return fileContent;
        }


        //public async Task<byte[]> Sign(SigningInfo signingInfo, bool useForAllFields = false)
        //{
        //    var fileContent = signingInfo.Data;
        //    _configuration = await _configurationConnector.Read();

        //    byte[] signedPDF;
        //    if (signingInfo.Signatures.Any())
        //    {
        //        var signatureList = new List<SignatureField>();
        //        foreach (var signature in signingInfo.Signatures)
        //        {
        //            if (useForAllFields)
        //            {
        //                signatureList.AddRange(signingInfo.Signatures);
        //            }
        //            else
        //            {
        //                signatureList.Add(signature);
        //            }
        //            // Can be here a bug in group sign mode,
        //            //We should lock file while 2 signers trying get to file content
        //            if (UseExternalGraphicService())
        //            {
        //                signedPDF = await SignExternalPFX(fileContent, signingInfo.Certificate, signingInfo.Reason, signingInfo.Signatures);
        //            }
        //            else
        //            {
        //                signedPDF = SignInternalPFX(fileContent, signingInfo.Certificate, signingInfo.Reason, signingInfo.Signatures);
        //            }
        //            if (signedPDF != null)
        //            {

        //                fileContent = signedPDF;
        //            }

        //            if (useForAllFields)
        //            {
        //                break;
        //            }
        //            else
        //            {
        //                signatureList.Clear();
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (UseExternalGraphicService())
        //        {
        //            signedPDF = await SignExternalPFX(fileContent, signingInfo.Certificate, signingInfo.Reason);
        //        }
        //        else
        //        {
        //            signedPDF = SignInternalPFX(fileContent, signingInfo.Certificate, signingInfo.Reason);
        //        }
        //        if (signedPDF != null)
        //        {
        //            fileContent = signedPDF;
        //        }
        //    }

        //    return fileContent;
        //}


        public Task VerifyCredential(SigningInfo signingInfo)
        {
            throw new NotImplementedException();
        }

        private bool UseExternalGraphicService()
        {
            return _configuration.UseExternalGraphicSignature && !string.IsNullOrWhiteSpace(_configuration.ExternalGraphicSignatureCert) && !string.IsNullOrWhiteSpace(_configuration.ExternalGraphicSignaturePin);
        }

        private async Task<byte[]> SignExternalPFX(byte[] fileContent, X509Certificate2 certificate, string reason, IEnumerable<SignatureField> signatures = null)
        {
            using (var client = _clientFactory.CreateClient())
            {
                try
                {
                    if (signatures == null)
                    {
                        SignPdfDocModel signPdfDocModel = new SignPdfDocModel()
                        {
                            InputFile = Convert.ToBase64String(fileContent),
                            CertID = _configuration.ExternalGraphicSignatureCert,
                            Pincode = _encryptor.Decrypt(_configuration.ExternalGraphicSignaturePin)
                        };
                        string content = JsonConvert.SerializeObject(signPdfDocModel).ToString();
                        StringContent stringContent = new StringContent(content, Encoding.UTF8, MEDIA_TYPE);
                        var serverResponse = await client.PostAsync($"{_configuration.ExternalGraphicSignatureSigner1Url}/SignPDF_PIN", stringContent);
                        if (serverResponse.StatusCode == HttpStatusCode.BadRequest ||
                            serverResponse.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            return SignInternalPFX(fileContent, certificate, reason, signatures);
                        }
                        else
                        {
                            string resposnsString = await serverResponse.Content.ReadAsStringAsync();
                            var result = JsonConvert.DeserializeObject<SignResponse>(resposnsString);
                            return Convert.FromBase64String(result.SignedBytes);
                        }
                    }
                    else
                    {
                        byte[] tempFileContent;
                        foreach (var signature in signatures)
                        {
                            var sigImage = _dataUriScheme.GetBytes(signature.Image);
                            var type = signature.Image?.Split(new char[] { ',' })?.FirstOrDefault();

                            if (type == "data:image/bmp;base64")
                            {
                                sigImage = ConvertToPNG(sigImage);
                            }

                            SignPdfFieldSigner1Model signPdfFieldSigner1Model = new SignPdfFieldSigner1Model()
                            {
                                CertID = _configuration.ExternalGraphicSignatureCert,
                                FieldName = signature.Name,
                                InputFile = Convert.ToBase64String(fileContent),
                                Reason = string.IsNullOrWhiteSpace(reason) ? "ComsignTrust" : reason,
                                Image = Convert.ToBase64String(sigImage),
                                Pincode = _encryptor.Decrypt(_configuration.ExternalGraphicSignaturePin)
                            };
                            string content = JsonConvert.SerializeObject(signPdfFieldSigner1Model).ToString();
                            StringContent stringContent = new StringContent(content, Encoding.UTF8, MEDIA_TYPE);
                            var serverResponse = await client.PostAsync($"{_configuration.ExternalGraphicSignatureSigner1Url}/SignPDF_PIN_FIELD", stringContent);
                            if (serverResponse.StatusCode == HttpStatusCode.BadRequest ||
                                serverResponse.StatusCode == HttpStatusCode.InternalServerError)
                            {
                                tempFileContent = SignInternalPFX(fileContent, certificate, reason, signatures);
                            }
                            else
                            {
                                string resposnsString = await serverResponse.Content.ReadAsStringAsync();
                                var result = JsonConvert.DeserializeObject<SignResponse>(resposnsString);
                                tempFileContent = Convert.FromBase64String(result.SignedBytes);
                            }
                            if(tempFileContent != null)
                            {
                                fileContent = tempFileContent;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to sign using external signer1 ");
                    return SignInternalPFX(fileContent, certificate, reason, signatures);
                }
            }
            // Ensure all code paths return a value
            return fileContent;
        }

        private byte[] SignInternalPFX(byte[] file, X509Certificate2 certificate, string reason, IEnumerable<SignatureField> signatures = null)
        {
            LoadDataBySignatureAppearance(file, certificate, signatures, out PDF_VIS_SIG_APPEARANCE appearance, out PDF_SIG_CONTEXT sigContext);
            var sigMetadata = new PDF_SIG_METADATA()
            {
                Reason = string.IsNullOrWhiteSpace(reason) ? "ComsignTrust" : reason,
                Location = "Israel",
                //Contact = "alexr@comda.co.il",               
                Certification = PDF_CERTIFICATION_LEVEL.NOT_CERTIFIED,
            };

            var cert_Data = new CERT_DATA()
            {
                SignCert = certificate,
                CertChain = new X509Certificate2[] { certificate }
            };

            var cred = new CRED_DATA()
            {
                //Pfx = certificate.Export(X509ContentType.Pfx, PFX_PASSWORD),
                SignInterface = SIGN_INTERFACE.CNG,
                PIN = PFX_PASSWORD
            };


            SIGN_DATA signData = new SIGN_DATA()
            {
                DigestAlg = CTDigestAlg.SHA256,
                RsaSigMode = RsaSigMode.PSS
            };

            byte[] signedHash = null;
            CTResult res = _pdfSign.SignDirect(CTDigestAlg.SHA256, cert_Data, appearance, sigMetadata, sigContext,
                (hash) =>
                {
                    (CTResult result, byte[] localSignedHash) = _iSign.SignCMS(
                    cred, cert_Data, signData, null, hash, null);
                    signedHash = localSignedHash;

                    return new SignHashResult(result, signedHash);
                });


            if (res == CTResult.SUCCESS)
            {
                return sigContext.PDF;
            }
            throw new Exception($"Failed to sign PDD , [{res}]");


        }

        private void LoadDataBySignatureAppearance(byte[] file, X509Certificate2 certificate, IEnumerable<SignatureField> signatures, out PDF_VIS_SIG_APPEARANCE appearance, out PDF_SIG_CONTEXT sigContext)
        {
            if (signatures == null)
            {
                appearance = new PDF_VIS_SIG_APPEARANCE()
                {
                    Visibility = PDF_SIG_VISIBILITY.NOT_VISIBLE,
                    AddSignatureDescription = false,
                    SignerName = certificate.Subject,
                    EnableObsoleteAdobe6Appearance = false
                };
                sigContext = new PDF_SIG_CONTEXT()
                {
                    FieldGeneration = PDF_FIELD_USE.CREATE_NEW_FIELD_DEFAULT_NAME,
                    PDF = file
                };
            }
            else
            {
                var sigImage = _dataUriScheme.GetBytes(signatures.FirstOrDefault().Image);
                var type = signatures.FirstOrDefault().Image?.Split(new char[] { ',' })?.FirstOrDefault();

                if (type == "data:image/bmp;base64")
                {

                    sigImage = ConvertToPNG(sigImage);

                }
                _image.SetImage(sigImage);

                appearance = new PDF_VIS_SIG_APPEARANCE()
                {
                    Visibility = PDF_SIG_VISIBILITY.VISIBLE,
                    Image = _image,
                    AddSignatureDescription = false,
                    SignerName = certificate.Subject,
                    EnableObsoleteAdobe6Appearance = false
                };
                var fieldsNames = signatures.Select(n => n.Name).ToArray();
                sigContext = new PDF_SIG_CONTEXT()
                {
                    FieldGeneration = PDF_FIELD_USE.USE_EXISTING_FIELD,
                    //FieldName = signature.Name,
                    FieldNames = fieldsNames,
                    PDF = file
                };
            }
        }


        private byte[] ConvertToPNG(byte[] image)
        {


            using (MemoryStream memstr = new MemoryStream(image))
            {
                using (var bmp = System.Drawing.Image.FromStream(memstr))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bmp.Save(ms, ImageFormat.Png);
                        return ms.ToArray();
                    }
                }

            }
        }


    }


    public class SignPdfFieldSigner1Model
    {
        public string CertID { get; set; }
        public string InputFile { get; set; }
        public string FieldName { get; set; }
        public string Pincode { get; set; }
        public string Image { get; set; }
        public string Reason { get; set; }
        public string Token { get; set; }
        public string TransactionID { get; set; }
    }

    public class SignPdfDocModel
    {


        public int Page { get; set; } = 0;
        public int Left { get; set; } = 0;
        public int Top { get; set; } = 0;
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;

        public string CertID { get; set; }
        public string InputFile { get; set; }
        public string Pincode { get; set; }
        public string Image { get; set; }
        public string Token { get; set; }
        public string TransactionID { get; set; }
    }

    public class SignResponse
    {
        public Signer1ResCode Result { get; set; }

        public string SignedBytes { get; set; }

    }
}
