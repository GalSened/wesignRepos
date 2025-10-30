using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using WeSignSetup.Models;

namespace WeSignSetup.Handlers
{
    public class AppsettingsHandler
    {
        private LogHandler _logHandler;
        private string _dataBaseFolder;

        public AppsettingsHandler(LogHandler logHandler)
        {
            _logHandler = logHandler;
        }

        public void UpdateAppSettingsFiles(string connectionString, string domainName, string ca, string appPort, string dataBaseFolder, string managementAppPort, ADDetails adDetails, bool isSecure = false)
        {
            _dataBaseFolder = dataBaseFolder;
            CreateDataFolders();
            
            GetJwtSettings(out string jwtBearerSignatureKey, out string jwtSignerSignatureKey);

            string protocol = isSecure ? "https" : "http";
            if (isSecure)
            {
                appPort = ConfigurationManager.AppSettings["SecureAppPort"];
                managementAppPort = ConfigurationManager.AppSettings["SecureManagementAppPort"];
            }
            string signerFronendApplicationRoute = $"{protocol}://{domainName}:{appPort}/signer";
            string userFronendApplicationRoute = $"{protocol}://{domainName}:{appPort}";
            string signerapi = ConfigurationManager.AppSettings["SignerApiApplicationRoute"];
            string agentHubEndpoint = $"{protocol}://{domainName}:{appPort}/{signerapi}/v3/agentsocket";
            string smartCardDesktopClientInstallerPath = Path.Combine(Folders.BaseFolder, "SmartCardDesktopClient", "SmartCardDesktopClientSetup.exe");
            string productKey = ConfigurationManager.AppSettings["WeSignProductKey"];

            string authSignerFronendApplicationRoute = $"{protocol}://{domainName}:{appPort}/auth/signer";
            UpdateSignerAppSettings(connectionString, jwtBearerSignatureKey, jwtSignerSignatureKey, signerFronendApplicationRoute,
                                    agentHubEndpoint, smartCardDesktopClientInstallerPath, productKey, userFronendApplicationRoute,
                                    authSignerFronendApplicationRoute);
            string managementapi = ConfigurationManager.AppSettings["ManagementApiApplicationRoute"];
            string managementAPIUrl = $"{protocol}://{domainName}:{managementAppPort}/{managementapi}";
            UpdateUserAppSettings(connectionString, userFronendApplicationRoute, jwtBearerSignatureKey, jwtSignerSignatureKey,
                                signerFronendApplicationRoute, ca, agentHubEndpoint, smartCardDesktopClientInstallerPath, productKey,
                                authSignerFronendApplicationRoute, managementAPIUrl);
            UpdateManagementAppSettings(connectionString, userFronendApplicationRoute, signerFronendApplicationRoute, authSignerFronendApplicationRoute, jwtBearerSignatureKey, jwtSignerSignatureKey, ca);
            UpdateWSEAuthAppSettings(connectionString, userFronendApplicationRoute, signerFronendApplicationRoute, jwtBearerSignatureKey,
                                    jwtSignerSignatureKey, adDetails);
            _logHandler.Debug("Successfully update all appSettings.json files for backend apps.");
        }

        #region Private Functions

