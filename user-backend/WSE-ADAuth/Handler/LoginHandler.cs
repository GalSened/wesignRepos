using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Models.Documents.Signers;
using Common.Models.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WSE_ADAuth.Models;
using Serilog;
using IO.ClickSend.ClickSend.Api;
using Common.Models.Users;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using System.Text;
using Comda.Authentication.Models;
using Microsoft.EntityFrameworkCore.Query;
using DAL.Connectors;
using Twilio.TwiML.Voice;

namespace WSE_ADAuth.Handler
{
    public class LoginHandler : ILoginHandler
    {
        private readonly GeneralSettings _generalSettings;
        private readonly IUserConnector _userConnector;
        private readonly IOneTimeTokens _oneTimeTokens;
        private readonly ADGeneralSettings _adGeneralSettings;
        private readonly ISignerTokenMappingConnector _signerTokenMappingConnector;
        private readonly IDocumentCollectionConnector _documentCollectionConnector;
        private readonly ILogger _logger;
        private readonly IJWT _jwt;
        private readonly AutoUserCreatingSettings _autoUserCreatingSettings;
        private readonly INewUserGenerator _newUserGenerator;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IEncryptor _encryptor;
        private readonly SAMLGeneralSettings _samlGeneralSettings;
        private const string MEDIA_TYPE = "application/json";

        public LoginHandler(IOptions<GeneralSettings> generalSettings, IJWT jwt, IUserConnector userConnector, ILogger logger, IHttpClientFactory clientFactory,
             IOneTimeTokens oneTimeTokens,  IOptions<ADGeneralSettings> adGeneralSettings, IOptions<AutoUserCreatingSettings> autoUserCreatingSettings,
             INewUserGenerator newUserGenerator, IEncryptor encryptor,ISignerTokenMappingConnector signerTokenMappingConnector, IOptions<SAMLGeneralSettings> samlGeneralSettings,
             IDocumentCollectionConnector documentCollectionConnector
              )
        {
            _generalSettings = generalSettings.Value;
            _userConnector = userConnector;
            _oneTimeTokens = oneTimeTokens;
            _adGeneralSettings = adGeneralSettings.Value;
            _signerTokenMappingConnector = signerTokenMappingConnector;
            _documentCollectionConnector = documentCollectionConnector;
            _logger = logger;
            _jwt = jwt;
            _autoUserCreatingSettings = autoUserCreatingSettings.Value;
            _newUserGenerator = newUserGenerator;
            _clientFactory = clientFactory;
            _encryptor = encryptor;
            _samlGeneralSettings = samlGeneralSettings.Value;
        }


