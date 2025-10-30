using Common.Enums;
using Common.Interfaces;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using ServerSignatureService;
using System;
using System.Net;
using System.Threading.Tasks;
using IUsers = Common.Interfaces.IUsers;

namespace SignatureServiceConnector
{
    public class SignConnector : ISignConnector
    {
        private readonly IConfiguration _configuration;
        private readonly IEncryptor _encryptor;
        

        public SignConnector(IConfiguration configuration, IEncryptor encryptor)
        {
            _configuration = configuration;
            _encryptor = encryptor;
            
            // ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            // ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => { return true; };
        }

        public string Signer1Endpoint { get; }

        private async Task<SignServiceClient> GetSigner1ClientConnector(CompanySigner1Details companySigner1Details)
        {
            var signer1Configuration = await _configuration.GetSigner1Configuration(companySigner1Details);
            var endpointConfiguration = GetEndpointConfig(signer1Configuration);
            var signServiceClient = new SignServiceClient(endpointConfiguration, signer1Configuration.Endpoint);
            AddAuth(signServiceClient, signer1Configuration);
            return signServiceClient;
        }

        private void AddAuth(SignServiceClient signServiceClient, Signer1Configuration signer1Configuration)
        {
            if(!string.IsNullOrWhiteSpace(signer1Configuration.User) && !string.IsNullOrWhiteSpace(signer1Configuration.Password))
            {
                signServiceClient.ClientCredentials.UserName.UserName = signer1Configuration.User;
                string password = _encryptor.Decrypt(signer1Configuration.Password);
                signServiceClient.ClientCredentials.UserName.Password = password;
            }
        }

       

        public async Task<Signer1ResCode> VerifyCredential(string certId , string pinCode,string token, CompanySigner1Details companySigner1Details = null)
        {
            Signer1Configuration signer1Configuration =await _configuration.GetSigner1Configuration(companySigner1Details);
            var endpointConfiguration = GetEndpointConfig(signer1Configuration);
            var signServiceClient = new SignServiceClient(endpointConfiguration, signer1Configuration.Endpoint);
            AddAuth(signServiceClient, signer1Configuration);
            Signer1ResCode result = Signer1ResCode.INPUT_ERROR;
            try
            {
                var response = await signServiceClient.Cred_VerifyAsync(certId, pinCode, "");
                result = (Signer1ResCode)((int)response.Body.Cred_VerifyResult);
            }
            catch (Exception ex)
            {
                if (ex.Message != "Invalid Input")
                {
                    throw ;
                }
            }

            return result;
        }

        private  SignServiceClient.EndpointConfiguration GetEndpointConfig(Signer1Configuration signer1Configuration)
        {
            if (string.IsNullOrWhiteSpace(signer1Configuration.Endpoint))
            {
                throw new Exception("Signer1 Endpoint configuration is empty, please check your configuration");
            }
            var uri = new Uri(signer1Configuration.Endpoint);
            var requestType = uri.Scheme;
            if (requestType.Contains("s"))
            {
                if (!string.IsNullOrWhiteSpace(signer1Configuration.User) && !string.IsNullOrWhiteSpace(signer1Configuration.Password))
                {
                    return SignServiceClient.EndpointConfiguration.BasicHttpBinding_ISignService_BasicAuth;
                }
                return SignServiceClient.EndpointConfiguration.BasicHttpBinding_ISignService;
            }
            return SignServiceClient.EndpointConfiguration.BasicHttpBinding_ISignService1;
        }

        public async Task<(Signer1ResCode ResultCode, byte[] SignedBytes)> SignPdfField(string certId, byte[] inputFile, string fieldName, string pincode, byte[] image, string token, CompanySigner1Details companySigner1Details)
        {
            
            SignServiceClient signServiceClient = await GetSigner1ClientConnector(companySigner1Details);
            var response = await signServiceClient.SignPDF_PIN_FIELDAsync(certId, inputFile, fieldName, pincode, image, token);

            Signer1ResCode signer1ResCode = (Signer1ResCode)((int)response?.Body?.SignPDF_PIN_FIELDResult.Result);
            byte[] signedBytes = response?.Body?.SignPDF_PIN_FIELDResult.SignedBytes;
            return (signer1ResCode, signedBytes);
        }

