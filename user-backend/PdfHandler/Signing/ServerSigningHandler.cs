using Common.Consts;
using Common.Enums;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.PDF;
using Common.Models.Configurations;
using Common.Models.Files.PDF;
using SignatureServiceConnector;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace PdfHandler.Signing
{
    class ServerSigningHandler : ISigning
    {

        
        private readonly IDataUriScheme _dataUriScheme;
        private readonly ISignConnector _signConnector;
        private readonly IEncryptor _encryptor;

        public ServerSigningHandler( IDataUriScheme dataUriScheme, ISignConnector signConnector,
            IEncryptor encryptor)
        {
            
            _dataUriScheme = dataUriScheme;
            _signConnector = signConnector;

            _encryptor = encryptor;
        }

        public async Task VerifyCredential(SigningInfo signingInfo)
        {
            string token = signingInfo.SignerAuthentication?.Signer1Credential?.SignerToken;
            if (!signingInfo.SignerAuthentication?.Signer1Credential?.ShouldUseADDetails ?? true)
            {

                if (!string.IsNullOrWhiteSpace(signingInfo.SignerAuthentication.Signer1Credential?.SignerToken) &&
                (string.IsNullOrWhiteSpace(signingInfo.SignerAuthentication?.Signer1Credential.Password) ||
                string.IsNullOrWhiteSpace(signingInfo.SignerAuthentication?.Signer1Credential.CertificateId)))
                {
                    string deccryptData = _encryptor.Decrypt(signingInfo.SignerAuthentication.Signer1Credential?.SignerToken ?? String.Empty);
                    if (deccryptData.Contains($"{Consts.SAML_SEPARATOR}"))
                    {
                        var splitString = deccryptData.Split(new string[] { $"{Consts.SAML_SEPARATOR}" }, StringSplitOptions.RemoveEmptyEntries);
                        //memshal zamin example - SignerToken = authToken then separator then SMAL 
                        if (splitString.Length > 1 && !string.IsNullOrWhiteSpace(splitString[0]))
                        {
                            signingInfo.SignerAuthentication.Signer1Credential.CertificateId = splitString[0];
                            token = splitString[1];
                        }
                        //leumit example - SignerToken = active directory name
                        else if (splitString.Length == 1 && !string.IsNullOrWhiteSpace(splitString[0]))
                        {
                            signingInfo.SignerAuthentication.Signer1Credential.CertificateId = splitString[0];
                        }
                    }
                }

                var response = await _signConnector.VerifyCredential(signingInfo.SignerAuthentication?.Signer1Credential?.CertificateId,
                    signingInfo.SignerAuthentication?.Signer1Credential?.Password, token, 
                    signingInfo.CompanySigner1Details);

                if (response != Signer1ResCode.SUCCESS)
                {                
                    throw new InvalidOperationException(ResultCode.InvalidCredential.GetNumericString(),
                                                        new Exception($"Wrong credential - {response})"));
                }
            }
        }

        public async Task<byte[]> Sign(SigningInfo signingInfo, bool useForAllFields = false)
        {
            byte[] fileContent = null;
            string certId = signingInfo.SignerAuthentication.Signer1Credential?.CertificateId;
            string token = "";
            if (!string.IsNullOrWhiteSpace(signingInfo.SignerAuthentication.Signer1Credential?.SignerToken) &&
                (string.IsNullOrWhiteSpace(signingInfo.SignerAuthentication?.Signer1Credential.Password) || string.IsNullOrWhiteSpace(certId)))
            {
                string deccryptData = _encryptor.Decrypt(signingInfo.SignerAuthentication.Signer1Credential?.SignerToken ?? String.Empty);
                if (deccryptData.Contains($"{Consts.SAML_SEPARATOR}"))
                {
                    var splitString = deccryptData.Split(new string[] { $"{Consts.SAML_SEPARATOR}" }, StringSplitOptions.RemoveEmptyEntries);
                    //memshal zamin example - SignerToken = authToken then separator then SMAL 
                    if (splitString.Length > 1 && !string.IsNullOrWhiteSpace(splitString[0]))
                    {
                        certId = signingInfo.SignerAuthentication.Signer1Credential.CertificateId = splitString[0];
                        token = splitString[1];
                    }
                    //leumit example - SignerToken = active directory name
                    else if (splitString.Length == 1 && !string.IsNullOrWhiteSpace(splitString[0]))
                    {
                        certId = signingInfo.SignerAuthentication.Signer1Credential.CertificateId = splitString[0];
                    }
                }
            }
            await VerifyCredential(signingInfo);
            fileContent = signingInfo.Data;
            foreach (var signature in signingInfo.Signatures ?? Enumerable.Empty<SignatureField>())
            {
                byte[] imageBytes = _dataUriScheme.GetBytes(signature.Image);
                var result = await _signConnector.SignPdfField(certId, fileContent,
                    signature.Name, signingInfo.SignerAuthentication?.Signer1Credential.Password, imageBytes, token, signingInfo.CompanySigner1Details);
                if (result.ResultCode != Signer1ResCode.SUCCESS)
                {
                    throw new InvalidOperationException(ResultCode.SignOperationFailed.GetNumericString(),
                                                        new Exception($"Sign using signer1 failed - {result.ResultCode})"));
                }
                fileContent = result.SignedBytes;
            }

            if (signingInfo.Signatures != null && signingInfo.Signatures.Any())
            {
                return fileContent;
            }
            else
            {
                var result =await _signConnector.SignPdf(certId, fileContent, signingInfo.SignerAuthentication?.Signer1Credential.Password, token, signingInfo.CompanySigner1Details);
                if (result.ResultCode != Signer1ResCode.SUCCESS)
                {
                    throw new InvalidOperationException(ResultCode.SignOperationFailed.GetNumericString(),
                                                        new Exception($"Sign using signer1 failed - {result.ResultCode})"));
                }
                return result.SignedBytes;

            }
        }
    }
}
