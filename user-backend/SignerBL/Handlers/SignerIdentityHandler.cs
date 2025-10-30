using Common.Enums.Documents;
using Common.Enums.Results;
using Common.Enums.Users;
using Common.Extensions;
using Common.Interfaces.DB;
using Common.Interfaces.Oauth;
using Common.Interfaces.SignerApp;
using Common.Models;
using Common.Models.Documents.Signers;
using Common.Models.Documents.SplitSignature;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignerBL.Handlers
{
    public class SignerIdentityHandler : ISignerIdentity
    {
        private readonly ISignersConnector _signersConnector;
        private readonly IVisualIdentity _visualIdentity;
        private readonly ISignerTokenMappingConnector _signerTokenMappingConnector;
        private readonly ILogger _logger;
        private readonly IDocumentCollectionConnector _documentCollectionConnector;
        private readonly ISignerValidator _validator;
        private readonly IOauth _oauth;
        private readonly GeneralSettings _generalSettings;

        public SignerIdentityHandler(ISignersConnector signersConnector, IVisualIdentity visualIdentity, 
            ISignerTokenMappingConnector signerTokenMappingConnector, ILogger logger, IOauth oauth,
            IDocumentCollectionConnector documentCollectionConnector, ISignerValidator validator,
            IOptions<GeneralSettings> generalSettings)
        {
            _signersConnector = signersConnector;
            _visualIdentity = visualIdentity;
            _signerTokenMappingConnector = signerTokenMappingConnector;
            _logger = logger;
            _documentCollectionConnector = documentCollectionConnector;
            _validator = validator;
            _oauth = oauth;
            _generalSettings = generalSettings.Value;

        }

        public async Task<IdentityCreateFlowResult> CreateIdentityFlow(SignerTokenMapping signerTokenMapping)
        {
            
            Guid processToken = signerTokenMapping.GuidToken;
            (Signer signer, Language le) = await ValidateIdentitiyFlow(signerTokenMapping);
            if (signer.IdentificationAttempts == 3)
            {
                throw new InvalidOperationException(ResultCode.VisualIdentityMaximumAttemptsReached.GetNumericString());

            }
            signer.IdentificationAttempts++;
            await _signersConnector.UpdateIdentificationAttempts(signer);
            var startFlowResult = await _visualIdentity.StartVisualIdentityFlow(new IdentityFlow
            {
                SignerToken = processToken
            });

            // case the user using Hebrew 
            if (le == Language.he)
            {
                startFlowResult.Url = startFlowResult.Url.Replace("locale=en-us", "locale=he");
            }
            

            return startFlowResult;
        }
        
        public  Task<SplitDocumentProcess> ProcessAfterSignerAuth(IdentityFlow identityFlow)
        {
            string callBackUrl = $"{_generalSettings.SignerFronendApplicationRoute}/oauth";
            return  _oauth.ProcessAfterSignerAuth(identityFlow, callBackUrl);
        }
        public IdentityCreateFlowResult GetURLForStartAuthForEIdasFlow(SignerTokenMapping signerTokenMapping)
        {
            string callBackUrl = $"{_generalSettings.SignerFronendApplicationRoute}/oauth";
            return new IdentityCreateFlowResult()
            {
                Url = _oauth.GetURLForStartAuthForEIdasFlow(signerTokenMapping, callBackUrl)
            };
        }
        public async Task<IdentityCheckFlowResult> CheckIdentityFlow(SignerTokenMapping signerTokenMapping, string code)
        {
            Guid processToken = signerTokenMapping.GuidToken;
            (Signer signer, _) = await ValidateIdentitiyFlow(signerTokenMapping);

            var startFlowResult = await _visualIdentity.ReadVisualIdentityReqults(new IdentityFlow
            {
                SignerToken = processToken,
                Code = code

            });


            if ((signer?.SignerAuthentication?.OtpDetails?.Identification == startFlowResult.PersonalId ||
                signer?.SignerAuthentication?.OtpDetails?.Identification?.ToLower() == startFlowResult.DocumentNumber?.ToLower()) &&
                startFlowResult.ProcessResult == Common.Enums.Oauth.VisualIdentityProcessResult.Passed)
            {
                signerTokenMapping = await _signerTokenMappingConnector.Read(new SignerTokenMapping { GuidAuthToken = processToken });
                return new IdentityCheckFlowResult { Token = signerTokenMapping.GuidToken };
            }

            if (startFlowResult.ProcessResult != Common.Enums.Oauth.VisualIdentityProcessResult.Passed)
            {
                throw new InvalidOperationException(ResultCode.VisualIdentityOperationFailed.GetNumericString());
            }

            throw new InvalidOperationException(ResultCode.VisualIdentityOperationFailedWrongUser.GetNumericString());

        }


        private async Task<(Signer, Language)> ValidateIdentitiyFlow(SignerTokenMapping signerTokenMapping)
        {
            _logger.Debug("validate Identity flow");
            signerTokenMapping.GuidAuthToken = signerTokenMapping.GuidToken;
            signerTokenMapping.GuidToken = Guid.Empty;
            (Signer signer, Guid documentCollectionId) = await _validator.ValidateSignerToken(signerTokenMapping);

            var documentCollection = new DocumentCollection()
            {
                Id = documentCollectionId
            };
            documentCollection = await _documentCollectionConnector.Read(documentCollection);
            if (documentCollection == null || documentCollection.Id == Guid.Empty || documentCollection.DocumentStatus == DocumentStatus.Declined || documentCollection.DocumentStatus == DocumentStatus.Canceled
                || documentCollection.DocumentStatus == DocumentStatus.Signed || documentCollection.DocumentStatus == DocumentStatus.Deleted)
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }

            signer = documentCollection.Signers.FirstOrDefault(x => x.Id == signer.Id);

            if (signer == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidDocumentCollectionId.GetNumericString());
            }
            if (signer?.SignerAuthentication?.AuthenticationMode != AuthMode.ComsignVisualIDP)
            {
                throw new InvalidOperationException(ResultCode.VisualIdentityNotRequired.GetNumericString());
            }
            return (signer, documentCollection.User?.UserConfiguration?.Language ?? Language.en);
        }

       
    }
}