        public async Task<string> DoDirectAuthSignerSamlHostedAppLogin(SignerLoginModel signerLoginModel)
        {
            var signerTokenMapping = new SignerTokenMapping()
            {
                GuidAuthToken = signerLoginModel.GuidAuthToken
            };
            _logger.Debug("reading from signer mapper");
            var dbSignerTokenMapping = await _signerTokenMappingConnector.Read(signerTokenMapping);

            if (dbSignerTokenMapping == null)
            {
                signerTokenMapping = new SignerTokenMapping()
                {
                    GuidToken = signerLoginModel.GuidAuthToken
                };
                dbSignerTokenMapping = await _signerTokenMappingConnector.Read(signerTokenMapping);
            }

            var user = await _userConnector.Read(new Common.Models.User { Username = signerLoginModel.UserName });


            if(user == null)
            {
                _logger.Warning("user with details {SignerLoginModelUserName} not found can't authenticate signer", signerLoginModel.UserName);                
            }
            bool theSignerIsAuthenticated = user != null;


            _logger.Debug("reading signer from token {DbSignerToken}", dbSignerTokenMapping?.JwtToken);
            var signer = _jwt.GetSigner(dbSignerTokenMapping?.JwtToken);
            if (signer != null)
            {
                _logger.Debug("signer {SignerId} found in token", signer.Id);
                var documentCollection = await _documentCollectionConnector.Read(new Common.Models.DocumentCollection { Id = dbSignerTokenMapping.DocumentCollectionId });
                var dbSigner = documentCollection.Signers.FirstOrDefault(x => x.Id == signer.Id);
                if(!theSignerIsAuthenticated || dbSigner.Contact.Email.ToLower() != user?.Email?.ToLower())
                {
                    
                    _logger.Warning("user with details {SignerLoginModelUserName} not the same as the signer that need to sign signer not authenticate.", signerLoginModel.UserName);
                    // info to the 

                }
                else
                {
                    dbSignerTokenMapping.ADName = _encryptor.Encrypt(signerLoginModel.UserName);
                }
                dbSignerTokenMapping.AuthId = _encryptor.Encrypt(signerLoginModel.UserName);
                dbSignerTokenMapping.AuthName = signerLoginModel.AuthName;
                dbSignerTokenMapping.AuthToken = _encryptor.Encrypt(signerLoginModel.AuthToken);
                await _signerTokenMappingConnector.Update(dbSignerTokenMapping);
                return $@"{_generalSettings.SignerFronendApplicationRoute}/signature/{dbSignerTokenMapping.GuidToken}";
            }



            return $@"{_generalSettings.SignerFronendApplicationRoute}";
        }
        public async Task<UserTokens> DoDirectExternalLogin(string token)
        {
            if(string.IsNullOrWhiteSpace(token))
            {
                return null;
            }
            UserTokens userTokens = new UserTokens()
            {
                RefreshToken = token
            };
            var user = await _oneTimeTokens.CheckRemoteLoginToken(userTokens);
            if (user == null)
            {
                return null;
            }
            await _oneTimeTokens.GenerateRefreshToken(user);
            return new UserTokens()
            {
                JwtToken = _jwt.GenerateToken(user),
                RefreshToken = await _oneTimeTokens.GetRefreshToken(user),
                AuthToken = user.UserTokens?.AuthToken ?? ""
            };

        }
        public async Task<string> DoLoginToClientFrontEnd(LoginToClient loginToClient)
        {
            _logger.Debug("Start auth client login {UserEmail} ", loginToClient.UserEmail);

            if (!string.IsNullOrWhiteSpace(loginToClient.UserEmail))
            {
                //need to change the app working with user Username
                var user = await _userConnector.Read(new Common.Models.User() { Email = loginToClient.UserEmail,Username = loginToClient.UserEmail });
                if(user == null && _autoUserCreatingSettings != null && _autoUserCreatingSettings.Active)
                {
                    _logger.Debug(" auth client login {UserEmail} user not exist creating new one ", loginToClient.UserEmail);
                    user = await _newUserGenerator.CreateNewUser(loginToClient);
                    
                }
                
                if (user != null && user.Status == Common.Enums.Users.UserStatus.Activated)
                {
                    if(loginToClient.LoginSource == LoginSource.SAML &&
                        _samlGeneralSettings.ApprovedCompanies != null && _samlGeneralSettings.ApprovedCompanies.Any())
                    {
                        if(!_samlGeneralSettings.ApprovedCompanies.Exists(x => x == user.CompanyId) )
                        {
                            _logger.Debug(" client login {UserEmail}  not in the approved companies - SSO is not Approved", loginToClient.UserEmail);
                            return string.Empty;
                        }

                    }


                    _logger.Debug(" auth client login {UserEmail} user exist and active", loginToClient.UserEmail);

                    string token = await _oneTimeTokens.GenerateRemoteLoginToken(user, loginToClient.UserAuthToken,
                        _adGeneralSettings.UserKeyValidationInSeconds);
                    return token;
                }
                else
                {
                    _logger.Debug("user : {UserEmail} is don't have an active account in WeSign", loginToClient.UserEmail);
                }

            }
            _logger.Debug("auth client login, user information don't have a login key");
            return "";
        }


        public async Task<string> DoAuthToSigner(SignerLoginModel signerLoginModel)
        {
            _logger.Debug("Start auth signer");
            var signerTokenMapping = new SignerTokenMapping()
            {
                GuidAuthToken = signerLoginModel.GuidAuthToken
            };
            _logger.Debug("reading from signer mapper");
            var dbSignerTokenMapping = await _signerTokenMappingConnector.Read(signerTokenMapping);
            _logger.Debug("reading signer from token {DbSignerToken}", dbSignerTokenMapping?.JwtToken);
            var signer = _jwt.GetSigner(dbSignerTokenMapping?.JwtToken);
            if (signer != null)
            {
                _logger.Debug("signer {SignerId} found in token", signer.Id);
                var documentCollection = await _documentCollectionConnector.Read(new Common.Models.DocumentCollection { Id = dbSignerTokenMapping.DocumentCollectionId });
                var dbSigner = documentCollection.Signers.FirstOrDefault(x => x.Id == signer.Id);

                if ((dbSigner.SendingMethod == Common.Enums.Documents.SendingMethod.Email && dbSigner.Contact.Email.ToLower() == signerLoginModel.Email.ToLower()) ||
                    (dbSigner.SendingMethod == Common.Enums.Documents.SendingMethod.SMS && !string.IsNullOrWhiteSpace(dbSigner.Contact.Phone.ToLower()) &&
                    signerLoginModel.PhoneNumbers.Contains(dbSigner.Contact.Phone)))
                {
                    if (!string.IsNullOrWhiteSpace(signerLoginModel.UserName))
                    {
                        dbSignerTokenMapping.ADName = signerLoginModel.UserName;
                        await _signerTokenMappingConnector.Update(dbSignerTokenMapping);
                    }

                    return $@"{_generalSettings.SignerFronendApplicationRoute}/signature/{dbSignerTokenMapping.GuidToken}";
                }
                else
                {
                    _logger.Debug("failed to auth signer {SignerId} for document {DocumentCollectionId}", signer.Id, documentCollection.Id);
                }
                
                
            }
            else
            {
                _logger.Debug("can't find signer in request");
            }

            return $@"{_generalSettings.SignerFronendApplicationRoute}";
        }
    }
}
