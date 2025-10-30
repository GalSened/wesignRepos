// Ignore Spelling: WSE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Models.Settings;
using LazyCache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WSE_ADAuth.Handler;
using WSE_ADAuth.Models;
using WSE_ADAuth.SAML;
using Common.Consts;
using Microsoft.AspNetCore.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Net;
using WSE_ADAuth.Features;
using Microsoft.FeatureManagement.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Common.Models;

namespace WSE_ADAuth.Controllers
{
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class SAMLController : Controller
    {
        private readonly ISAMLRequest _samlRequest;
        private readonly SAMLGeneralSettings _samlGeneralSettings;        
        private readonly ISAMLResponse _samlResponse;
        private readonly GeneralSettings _generalSettings;        
        private readonly ILoginHandler _loginHandler;
        private readonly IAppCache _appCache;
        private readonly ILogger _logger;

        public SAMLController(ISAMLRequest samlRequest, IOptions<SAMLGeneralSettings> samlGeneralSettings, ILoginHandler loginHandler,
           ISAMLResponse samlResponse, IOptions<GeneralSettings> generalSettings, 
            IAppCache appCache, ILogger logger)
        {
            _samlRequest = samlRequest;
            _samlGeneralSettings = samlGeneralSettings.Value;            
            _samlResponse = samlResponse;
            _generalSettings = generalSettings.Value;
            
            _loginHandler = loginHandler;
            _appCache = appCache;
            _logger = logger;
        }
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet("SAML/HostedApp/Token")]
        [FeatureGate(FeatureFlags.EnableLoginHostedAppApi)]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(UserTokensResponseDTO))]
        public async Task<IActionResult> LoginHostedAppApi()
        {
            string token = await GetHostedAppLoginInformation();
            var loginToken = await _loginHandler.DoDirectExternalLogin(token);
            if (loginToken != null)
            {
                return Ok(new UserTokensResponseDTO()
                {
                    Token = loginToken.JwtToken,
                    RefreshToken = loginToken.RefreshToken,
                    AuthToken = loginToken.AuthToken
                });
            }

            return BadRequest();

        }

        [HttpGet("SAML/HostedApp/signature/{signerToken}")]
        [SwaggerResponse((int)HttpStatusCode.Redirect)]
        public async Task<IActionResult> SamlSignature(Guid signerToken)
        {

            // need to read  information from the header
            string userIdentificationKey = HttpContext.Request.Headers[_samlGeneralSettings.HostedAppHeaderKey];
            string assertionKey = HttpContext.Request.Headers[_samlGeneralSettings.AssertionHeaderKey];

            string signerFname = HttpContext.Request.Headers[_samlGeneralSettings.HostedAppHeaderFirstName];
            string signerLName = HttpContext.Request.Headers[_samlGeneralSettings.HostedAppHeaderLastName];
            _logger.Debug("In LoginHostedApp signature Keys {AssertionKey} UserID: {UserId}  , Name: {signerFname} {signerLName}", assertionKey, 
                userIdentificationKey, signerFname, signerLName);

            string UserAuthToken = $"{_samlGeneralSettings.CertIDPrimer}{userIdentificationKey}{Consts.SAML_SEPARATOR}{assertionKey}";
            string signerURL =await _loginHandler.DoDirectAuthSignerSamlHostedAppLogin(new SignerLoginModel
            {
                AuthToken = UserAuthToken,
                UserName = userIdentificationKey,
                GuidAuthToken = signerToken,
                AuthName = $"{signerFname} {signerLName}",
                AuthId = userIdentificationKey,

            });
            return Redirect(signerURL);
            
        }

        [HttpGet("SAML/HostedApp")]
        [SwaggerResponse((int)HttpStatusCode.Redirect)]
        public async Task<IActionResult> LoginHostedApp(string signerGuidCache)
        {
            string token = await GetHostedAppLoginInformation();
            string redirectPath = $@"{_generalSettings.UserFronendApplicationRoute}";
            if (!string.IsNullOrWhiteSpace(token))
            {
                redirectPath = $@"{_generalSettings.UserFronendApplicationRoute}/externallogin/{token}";
            }

            return Redirect(redirectPath);
        }

      

        [HttpGet("SAML/BuildSSORequestURL")]
        [SwaggerResponse((int)HttpStatusCode.Redirect)]
        public IActionResult BuildSSORequestURL(string signerGuidCache)
        {            
            var req = _samlRequest.GetRequest(AuthRequestFormat.Base64);
            if(!string.IsNullOrWhiteSpace( signerGuidCache))
            {
                _appCache.Add<string>(_samlRequest.GetRequestId(), signerGuidCache, new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions());

            }
            //https://login.microsoftonline.com/08632312-b779-4cac-8f06-4a914d7e5091/saml2 example
            return Redirect($@"{_samlGeneralSettings.SAML_IDPBaseURL}?SAMLRequest=" + HttpUtility.UrlEncode(req));
        }
        
        [HttpPost]
        [Route("SAML/Callback")]
        [SwaggerResponse((int)HttpStatusCode.Redirect)]
        public async Task<IActionResult> LoginCallback()
        {
            string redirectPath = _generalSettings.UserFronendApplicationRoute;
            try
            {
               
                //HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);
                // SIGNER??? how can i Know
                var data = HttpContext.Request.Form["SAMLResponse"];
                _samlResponse.LoadXmlFromBase64(data);

                if (_samlResponse.IsValid())
                {
                    string guidToken  = _appCache.Get<string>(_samlResponse.GetResponseId());
                    string email = _samlResponse.GetUserEmail();
                    if(string.IsNullOrWhiteSpace(email))
                    {
                        return Unauthorized("Email not found");
                    }
                    if (!string.IsNullOrWhiteSpace(guidToken))
                    {
                        // signer auth
                        var signerLogin = new SignerLoginModel()
                        {
                            Email = email,
                            GuidAuthToken = Guid.Parse(guidToken),
                            PhoneNumbers = _samlResponse.GetUserPhoneNumbers(),
                            UserName = ""
                        };

                        redirectPath = await _loginHandler.DoAuthToSigner(signerLogin);
                        _appCache.Remove(_samlResponse.GetResponseId());


                    }
                    else
                    {
                        // user login
                        email = _samlResponse.GetUserEmail();
                        string userAuthToken = _samlResponse.GetUserNameID();
                        userAuthToken += $"{Consts.SAML_SEPARATOR}{_samlResponse.GetSamlXml()}";

                        var loginModel = new LoginToClient()
                        {
                            LoginSource = LoginSource.SAML,
                            UserAuthToken =  _samlGeneralSettings.SaveAuthToken ?  userAuthToken : "",
                            UserEmail = email,
                            UserName = ""
                        };

                       
                        var token = await _loginHandler.DoLoginToClientFrontEnd(loginModel);
                        redirectPath = $@"{_generalSettings.UserFronendApplicationRoute}";
                        if (!string.IsNullOrWhiteSpace(token))
                        {
                            redirectPath = $@"{_generalSettings.UserFronendApplicationRoute}/externallogin/{token}";
                        }

                    }




                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex, "Failed to validate SAML");
                return Unauthorized();
            }
            return Redirect(redirectPath);
        }

        private async Task<string> GetHostedAppLoginInformation()
        {
            string userIdentificationKey = HttpContext.Request.Headers[_samlGeneralSettings.HostedAppHeaderKey];
            string assertionKey = HttpContext.Request.Headers[_samlGeneralSettings.AssertionHeaderKey];

            _logger.Debug("In LoginHostedApp Keys {AssertionKey} UserID: {UserId}", assertionKey, userIdentificationKey);
          
            string UserAuthToken = $"{_samlGeneralSettings.CertIDPrimer}{userIdentificationKey}{Consts.SAML_SEPARATOR}{assertionKey}";
            var loginModel = new LoginToClient()
            {
                LoginSource = LoginSource.SAML,
                UserAuthToken = UserAuthToken,
                UserEmail = userIdentificationKey,
                UserName = ""
            };
            _logger.Debug("In LoginHostedApp DoLoginToClientFrontEnd from external resource ");
            var token = await _loginHandler.DoLoginToClientFrontEnd(loginModel);
            return token;
        }
    }
}
