// Ignore Spelling: app

using Common.Enums;
using Common.Enums.Contacts;
using Common.Enums.Documents;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents.Signers;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Handlers
{
    public class GenerateLinkHandler : IGenerateLinkHandler
    {
        private readonly GeneralSettings _generalSettings;
      
        private readonly IJWT _jwt;
        private readonly IConfiguration _configuration;
        private readonly IConfigurationConnector _configurationConnector;
        private readonly ISignerTokenMappingConnector _signerTokenMappingConnector;

        public GenerateLinkHandler(IConfigurationConnector configurationConnector, ISignerTokenMappingConnector signerTokenMappingConnector, IConfiguration configuration, IJWT jwt, IOptions<GeneralSettings> generalSettings)
        {
            _generalSettings = generalSettings.Value;      
            _jwt = jwt;
            _configuration = configuration;
            _configurationConnector = configurationConnector;
            _signerTokenMappingConnector = signerTokenMappingConnector;


        }

        public async Task<SignerLink> GenerateDocumentDownloadLink(DocumentCollection documentCollection, Signer signer, User user, CompanyConfiguration companyConfiguration)
        {
            if (signer == null)
            {
                throw new Exception($"Null input - signer is null");
            }
            if (documentCollection == null)
            {
                throw new Exception($"Null input - documentCollection is null");
            }
            int expirationTimeInHours = _configuration.GetSignerLinkExperationTimeInHours(user, companyConfiguration);
            
            var signerLink = await GetSignerLink(documentCollection, expirationTimeInHours, signer, $"{_generalSettings.SignerFronendApplicationRoute}/download", shouldUseSignerAuth: false);

            return signerLink;
        }

        public async Task<IEnumerable<SignerLink>> GenerateSigningLink(DocumentCollection documentCollection, User user, CompanyConfiguration companyConfiguration, bool shouldGenerateNewGuid = true)
        {
            var signerLinks = new List<SignerLink>();
            int expirationTimeInHours = _configuration.GetSignerLinkExperationTimeInHours(user, companyConfiguration);
            var appConfiguration =await _configurationConnector.Read();

            foreach (var signer in documentCollection?.Signers ?? Enumerable.Empty<Signer>())
            {
                SignerLink signerLink = await GenerateSigningLinkToSingleSigner(documentCollection, shouldGenerateNewGuid, expirationTimeInHours, appConfiguration, signer);
                signerLinks.Add(signerLink);
                if (documentCollection.Mode == DocumentMode.Online)
                {
                    SignerLink senderLink = new SignerLink
                    {
                        SignerId = signerLink.SignerId,
                      
                    };
                    var link = ReplaceLastOccurrence(signerLink.Link, "signature", "sender");
                    senderLink.Link = link;
                    signerLinks.Add(senderLink);
                }
            }
            return signerLinks;
        }

        public  string ReplaceLastOccurrence(string link, string find, string replaceWith)
        {
            if(string.IsNullOrWhiteSpace(link))
            {
                return link;
            }
            int wordLocation = link.LastIndexOf(find);

            if (wordLocation == -1)
                return link;

            string result = link.Remove(wordLocation, find.Length).Insert(wordLocation, replaceWith);
            return result;
        }

        public Task<SignerLink> GenerateSigningLinkToSingleSigner(DocumentCollection documentCollection, bool shouldGenerateNewGuid, int expirationTimeInHours, Configuration appConfiguration, Signer signer)
        {
            if (signer == null)
            {
                throw new Exception($"Null input - signer is null");
            }
            if (documentCollection == null)
            {
                throw new Exception($"Null input - documentCollection is null");
            }
            if (documentCollection.Signers.FirstOrDefault(x => x.Id == signer.Id) == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidSignerId.GetNumericString());
            }
            //if (signer.Status != SignerStatus.Sent && signer.Status != SignerStatus.Viewed)
            //{
            //    throw new InvalidOperationException(ResultCode.CannotCreateSigningLinkToSignerThatSignedOrDecline.GetNumericString());

            //}
            bool shouldUseSignerAuth = appConfiguration.ShouldUseSignerAuth && signer.SignerAuthentication?.AuthenticationMode == AuthMode.IDP;
            string baseRoute = shouldUseSignerAuth ? _generalSettings.AuthSignerFronendApplicationRoute : _generalSettings.SignerFronendApplicationRoute;
            string route = $"{baseRoute}/signature";
            return  GetSignerLink(documentCollection, expirationTimeInHours, signer, route, shouldUseSignerAuth, shouldGenerateNewGuid);

        }

        private async Task<SignerLink> GetSignerLink(DocumentCollection documentCollection, int expirationTimeInHours, Signer signer, string route, bool shouldUseSignerAuth, bool shouldGenerateNewGuid = true)
        {
            signer.LinkExpirationInHours = signer?.LinkExpirationInHours > 0 ? signer.LinkExpirationInHours : expirationTimeInHours;
            var signerTokenMapping = new SignerTokenMapping()
            {
                DocumentCollectionId = documentCollection.Id,
                SignerId = signer.Id
            };
            if (shouldGenerateNewGuid)
            {
                signerTokenMapping.GuidToken = Guid.NewGuid();
                signerTokenMapping.JwtToken = _jwt.GenerateSignerToken(signer, signer.LinkExpirationInHours);
            }
            bool requireOTPOrCodeFromSigner = signer?.SignerAuthentication?.OtpDetails != null && (signer?.SignerAuthentication?.OtpDetails?.Mode != OtpMode.None);

            bool requireVisualIdentityFlow = signer?.SignerAuthentication?.AuthenticationMode == AuthMode.ComsignVisualIDP;


            if ((shouldUseSignerAuth || requireOTPOrCodeFromSigner || requireVisualIdentityFlow))
            {
                signerTokenMapping.GuidAuthToken = Guid.NewGuid();
            }
            
            SignerTokenMapping oldSignerTokenMapping = await _signerTokenMappingConnector.Read(signerTokenMapping);
            if (shouldGenerateNewGuid)
            {
                await _signerTokenMappingConnector.Delete(signerTokenMapping);
                try
                {
                    await _signerTokenMappingConnector.Create(signerTokenMapping);
                }
                //Roll back in case deletion work but creation failed.
                //We will return old value to db
                catch
                {
                    await _signerTokenMappingConnector.Create(oldSignerTokenMapping);
                    throw ;
                }
            }
            else if(shouldUseSignerAuth || requireOTPOrCodeFromSigner || requireVisualIdentityFlow)
            {
                signerTokenMapping.GuidAuthToken = oldSignerTokenMapping?.GuidAuthToken ?? signerTokenMapping.GuidAuthToken;
            }
            
            var guid = (shouldUseSignerAuth  || requireOTPOrCodeFromSigner  || requireVisualIdentityFlow) ? signerTokenMapping.GuidAuthToken : shouldGenerateNewGuid? signerTokenMapping.GuidToken : oldSignerTokenMapping?.GuidToken;
             var signerLink = new SignerLink()
            {
                SignerId = signer.Id,
                Link = $"{route}/{guid}"
            };
            return signerLink;
        }
    }
}