        private void GetJwtSettings(out string jwtBearerSignatureKey, out string jwtSignerSignatureKey)
        {
            //Case of clean installation
            jwtBearerSignatureKey = Guid.NewGuid().ToString();
            jwtSignerSignatureKey = Guid.NewGuid().ToString();
            string jsonPath = string.Empty;

            //Case of update mode while there is backup folder            
            string backupFolder = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Folders.UserBackendPath)), "Backup");
            if (Directory.Exists(backupFolder))
            {
                var lastUpdateBackupSitesFolder = new DirectoryInfo(backupFolder).GetDirectories().OrderByDescending(d => d.LastWriteTimeUtc).First();
                jsonPath = Path.Combine(lastUpdateBackupSitesFolder.FullName, "UserBackend", "appsettings.json");
            }
            if (File.Exists(jsonPath))
            {
                _logHandler.Debug("Backup folder found, get JWT settings from json file from backup folder");
                GetJwtValuesFromFile(out jwtBearerSignatureKey, out jwtSignerSignatureKey, jsonPath);
            }
            else
            {
                //Case of update mode without backup folder
                jsonPath = Path.Combine(Folders.UserBackendPath, "appsettings.json");
                if (File.Exists(jsonPath))
                {
                    _logHandler.Debug("Get JWT settings from json file from sites folder ");
                    GetJwtValuesFromFile(out jwtBearerSignatureKey, out jwtSignerSignatureKey, jsonPath);
                }
                else
                {
                    _logHandler.Debug($"UpdateAppSettingsFiles - [{jsonPath}] not found, probably it is clean installation, using new Guid for JWT ");
                }
            }
        }

        private void GetJwtValuesFromFile(out string jwtBearerSignatureKey, out string jwtSignerSignatureKey, string jsonPath)
        {
            _logHandler.Debug($"UpdateAppSettingsFiles - [{jsonPath}] found , Get jwtSignerSignatureKey and jwtBearerSignatureKey from userBackend appsettings file");
            string json = File.ReadAllText(jsonPath);
            dynamic jsonObj = JsonConvert.DeserializeObject(json);
            var dynamicJwtBearerSignatureKey = jsonObj["JwtSettings"]["JwtBearerSignatureKey"];
            jwtBearerSignatureKey = Convert.ToString(dynamicJwtBearerSignatureKey);
            var dynamicJwtSignerSignatureKey = jsonObj["JwtSettings"]["JwtSignerSignatureKey"];
            jwtSignerSignatureKey = Convert.ToString(dynamicJwtSignerSignatureKey);
        }

        private void CreateDataFolders()
        {
            Directory.CreateDirectory(Path.Combine(_dataBaseFolder, "Appendices"));
            Directory.CreateDirectory(Path.Combine(_dataBaseFolder, "Attachments"));
            Directory.CreateDirectory(Path.Combine(_dataBaseFolder, "Certificates"));
            Directory.CreateDirectory(Path.Combine(_dataBaseFolder, "CompaniesLogo"));
            Directory.CreateDirectory(Path.Combine(_dataBaseFolder, "ContactSeals"));
            Directory.CreateDirectory(Path.Combine(_dataBaseFolder, "Documents"));
            Directory.CreateDirectory(Path.Combine(_dataBaseFolder, "EmailTemplates"));
            Directory.CreateDirectory(Path.Combine(_dataBaseFolder, "Logs"));
            Directory.CreateDirectory(Path.Combine(_dataBaseFolder, "Templates"));
            Directory.CreateDirectory(Path.Combine(_dataBaseFolder, "Certificates", "Contacts"));
            Directory.CreateDirectory(Path.Combine(_dataBaseFolder, "Certificates", "Users"));
            Directory.CreateDirectory(Path.Combine(_dataBaseFolder, "License"));
            Directory.CreateDirectory(Path.Combine(_dataBaseFolder, "Temp"));
            Directory.CreateDirectory(Path.Combine(_dataBaseFolder, "ContactSignatureImages"));
        }

        private void UpdateSignerAppSettings(string connectionString, string jwtBearerSignatureKey, string jwtSignerSignatureKey, string signerFronendApplicationRoute, string agentHubEndpoint, string smartCardDesktopClientInstallerPath, string productKey, string userFronendApplicationRoute, string authSignerFronendApplicationRoute)
        {
            var signerAppParameters = new List<(string[] key, string value)>()
            {
                (new string[] { "GeneralSettings", "ConnectionString"}, connectionString),
                (new string[] { "GeneralSettings", "SignerFronendApplicationRoute"}, signerFronendApplicationRoute),
                (new string[] { "GeneralSettings", "UserFronendApplicationRoute"}, userFronendApplicationRoute),
                (new string[] { "GeneralSettings", "AgentHubEndpoint"}, agentHubEndpoint),
                (new string[] { "GeneralSettings", "SmartCardDesktopClientInstallerPath"}, smartCardDesktopClientInstallerPath),
                (new string[] { "GeneralSettings", "ProductId"}, productKey),
                (new string[] { "GeneralSettings", "AuthSignerFronendApplicationRoute"}, authSignerFronendApplicationRoute),

                (new string[] { "JwtSettings", "JwtBearerSignatureKey"}, jwtBearerSignatureKey),
                (new string[] { "JwtSettings", "JwtSignerSignatureKey"}, jwtSignerSignatureKey),

                (new string[] { "FolderSettings", "Documents"}, Path.Combine(_dataBaseFolder,"Documents")),
                (new string[] { "FolderSettings", "Templates"}, Path.Combine(_dataBaseFolder,"Templates")),
                (new string[] { "FolderSettings", "EmailTemplates"}, Path.Combine(_dataBaseFolder,"EmailTemplates")),
                (new string[] { "FolderSettings", "CompaniesLogo"}, Path.Combine(_dataBaseFolder,"CompaniesLogo")),
                (new string[] { "FolderSettings", "ContactSeals"}, Path.Combine(_dataBaseFolder,"ContactSeals")),
                (new string[] { "FolderSettings", "Certificates"}, Path.Combine(_dataBaseFolder,"Certificates")),
                (new string[] { "FolderSettings", "ContactCertificates"}, Path.Combine(_dataBaseFolder,"Certificates", "Contacts")),
                (new string[] { "FolderSettings", "UserCertificates"}, Path.Combine(_dataBaseFolder,"Certificates", "Users")),
                (new string[] { "FolderSettings", "Appendices"}, Path.Combine(_dataBaseFolder,"Appendices")),
                (new string[] { "FolderSettings", "Attachments"}, Path.Combine(_dataBaseFolder,"Attachments")),
                (new string[] { "FolderSettings", "ContactSignatureImages"}, Path.Combine(_dataBaseFolder,"ContactSignatureImages")),

                (new string[] { "Serilog", "WriteTo", "0", "Args", "path", }, Path.Combine(_dataBaseFolder,"Logs","Signer","WeSignSigner-.log")),
                (new string[] { "Serilog", "WriteTo", "1", "Args", "ConnectionString", }, connectionString),
            };
            Update(Path.Combine(Folders.SignerBackendPath, "appsettings.json"), signerAppParameters);
        }

        private void UpdateUserAppSettings(string connectionString, string userFronendApplicationRoute, string jwtBearerSignatureKey, string jwtSignerSignatureKey, string signerFronendApplicationRoute, string ca, string agentHubEndpoint, string smartCardDesktopClientInstallerPath, string productKey, string authSignerFronendApplicationRoute, string managementAPIUrl)
        {
            var userAppParameters = new List<(string[] key, string value)>()
            {
                (new string[] { "GeneralSettings", "CA"}, ca),
                (new string[] { "GeneralSettings", "ConnectionString"}, connectionString),
                (new string[] { "GeneralSettings", "SignerFronendApplicationRoute"}, signerFronendApplicationRoute),
                (new string[] { "GeneralSettings", "UserFronendApplicationRoute"}, userFronendApplicationRoute),
                (new string[] { "GeneralSettings", "AgentHubEndpoint"}, agentHubEndpoint),
                (new string[] { "GeneralSettings", "SmartCardDesktopClientInstallerPath"}, smartCardDesktopClientInstallerPath),
                (new string[] { "GeneralSettings", "ProductId"}, productKey),
                (new string[] { "GeneralSettings", "LicenseDMZEndpoint"}, ConfigurationManager.AppSettings["LicenseDMZEndpoint"]),
                (new string[] { "GeneralSettings", "AuthSignerFronendApplicationRoute"}, authSignerFronendApplicationRoute),
                (new string[] { "GeneralSettings", "ManagementAPIUrl"}, managementAPIUrl),

                (new string[] { "JwtSettings", "JwtBearerSignatureKey"}, jwtBearerSignatureKey),
                (new string[] { "JwtSettings", "JwtSignerSignatureKey"}, jwtSignerSignatureKey),

                (new string[] { "FolderSettings", "Documents"}, Path.Combine(_dataBaseFolder,"Documents")),
                (new string[] { "FolderSettings", "Templates"}, Path.Combine(_dataBaseFolder,"Templates")),
                (new string[] { "FolderSettings", "EmailTemplates"}, Path.Combine(_dataBaseFolder,"EmailTemplates")),
                (new string[] { "FolderSettings", "CompaniesLogo"}, Path.Combine(_dataBaseFolder,"CompaniesLogo")),
                (new string[] { "FolderSettings", "ContactSeals"}, Path.Combine(_dataBaseFolder,"ContactSeals")),
                (new string[] { "FolderSettings", "Certificates"}, Path.Combine(_dataBaseFolder,"Certificates")),
                (new string[] { "FolderSettings", "ContactCertificates"}, Path.Combine(_dataBaseFolder,"Certificates", "Contacts")),
                (new string[] { "FolderSettings", "UserCertificates"}, Path.Combine(_dataBaseFolder,"Certificates", "Users")),
                (new string[] { "FolderSettings", "Appendices"}, Path.Combine(_dataBaseFolder,"Appendices")),
                (new string[] { "FolderSettings", "Attachments"}, Path.Combine(_dataBaseFolder,"Attachments")),
                (new string[] { "FolderSettings", "License"}, Path.Combine(_dataBaseFolder,"License")),
                (new string[] { "FolderSettings", "TempFolder"}, Path.Combine(_dataBaseFolder,"Temp")),
                (new string[] { "FolderSettings", "Temp"}, Path.Combine(_dataBaseFolder,"Temp")),

                (new string[] { "Serilog", "WriteTo", "0", "Args", "path", }, Path.Combine(_dataBaseFolder,"Logs","User","WeSignUser-.log")),
                (new string[] { "Serilog", "WriteTo", "1", "Args", "ConnectionString", }, connectionString),
            };
            Update(Path.Combine(Folders.UserBackendPath, "appsettings.json"), userAppParameters);
        }

        private void UpdateManagementAppSettings(string connectionString, string userFronendApplicationRoute, string signerFronendApplicationRoute, string authSignerFronendApplicationRoute,string  jwtBearerSignatureKey, string jwtSignerSignatureKey, string ca)
        {
            var managementAppParameters = new List<(string[] key, string value)>()
            {
                (new string[] { "GeneralSettings", "CA"}, ca),
                (new string[] { "GeneralSettings", "UserFronendApplicationRoute"}, userFronendApplicationRoute),
                (new string[] { "GeneralSettings", "ConnectionString"}, connectionString),
                (new string[] { "GeneralSettings", "ProductId"}, ConfigurationManager.AppSettings["WeSignProductKey"]),
                (new string[] { "GeneralSettings", "LicenseDMZEndpoint"}, ConfigurationManager.AppSettings["LicenseDMZEndpoint"]),
                (new string[] { "GeneralSettings", "SignerFronendApplicationRoute"}, signerFronendApplicationRoute),
                (new string[] { "GeneralSettings", "AuthSignerFronendApplicationRoute"}, authSignerFronendApplicationRoute),

                (new string[] { "JwtSettings", "JwtBearerSignatureKey"}, jwtBearerSignatureKey),
                (new string[] { "JwtSettings", "JwtSignerSignatureKey"}, jwtSignerSignatureKey),

                (new string[] { "FolderSettings", "Documents"}, Path.Combine(_dataBaseFolder,"Documents")),
                (new string[] { "FolderSettings", "Templates"}, Path.Combine(_dataBaseFolder,"Templates")),
                (new string[] { "FolderSettings", "EmailTemplates"}, Path.Combine(_dataBaseFolder,"EmailTemplates")),
                (new string[] { "FolderSettings", "CompaniesLogo"}, Path.Combine(_dataBaseFolder,"CompaniesLogo")),
                (new string[] { "FolderSettings", "ContactSeals"}, Path.Combine(_dataBaseFolder,"ContactSeals")),
                (new string[] { "FolderSettings", "Certificates"}, Path.Combine(_dataBaseFolder,"Certificates")),
                (new string[] { "FolderSettings", "ContactCertificates"}, Path.Combine(_dataBaseFolder,"Certificates", "Contacts")),
                (new string[] { "FolderSettings", "UserCertificates"}, Path.Combine(_dataBaseFolder,"Certificates", "Users")),
                (new string[] { "FolderSettings", "Appendices"}, Path.Combine(_dataBaseFolder,"Appendices")),
                (new string[] { "FolderSettings", "Attachments"}, Path.Combine(_dataBaseFolder,"Attachments")),
                (new string[] { "FolderSettings", "License"}, Path.Combine(_dataBaseFolder,"License")),
                (new string[] { "FolderSettings", "Temp"}, Path.Combine(_dataBaseFolder,"Temp")),


                (new string[] { "Serilog", "WriteTo", "0", "Args", "path", }, Path.Combine(_dataBaseFolder,"Logs","Management","WeSignManagement-.log")),
                (new string[] { "Serilog", "WriteTo", "1", "Args", "ConnectionString", }, connectionString),
            };
            Update(Path.Combine(Folders.ManagementBackendPath, "appsettings.json"), managementAppParameters);
        }

        private void UpdateWSEAuthAppSettings(string connectionString, string userFronendApplicationRoute, string signerFronendApplicationRoute, string jwtBearerSignatureKey, string jwtSignerSignatureKey, ADDetails adDetails)
        {
            var authAppParameters = new List<(string[] key, string value)>()
            {
                (new string[] { "GeneralSettings", "UserFronendApplicationRoute"}, userFronendApplicationRoute),
                (new string[] { "GeneralSettings", "ConnectionString"}, connectionString),
                (new string[] { "GeneralSettings", "SignerFronendApplicationRoute"}, signerFronendApplicationRoute),

                (new string[] { "JwtSettings", "JwtBearerSignatureKey"}, jwtBearerSignatureKey),
                (new string[] { "JwtSettings", "JwtSignerSignatureKey"}, jwtSignerSignatureKey),

                (new string[] { "ADGeneralSettings", "ADUserGroupName"}, adDetails.ADUserGroupName),
                (new string[] { "ADGeneralSettings", "ADSignerGroupName"}, adDetails.ADSignerGroupName),

                (new string[] { "Serilog", "WriteTo", "0", "Args", "path", }, Path.Combine(_dataBaseFolder,"Logs","WSEAuth","WSEAuth-.log"))
            };
            Update(Path.Combine(Folders.WseAuthFolder, "appsettings.json"), authAppParameters);
        }

        private void Update(string path, List<(string[] key, string value)> parameters)
        {
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                dynamic jsonObj = JsonConvert.DeserializeObject(json);
                foreach (var (key, value) in parameters)
                {
                    if (key.Length == 2)
                    {
                        jsonObj[key[0]][key[1]] = value;
                    }
                    if (key.Length == 5)
                    {
                        jsonObj[key[0]][key[1]][int.Parse(key[2])][key[3]][key[4]] = value;
                    }
                }
                string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
                File.WriteAllText(path, output);
            }
        }

        #endregion
    }
}
