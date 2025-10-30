
using System.Diagnostics;

using Serilog;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Options;
using WSE_ADAuth.Models;
using System;
using Common.Interfaces.DB;
using Common.Interfaces;
using Common.Models.Settings;
using WSE_ADAuth.AD;
using WSE_ADAuth.Handler;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using Common.Models;

namespace WSE_ADAuth.Controllers
{
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class UserAuthController : Controller
    {
        private readonly ILogger _logger;
        private readonly ADGeneralSettings _adGeneralSettings;
        private readonly IAD _adHandler;
        private readonly IUserConnector _userConnector;
        private readonly IOneTimeTokens _oneTimeTokens;
        private readonly GeneralSettings _generalSettings;
        private readonly ILoginHandler _loginHandler;

        public UserAuthController(ILogger logger, IOptions<ADGeneralSettings> adGeneralSettings, IOptions<GeneralSettings> generalSettings, IAD adHandler, IUserConnector userConnector,
            IOneTimeTokens oneTimeTokens, ILoginHandler loginHandler)
        {
            _logger = logger;
            _adGeneralSettings = adGeneralSettings.Value;
            _adHandler = adHandler;
            _userConnector = userConnector;
            _oneTimeTokens = oneTimeTokens;
            _generalSettings = generalSettings.Value;
            _loginHandler = loginHandler;
        }
        [SwaggerResponse((int)HttpStatusCode.Redirect)]
        public async Task<IActionResult> Index()
        {

           string redirectPath = _generalSettings.UserFronendApplicationRoute;
            try
            {
                // AD LOGIN
                // need to redirect to SAML if needed??
                _adHandler.InItADHandlerForUser(HttpContext.User.Identity.Name);
                string email = _adHandler.GetUserEmail();
                _logger.Debug("User Email: {Email}", email);

                if (!string.IsNullOrWhiteSpace(email))
                {
                                        
                    if (_adHandler.IsUserInADGroup(_adGeneralSettings.ADUserGroupName))
                    {
                        var loginModel = new LoginToClient()
                        {
                            LoginSource = LoginSource.AD,
                            UserAuthToken = "",
                            UserEmail = email,
                            UserName = _adHandler.GetUserAdName()??""
                        };
                        var token = await _loginHandler.DoLoginToClientFrontEnd(loginModel);
                         redirectPath = $@"{_generalSettings.UserFronendApplicationRoute}";
                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            redirectPath = $@"{_generalSettings.UserFronendApplicationRoute}/externallogin/{token}";
                        }
                      
                    }
                    else
                    {
                        _logger.Information("User {UserName} not in group: {AdUserGroup}", User.Identity.Name, _adGeneralSettings.ADUserGroupName);
                    }
                }
                else if(_adGeneralSettings.SupportSAML)
                {
                    return RedirectToAction("BuildSSORequestURL","SAML");
                }
                else
                {
                    _logger.Information("User {UserName} has not email ", User.Identity.Name);
                }
                                
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Error while trying to Login with AD- redirecting user to WSE login screen" );
            }
            
            return Redirect(redirectPath);
        }

       
    }
}