        public async Task<(Signer1ResCode ResultCode, byte[] SignedBytes)> SignPdf(string certId, byte[] inputFile, string pincode, string token, CompanySigner1Details companySigner1Details)
        {
            SignServiceClient signServiceClient = await GetSigner1ClientConnector(companySigner1Details);
            byte[] defaultImage = null;
            var response = await signServiceClient.SignPDF_PINAsync(certId, inputFile, Page: 1, Left: 0, Top: 0, Width: 100, Height: 100, pincode, defaultImage, token);
            Signer1ResCode signer1ResCode = (Signer1ResCode)((int)response?.Body?.SignPDF_PINResult.Result);
            byte[] signedBytes = response?.Body?.SignPDF_PINResult.SignedBytes;
            return (signer1ResCode, signedBytes);
        }

        public async Task<(Signer1ResCode ResultCode, byte[] SignedBytes)> SignXML(string certId, byte[] inputFile, string pincode, string token, CompanySigner1Details companySigner1Details)
        {
            SignServiceClient signServiceClient =await GetSigner1ClientConnector(companySigner1Details);
            var response = await signServiceClient.SignXml_PINAsync(certId, inputFile, pincode, token);
            Signer1ResCode signer1ResCode = (Signer1ResCode)((int)response?.Body?.SignXml_PINResult.Result);
            byte[] signedBytes = response?.Body?.SignXml_PINResult.SignedBytes;
            return (signer1ResCode, signedBytes);
        }
    
        public async Task<(Signer1ResCode ResultCode, byte[] SignedBytes)> SignWord(string certId, byte[] inputFile, string pincode, string token, CompanySigner1Details companySigner1Details)
        {
            SignServiceClient signServiceClient = await GetSigner1ClientConnector(companySigner1Details);
            var response = await signServiceClient.SignWord_PINAsync(certId, inputFile, pincode, "", token);
            Signer1ResCode signer1ResCode = (Signer1ResCode)((int)response?.Body?.SignWord_PINResult.Result);
            byte[] signedBytes = response?.Body?.SignWord_PINResult.SignedBytes;
            return (signer1ResCode, signedBytes);
        }

        public async Task<(Signer1ResCode ResultCode, byte[] SignedBytes)> SignExcel(string certId, byte[] inputFile, string pincode, string token, CompanySigner1Details companySigner1Details)
        {
            SignServiceClient signServiceClient = await GetSigner1ClientConnector(companySigner1Details);
            var response = await signServiceClient.SignExcel_PINAsync(certId, inputFile, pincode, "", token);
            Signer1ResCode signer1ResCode = (Signer1ResCode)((int)response?.Body?.SignExcel_PINResult.Result);
            byte[] signedBytes = response?.Body?.SignExcel_PINResult.SignedBytes;
            return (signer1ResCode, signedBytes);
        }

        //private Signer1Configuration getSigner1Configuration()
        //{

        //}

        //public (Signer1ResCode ResultCode, byte[] SignedBytes) SignTiff(string certId, byte[] inputFile, string pincode, byte[] image, string token)
        //{
        //    SignServiceClient signServiceClient = GetSigner1ClientConnector();
        //    var response = signServiceClient.SignTiff_PINAsync(certId, inputFile, pincode, "", token).GetAwaiter().GetResult();
        //    Signer1ResCode signer1ResCode = (Signer1ResCode)((int)response?.Body?.SignExcel_PINResult.Result);
        //    byte[] signedBytes = response?.Body?.SignExcel_PINResult.SignedBytes;
        //    return (signer1ResCode, signedBytes);
        //}

        //public (Signer1ResCode ResultCode, byte[] SignedBytes) SignXMLForeclosure(string certId, byte[] inputFile, string pincode, string token)
        //{
        //    SignServiceClient signServiceClient = GetSigner1ClientConnector();
        //    var response = signServiceClient.SignForeclosureAsync(certId, pincode, inputFile,  token).GetAwaiter().GetResult();
        //    Signer1ResCode signer1ResCode = (Signer1ResCode)((int)response?.Body?.SignForeclosureResult.Result);
        //    byte[] signedBytes = response?.Body?.SignForeclosureResult.SignedBytes;
        //    return (signer1ResCode, signedBytes);

        //}
    }


}
