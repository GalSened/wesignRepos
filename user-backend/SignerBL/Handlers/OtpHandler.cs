using Common.Enums;
using Common.Enums.Contacts;
using Common.Enums.Documents;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.SignerApp;
using Common.Models;
using Common.Models.Documents.Signers;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Crypto;
using System;
using System.CodeDom;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace SignerBL.Handlers
{
    public class OtpHandler : IOTP
    {
        private readonly ISignerTokenMappingConnector _signerTokenMappingConnector;
        private readonly ISignersConnector _signersConnector;
        private readonly IConfigurationConnector _configurationConnector;
        private readonly ICompanyConnector _companyConnector;
        private readonly IDocumentCollectionConnector _documentCollectionConnector;
        private readonly IJWT _jwt;
        private readonly IDater _dater;
        private readonly ISender _sender;
        private readonly GeneralSettings _generalSettings;
        private readonly IEncryptor _encryptor;
        private const string OTP_ATTEMPTS_EXCEEDED = "OTP submission attempts exceeded";
        private const string PASSWORD_ATTEMPTS_EXCEEDED = "Password submission attempts exceeded";

        public OtpHandler(ISignerTokenMappingConnector signerTokenMappingConnector, IDocumentCollectionConnector documentCollectionConnector, ISignersConnector signersConnector,
            IConfigurationConnector configurationConnector, ICompanyConnector companyConnector, IJWT jwt, IDater dater, ISender sender, IOptions<GeneralSettings> generalSettings, IEncryptor encryptor)
        {
            _signerTokenMappingConnector = signerTokenMappingConnector;
            _signersConnector = signersConnector;
            _configurationConnector = configurationConnector;
            _companyConnector = companyConnector;
            _documentCollectionConnector = documentCollectionConnector;
            _jwt = jwt;
            _dater = dater;
            _sender = sender;
            _generalSettings = generalSettings.Value;
            _encryptor = encryptor;
        }

        public async Task<(string, Guid)> ValidatePassword(Guid token = default, string identification = null, bool incrementAttempts = false)
        {
            var signerTokenMapping = new SignerTokenMapping()
            {
                GuidToken = token
            };

            var dbSignerTokenMapping = await _signerTokenMappingConnector.Read(signerTokenMapping);

            if (dbSignerTokenMapping == null)
            {
                signerTokenMapping = new SignerTokenMapping()
                {
                    GuidAuthToken = token
                };

                dbSignerTokenMapping = await _signerTokenMappingConnector.Read(signerTokenMapping);
            }

            var signer = _jwt.GetSigner(dbSignerTokenMapping?.JwtToken);

            if (signer == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
            }
            var documentCollection = await _documentCollectionConnector.Read(new DocumentCollection { Id = dbSignerTokenMapping.DocumentCollectionId });

            if (documentCollection == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }

            Signer dbSigner = documentCollection?.Signers.FirstOrDefault(x => x.Id == signer.Id);

            if (dbSigner.SignerAuthentication == null)
            {
                throw new InvalidOperationException(ResultCode.SignerAuthDoesntExist.GetNumericString());
            }

            OtpDetails otpDetails = dbSigner.SignerAuthentication.OtpDetails;

            bool isCorrectPassword = !string.IsNullOrWhiteSpace(identification) && otpDetails.Identification == identification;

            bool isPasswordMode = otpDetails.Mode == OtpMode.CodeAndPasswordRequired || dbSigner.SignerAuthentication.OtpDetails.Mode == OtpMode.PasswordRequired;

            if (incrementAttempts && isPasswordMode && !isCorrectPassword)
            {
                if (otpDetails.Attempts >= 3)
                {
                    documentCollection.DocumentStatus = DocumentStatus.Declined;
                    await _documentCollectionConnector.Update(documentCollection);

                    dbSigner.Status = SignerStatus.Rejected;
                    await _signersConnector.UpdateSignerStatus(dbSigner);
                    await _signersConnector.UpdateSignerNotes(dbSigner.Id, PASSWORD_ATTEMPTS_EXCEEDED);
                    throw new InvalidOperationException(ResultCode.PasswordSubmissionLimitExceeded.GetNumericString());
                }

                int attempts = dbSigner.SignerAuthentication.OtpDetails.Attempts + 1;
                await _signersConnector.UpdateOtpAttempts(dbSigner.Id, attempts);

                throw new InvalidOperationException(ResultCode.InvalidCredential.GetNumericString());
            }

            if (otpDetails.Mode == OtpMode.None)
            {
                throw new InvalidOperationException(ResultCode.InvalidUserAuthenticationMode.GetNumericString());
            }

            else if (otpDetails.Mode == OtpMode.PasswordRequired)
            {
                return (string.Empty, dbSignerTokenMapping.GuidToken);
            }

            GenerateInternalCode(documentCollection, dbSigner);
            Common.Models.Configurations.Configuration appConfiguration = await _configurationConnector.Read();

            Company company = new Company
            {
                Id = documentCollection.User.CompanyId
            };

            var dbCompany = await _companyConnector.Read(company);

            string sentSignerMeans = await _sender.SendOtpCode(appConfiguration, dbSigner, documentCollection.User, dbCompany);

            return (sentSignerMeans, dbSignerTokenMapping.GuidToken);
        }

        public async Task<(bool, Guid)> IsValidCode(string code, Guid token = default, bool incrementAttempts = false)
        {
            var signerTokenMapping = new SignerTokenMapping()
            {
                GuidToken = token
            };

            var dbSignerTokenMapping = await _signerTokenMappingConnector.Read(signerTokenMapping);

            if (dbSignerTokenMapping == null)
            {
                signerTokenMapping = new SignerTokenMapping()
                {
                    GuidAuthToken = token
                };

                dbSignerTokenMapping = await _signerTokenMappingConnector.Read(signerTokenMapping);
            }

            Signer signer = _jwt.GetSigner(dbSignerTokenMapping?.JwtToken);

            if (signer == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
            }

            var docCollection = new DocumentCollection
            {
                Id = dbSignerTokenMapping.DocumentCollectionId
            };

            var documentCollection = await _documentCollectionConnector.Read(docCollection);

            if (documentCollection == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }

            Signer dbSigner = documentCollection.Signers.FirstOrDefault(x => x.Id == signer.Id);
            OtpDetails otpDetails = dbSigner?.SignerAuthentication?.OtpDetails;

            bool isExpired = otpDetails.ExpirationTime <= _dater.UtcNow();
            bool isOTPCodeValid = otpDetails?.Code == code.Trim();
            bool success = isOTPCodeValid && !isExpired;

            if (!success && incrementAttempts)
            {
                int attempts = dbSigner.SignerAuthentication.OtpDetails.Attempts + 1;
                await _signersConnector.UpdateOtpAttempts(dbSigner.Id, attempts);

                if (otpDetails.Attempts >= 3)
                {
                    documentCollection.DocumentStatus = DocumentStatus.Declined;
                    await _documentCollectionConnector.Update(documentCollection);

                    dbSigner.Status = SignerStatus.Rejected;
                    await _signersConnector.UpdateSignerStatus(dbSigner);
                    await _signersConnector.UpdateSignerNotes(dbSigner.Id, OTP_ATTEMPTS_EXCEEDED);
                    throw new InvalidOperationException(ResultCode.OTPSubmissionLimitExceeded.GetNumericString());
                }
            }

            return (success, dbSignerTokenMapping.GuidToken);
        }

        public string GenerateCode(Guid token = default, string identification = null)
        {
            throw new NotImplementedException();
        }

        #region Private Functions

        private void GenerateInternalCode(DocumentCollection documentCollection, Signer signer)
        {
            Random generator = new Random();
            string code = generator.Next(0, 999999).ToString("D6");
            var dbSigner = documentCollection.Signers.FirstOrDefault(x => x.Id == signer.Id);

            if (dbSigner.SignerAuthentication == null || dbSigner.SignerAuthentication.OtpDetails == null)
            {
                throw new Exception($"There is no signer authentication or otp details for signer [{dbSigner.Id}]");
            }

            dbSigner.SignerAuthentication.OtpDetails.Code = code;
            dbSigner.SignerAuthentication.OtpDetails.ExpirationTime = _dater.UtcNow().AddMinutes(_generalSettings.OtpCodeExpirationMinuteTime);
            _signersConnector.UpdateGeneratedOtpDetails(dbSigner);
        }

        #endregion
    }
}