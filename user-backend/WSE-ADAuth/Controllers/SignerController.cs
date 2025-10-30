using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Microsoft.AspNetCore.Mvc;
using WSE_ADAuth.Models;
using Microsoft.Extensions.Options;
using Common.Interfaces.DB;
using Common.Interfaces;
using Common.Models.Documents.Signers;
using Common.Models.Settings;
using WSE_ADAuth.AD;
using WSE_ADAuth.Handler;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using Common.Models;

namespace WSE_ADAuth.Controllers
{
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class SignerController : Controller
    {
        private ILogger _logger;
        private ADGeneralSettings _adGeneralSettings;
        
        private IJWT _jwt;
        private IAD _adHandler;
        private readonly GeneralSettings _generalSettings;
        private readonly ILoginHandler _loginHandler;
        private readonly IOptions<SAMLGeneralSettings> _samlGeneralSettings;

        public SignerController(ILogger logger, IOptions<ADGeneralSettings> adGeneralSettings, IOptions<GeneralSettings> generalSettings, IAD adHandler,
             IJWT jwt, IOptions<SAMLGeneralSettings> samlGeneralSettings,
             ILoginHandler loginHandler)
            {
            _logger = logger;
            _adGeneralSettings = adGeneralSettings.Value;
            
            _jwt = jwt;
            _adHandler = adHandler;
            _generalSettings = generalSettings.Value;
            _loginHandler = loginHandler;
            _samlGeneralSettings = samlGeneralSettings;
        }

        [SwaggerResponse((int)HttpStatusCode.Redirect)]
        public async Task<IActionResult> Signature(string id)
        {
            var routeUrl = _generalSettings.SignerFronendApplicationRoute;
            try
            {
                _logger.Debug("Signer Auth request {Id} - User {UserName}", id, HttpContext.User.Identity.Name);


                if (!Guid.TryParse(id, out var newGuid))
                {

                    _logger.Debug("Signer Auth request {Id} not an legal guid", id);
                    return Redirect(_generalSettings.SignerFronendApplicationRoute);
                }

                if(!string.IsNullOrWhiteSpace(HttpContext.User.Identity.Name))
                {
                    _adHandler.InItADHandlerForUser(HttpContext.User.Identity.Name);
                    string email = _adHandler.GetUserEmail();
                    if (string.IsNullOrWhiteSpace(email))
                    {
                        _logger.Debug("User {UserName} has not email ", HttpContext.User.Identity.Name);
                        return Redirect(_generalSettings.SignerFronendApplicationRoute);
                    }
                    if (_adHandler.IsUserInADGroup(_adGeneralSettings.ADSignerGroupName))
                    {
                        _logger.Debug("User {UserName} in group - Trying to login ", HttpContext.User.Identity.Name);
                        var signerLogin = new SignerLoginModel()
                        {
                            Email = email,
                            GuidAuthToken = newGuid,
                            PhoneNumbers = _adHandler.GetUserPhones(),
                            UserName = _adHandler.GetUserAdName()
                        };

                        routeUrl = await _loginHandler.DoAuthToSigner(signerLogin);
                    }
                    else
                    {
                        _logger.Debug("Signer {UserName} not in group  {AdSignerGroupName}", HttpContext.User.Identity.Name, _adGeneralSettings.ADSignerGroupName);
                    }

                }
                else
                {
                    _logger.Debug("Go to saml to auth signer");
                    return Redirect($"{_samlGeneralSettings.Value.InternalSAMLShortURL}?signerGuidCache={id}");
                }
                                
            }
            catch (Exception ex)
            {
                _logger.Error("Error while trying to Authenticate Signer with AD- redirecting signer to WSE signature error screen", ex);
            }

            return Redirect(routeUrl);
        }
    }
}
