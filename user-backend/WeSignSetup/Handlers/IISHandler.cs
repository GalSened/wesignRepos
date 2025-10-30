using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Web.Administration;
using WeSignSetup.Models;

namespace WeSignSetup.Handlers
{
    public class IISHandler
    {
        private const string SIGNER_API = "SignerApi";
        private const string USER_API = "UserApi";
        private const string USER_SOAP = "UserSoap";
        private const string HISTORY_SERVICE_API = "HistoryServiceApi";
        private const string MANAGEMNT_API = "ManagementApi";
        private const string DEFAULT_WEB_SITE = "Default Web Site";
        private const string MANAGMENT = "Management";
        private const string SIGNER = "Signer";
        private const string WSE_AUTH = "WseAuth";
        private const string PDF_EXTERNAL_CONVERTER = "PdfExternalConvertor";
        //public const string NO_MANAGE_CODE_VERSION = "No Managed Code";
        public const string NO_MANAGE_CODE_VERSION = "";
        private readonly LogHandler _logHandler ;
        private readonly SitesDetails _sitesDetails;

        public IISHandler(LogHandler logHandler, SitesDetails sitesDetails)
        {
            _logHandler  = logHandler;
            _sitesDetails = sitesDetails;
        }

        public void InstallSitesAndApplications(string sslThumbprint)
        {
            var serverManager = new ServerManager();
            CreateAllAppPools(serverManager);
            CreateManagementSite(serverManager, sslThumbprint);
            CreateDefaultWebSiteAndApplications(serverManager, sslThumbprint);
            serverManager.CommitChanges();
            serverManager.Dispose();
            _logHandler.Debug("IISHandler - Successfully install all sites and applications in IIS.");
        }

        private void CreateAllAppPools(ServerManager serverManager)
        {
            _logHandler.Debug("CreateAllAppPools - start creating applications pools ...");
            CreateAppPool(USER_SOAP, ManagedPipelineMode.Integrated, serverManager);
            CreateAppPool(USER_API, ManagedPipelineMode.Integrated, serverManager, NO_MANAGE_CODE_VERSION);
            CreateAppPool(HISTORY_SERVICE_API, ManagedPipelineMode.Integrated, serverManager, NO_MANAGE_CODE_VERSION);            
            CreateAppPool(PDF_EXTERNAL_CONVERTER, ManagedPipelineMode.Integrated, serverManager, NO_MANAGE_CODE_VERSION);            
            CreateAppPool(SIGNER, ManagedPipelineMode.Integrated, serverManager, NO_MANAGE_CODE_VERSION);
            CreateAppPool(SIGNER_API, ManagedPipelineMode.Integrated, serverManager, NO_MANAGE_CODE_VERSION);
            CreateAppPool(MANAGEMNT_API, ManagedPipelineMode.Integrated, serverManager, NO_MANAGE_CODE_VERSION);
            CreateAppPool(MANAGMENT, ManagedPipelineMode.Integrated, serverManager, NO_MANAGE_CODE_VERSION);
            CreateAppPool(WSE_AUTH, ManagedPipelineMode.Integrated, serverManager, NO_MANAGE_CODE_VERSION);
            if (!_sitesDetails.ShouldUseDefaultWebSite && !string.IsNullOrWhiteSpace(_sitesDetails.MainSiteName))
            {
                CreateAppPool(_sitesDetails.MainSiteName, ManagedPipelineMode.Integrated, serverManager, NO_MANAGE_CODE_VERSION);
            }
        }

        #region Private Functions

        private void CreateManagementSite(ServerManager serverManager, string sslThumbprint)
        {
            _logHandler.Debug("CreateManagementSite - start creating management site...");

            string managementSite = !string.IsNullOrWhiteSpace(_sitesDetails.ManagementSiteName) ? _sitesDetails.ManagementSiteName : MANAGMENT;             
            CreateIISWebsite(managementSite, $"*:{_sitesDetails.ManagementSitePort}:", Folders.ManagementFrontendPath, MANAGMENT, serverManager, sslThumbprint);
            string applicationName = "/managementApi";
            CreateIISApplication(managementSite, applicationName, Folders.ManagementBackendPath, MANAGEMNT_API, serverManager);
        }

