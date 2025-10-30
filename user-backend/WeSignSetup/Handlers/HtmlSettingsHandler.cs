using HtmlAgilityPack;
using System.Configuration;
using System.IO;
using System.Linq;
using WeSignSetup.Models;

namespace WeSignSetup.Handlers
{
    public class HtmlSettingsHandler
    {
        private LogHandler _logHandler;

        public HtmlSettingsHandler(LogHandler logHandler)
        {
            _logHandler = logHandler;
        }

        public void UpdateApiEndpoints(string domain, string appPort, string managementAppPort, bool shouldAllowRDPToManagement = false , bool isSecure = false)
        {
            string signerPath = Path.Combine(Folders.SignerFrontendPath, "index.html");
            string protocol = isSecure ? "https" : "http";
            if (isSecure)
            {
                appPort = ConfigurationManager.AppSettings["SecureAppPort"];
                managementAppPort = ConfigurationManager.AppSettings["SecureManagementAppPort"];
            }
            string signerapi = ConfigurationManager.AppSettings["SignerApiApplicationRoute"];
            string signer = ConfigurationManager.AppSettings["SignerApplicationRoute"];
            string signerApiEndpoint = $"{protocol}://{domain}:{appPort}/{signerapi}/v3";
            string signerEndpoint = $"{protocol}://{domain}:{appPort}/{signer}";
            string hubApiEndpoint = $"{protocol}://{domain}:{appPort}/{signerapi}/v3";
            string registerEndpoint = $"{protocol}://{domain}:{appPort}/login/register";
            Update(signerPath, "data-api-endpoint", signerApiEndpoint);
            Update(signerPath, "data-hub-endpoint", hubApiEndpoint);
            Update(signerPath, "register-endpoint", registerEndpoint);
            Update(signerPath, "signer-endpoint", signerEndpoint);

            string userPath = Path.Combine(Folders.UserFrontendPath, "index.html");
            string userapi = ConfigurationManager.AppSettings["UserApiApplicationRoute"];
            string userApiEndpoint = $"{protocol}://{domain}:{appPort}/{userapi }/v3";
            string userEndpoint = $"{protocol}://{domain}:{appPort}";
            string paymentApiEndpoint = $"{protocol}://{domain}:{appPort}/{userapi }/v3";
            string signerHubApiEndpoint = $"{protocol}://{domain}:{appPort}/{userapi}/v3";
            string ssoLoginEndpoint = $"{protocol}://{domain}:{appPort}/auth";
            Update(userPath, "data-api-endpoint", userApiEndpoint);
            Update(userPath, "data-signer-api-endpoint", signerApiEndpoint);
            Update(userPath, "data-hub-endpoint", signerHubApiEndpoint);
            Update(userPath, "data-payment-api-endpoint", paymentApiEndpoint);
            Update(userPath, "SSO-login-endpoint", ssoLoginEndpoint);
            Update(userPath, "user-endpoint", userEndpoint);

            string managementPath = Path.Combine(Folders.ManagementFrontendPath, "index.html");
            string defaultManagementAppDomain = shouldAllowRDPToManagement ? domain : ConfigurationManager.AppSettings["DefaultManagementAppDomain"];            
            string managementapi = ConfigurationManager.AppSettings["ManagementApiApplicationRoute"];
            string managementApiEndpoint = $"{protocol}://{defaultManagementAppDomain}:{managementAppPort}/{managementapi}/v3";            
            Update(managementPath, "data-api-endpoint" , managementApiEndpoint);

            _logHandler.Debug("Successfully update all HTML files with domain name of API endpoint.");
        }       

        /// <summary>
        /// Optionals keys: "data-api-endpoint", "data-hub-endpoint", "register-endpoint", "data-payment-api-endpoint"
        /// </summary>
        /// <param name="path"></param>
        /// <param name="metaKey"></param>
        /// <param name="metaValue"></param>
        private void Update(string path, string metaKey, string metaValue)
        {
            if (File.Exists(path))
            {
                var doc = new HtmlDocument();
                doc.Load(path);
                var metaElements = doc.DocumentNode.SelectNodes("//meta");
                var metaNode = metaElements.FirstOrDefault(x => x.Attributes.FirstOrDefault(y => y.Name.ToLower() == metaKey.ToLower()) != null);
                if (metaNode != null)
                {
                    metaNode.SetAttributeValue(metaKey, metaValue);
                    doc.Save(path);
                }
                else
                {
                    _logHandler.Debug($"Meta key [{metaKey}] not found");
                }
            }
            else
            {
                _logHandler.Debug($"File [{path}] not exist");
            }

        }
    }
}