        private void CreateDefaultWebSiteAndApplications(ServerManager serverManager, string sslThumbprint)
        {            
            if (_sitesDetails.ShouldUseDefaultWebSite)
            {
                _logHandler.Debug($"IISHandler - Create Default WebSite And Applications under {DEFAULT_WEB_SITE}");

                UpdateDefaultWebSite(serverManager, sslThumbprint);
            }
            else
            {
                _logHandler.Debug($"IISHandler - Create WebSite And Applications under [{_sitesDetails.MainSiteName}]");

                CreateAppPool(_sitesDetails.MainSiteName, ManagedPipelineMode.Integrated, serverManager, NO_MANAGE_CODE_VERSION);
                CreateIISWebsite(_sitesDetails.MainSiteName, $"*:{_sitesDetails.MainSitePort}:", Folders.UserFrontendPath, _sitesDetails.MainSiteName, serverManager, sslThumbprint);
            }

            _sitesDetails.MainSiteName = _sitesDetails.ShouldUseDefaultWebSite ? DEFAULT_WEB_SITE : _sitesDetails.MainSiteName;
            CreateIISApplication(_sitesDetails.MainSiteName, "/userSoap", Folders.UserSoapServicePath, USER_SOAP, serverManager);
            CreateIISApplication(_sitesDetails.MainSiteName, "/userApi", Folders.UserBackendPath, USER_API, serverManager);
            CreateIISApplication(_sitesDetails.MainSiteName, "/signerApi", Folders.SignerBackendPath, SIGNER_API, serverManager);
            CreateIISApplication(_sitesDetails.MainSiteName, "/signer", Folders.SignerFrontendPath, SIGNER, serverManager);
            CreateIISApplication(_sitesDetails.MainSiteName, "/auth", Folders.WseAuthFolder, WSE_AUTH, serverManager);
            CreateIISApplication(_sitesDetails.MainSiteName, "/pdfExternal", Folders.PdfExternalService, PDF_EXTERNAL_CONVERTER, serverManager);
            CreateIISApplication(_sitesDetails.MainSiteName, "/HistoryServiceApi", Folders.HistoryServiceApi, HISTORY_SERVICE_API, serverManager);
            EnabledWindowsAuthentication(_sitesDetails.MainSiteName, "auth", serverManager);
        }

        private void UpdateDefaultWebSite(ServerManager serverManager, string sslThumbprint)
        {
            if (serverManager.Sites[DEFAULT_WEB_SITE] != null)
            {
                serverManager.Sites[DEFAULT_WEB_SITE].Applications["/"].VirtualDirectories["/"].PhysicalPath = Folders.UserFrontendPath;
            }
            else
            {
                CreateAppPool(DEFAULT_WEB_SITE, ManagedPipelineMode.Integrated, serverManager, NO_MANAGE_CODE_VERSION); 
                //CreateIISWebsite(DEFAULT_WEB_SITE, "*:443:", Folders.UserFrontendPath, DEFAULT_WEB_SITE, serverManager, sslThumbprint);
            }

            var httpBinding = serverManager.Sites[DEFAULT_WEB_SITE].Bindings.FirstOrDefault(x => x.Protocol.Contains("http"));
            if (httpBinding == null)
            {
                serverManager.Sites[DEFAULT_WEB_SITE].Bindings.Add("*:80:", "http");
            }
        }

        private void CreateAppPool(string poolname, ManagedPipelineMode mode, ServerManager serverManager, string runtimeVersion = "v4.0")
        {
            _logHandler.Debug($"CreateAppPool- start creating application pool named [{poolname}]");
            var appPool = serverManager.ApplicationPools[poolname];
            if (appPool == null)
            {
                ApplicationPool newPool = serverManager.ApplicationPools.Add(poolname);
                newPool.ManagedRuntimeVersion = runtimeVersion;
                newPool.ManagedPipelineMode = mode;
                newPool.Enable32BitAppOnWin64 = true;
                _logHandler.Debug($"IISHandler - Successfully create application pool named '{poolname}'");
            }
            else
            {
                _logHandler.Debug($"IISHandler - Application pool '{poolname}' already exist");
            }
        }

        private void CreateIISWebsite(string websiteName, string bidingInfo, string phyPath, string appPool, ServerManager serverManager, string sslThumbprint, bool isSecure = false)
        {
            _logHandler.Debug($"CreateIISWebsite - start creating site named [{websiteName}]...");

            var site = serverManager.Sites[websiteName];
            if (site == null)
            {
                _logHandler.Debug($"IISHandler - Website '{websiteName}' not exist");
                if (isSecure)
                {
                    serverManager.Sites.Add(websiteName, "https", bidingInfo, phyPath);
                    _logHandler.Debug($"IISHandler - Add website '{websiteName}' with 'https' binding");
                }
                else
                {
                    serverManager.Sites.Add(websiteName, "http", bidingInfo, phyPath);
                    _logHandler.Debug($"IISHandler - Add website '{websiteName}' with 'http' binding");
                }

                if (!string.IsNullOrWhiteSpace(appPool))
                {
                    serverManager.Sites[websiteName].ApplicationDefaults.ApplicationPoolName = appPool;
                    _logHandler.Debug($"IISHandler - Add application pool '{appPool}' to website '{websiteName}'");
                    BindSslToSite(websiteName, serverManager, sslThumbprint);
                }
            }
            else
            {
                _logHandler.Debug($"IISHandler - Website '{websiteName}' already exist");
            }
        }

        private void BindSslToSite(string websiteName, ServerManager serverManager, string sslThumbprint)
        {
            if (!string.IsNullOrWhiteSpace(sslThumbprint))
            {
                var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);
                var certificate = store.Certificates.Find(X509FindType.FindByThumbprint, sslThumbprint, true);
                if (certificate.Count > 0)
                {
                    var site1 = serverManager.Sites[websiteName];
                    var bind = site1.Bindings.FirstOrDefault(x => x.Protocol.Contains("https"));
                    if (bind != null)
                    {
                        bind.CertificateHash = certificate[0].GetCertHash();
                        bind.CertificateStoreName = "MY";
                        bind.SslFlags = SslFlags.Sni;
                    }
                    else
                    {
                        _logHandler.Debug($"IISHandler - There is no 'https' binding to site '{websiteName}'");
                    }
                }
                else
                {
                    _logHandler.Debug($"IISHandler - There is no certificate with Thumbprint '{sslThumbprint}'");
                }
                store.Close();
                _logHandler.Debug($"IISHandler - Add SSL binding to website '{websiteName}'");
            }
        }

        private void CreateIISApplication(string websiteName, string applicationName, string physicalPath, string appPoolName, ServerManager serverManager)
        {
            if (serverManager.Sites[websiteName] != null && serverManager.Sites[websiteName].Applications[applicationName] == null)
            {
                var application = serverManager.Sites[websiteName].Applications.Add(applicationName, physicalPath);
                application.ApplicationPoolName = appPoolName;
                _logHandler.Debug($"IISHandler - Add website '{websiteName}' with application '{applicationName}' ");
            }
            else
            {
                _logHandler.Debug($"IISHandler - Website '{websiteName}' with application '{applicationName}' already exist");
            }
        }

        private void EnabledWindowsAuthentication(string websiteName, string applicationName, ServerManager serverManager)
        {
            try
            {
                _logHandler.Debug($"IISHandler - Enabled windows authentication for WebSite [{websiteName}] And Applications [{applicationName}]");
                Configuration config = serverManager.GetApplicationHostConfiguration();
                ConfigurationSection windowsAuthenticationSection = config.GetSection("system.webServer/security/authentication/windowsAuthentication", $"{websiteName}/{applicationName}");
                windowsAuthenticationSection["enabled"] = true;
                _logHandler.Debug($"IISHandler - Successfully Enabled windows authentication");

            }
            catch (Exception ex)
            {
                _logHandler.Error($"Error while EnabledWindowsAuthentication for [{websiteName}/{applicationName}]. ", ex);               
            }
        }

        #endregion
    }
}
