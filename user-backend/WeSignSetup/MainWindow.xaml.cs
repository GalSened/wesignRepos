using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using WeSignSetup.Handlers;
using WeSignSetup.Models;
using HtmlAgilityPack;

namespace WeSignSetup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string _chosenSslThumbprint = "";
        private Dictionary<string, string> _sslNameToThumbprint;
        //private string _connectionStringWithDbUser;        
        private string _connectionString;
        private readonly LogHandler _logHandler;
        private readonly ADDetails _adDetails;
        private readonly log4net.ILog _log;
        private EncryptorHandler _encryptor;

        public MainWindow(bool isUpdateMode, bool isUninstallMode, bool isCleanInstalltion)
        {
            try
            {
                if (!isCleanInstalltion && !isUninstallMode && !isUpdateMode)
                {
                    MessageBox.Show("Please provide correct args to program. supported args: --uninstall, --options or empty args");
                    Close();
                }
                InitializeComponent();
                _log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
                _encryptor = new EncryptorHandler();
                _logHandler = new LogHandler(uiLogs);
                _adDetails = new ADDetails();
                string currAppFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string baseWeSignSitesFolder = Directory.GetParent(currAppFolder).FullName;
                Folders.Init(baseWeSignSitesFolder);
                if (isUninstallMode)
                {
                    _logHandler?.Debug("Uninstall mode ------");
                    CreateCsvDataFile();
                    configurationSection.IsEnabled = false;
                    backupButton.Visibility = Visibility.Visible;
                    deatilsLabel.Visibility = Visibility.Hidden;
                }
                else if (isUpdateMode)
                {
                    _logHandler?.Debug("Update mode ------");
                    string path = Path.Combine(Path.GetTempPath(), "data.csv");
                    if (!File.Exists(path))
                    {
                        CreateCsvDataFile();
                    }
                    deatilsLabel.Visibility = Visibility.Visible;
                    configurationSection.IsEnabled = true;
                    updateConfigFilesButton.Visibility = Visibility.Visible;
                    updateDbButton.Visibility = Visibility.Visible;
                    updateDbButton.IsEnabled = false;
                    createSitesButton.Visibility = Visibility.Visible;
                    createSitesButton.IsEnabled = false;
                    backupButton.Visibility = Visibility.Collapsed;
                    submitButton.Visibility = Visibility.Collapsed;

                    InitDbButtons();
                    LoadDataFromCsvToUI();

                }
                //Clean installation
                else if (isCleanInstalltion)
                {
                    deatilsLabel.Visibility = Visibility.Visible; 
                    configurationSection.IsEnabled = true;
                    updateConfigFilesButton.Visibility = Visibility.Hidden;
                    updateDbButton.Visibility = Visibility.Hidden;
                    createSitesButton.Visibility = Visibility.Hidden;
                    backupButton.Visibility = Visibility.Collapsed;
                    submitButton.Visibility = Visibility.Visible;
                }
                
                CreateCsvDataFile();
                //InitSslCerts();
                _logHandler.Debug($"\nUserBackend Folder Path = {Folders.UserBackendPath}\nUserFrontend Folder Path = {Folders.UserFrontendPath}\nSignerBackend Folder Path = {Folders.SignerBackendPath}\nSignerFrontend Folder Path = {Folders.SignerFrontendPath}\nManagementBackend Folder Path = {Folders.ManagementBackendPath}\nManagementFrontend Folder Path = {Folders.ManagementFrontendPath}");
            }
            catch (Exception ex)
            {
                _logHandler.Error("Error on ctor. ", ex);
            }
        }

        private void CreateCsvDataFile(bool isUpdateMode = false)
        {
            try
            {
                string[] contents = new string[10];
                string path = Path.Combine(Path.GetTempPath(), "data.csv");
                _logHandler?.Debug($"CreateCsvDataFile in path [{path}]");
                string jsonPath = Path.Combine(Folders.UserBackendPath, "appsettings.json");
                if (File.Exists(jsonPath))
                {
                    string json = File.ReadAllText(jsonPath);
                    dynamic jsonObj = JsonConvert.DeserializeObject(json);
                    var dynamicConnectionString = jsonObj["GeneralSettings"]["ConnectionString"] ;
                    _connectionString = Convert.ToString(dynamicConnectionString);
                    _logHandler?.Debug($"CreateCsvDataFile - connectionString - [{_connectionString}]");

                    if (!string.IsNullOrWhiteSpace(_connectionString))
                    {
                        var parts = _connectionString.Split(';');
                        foreach (var item in parts)
                        {
                            if (item.StartsWith("Password"))
                            {
                                contents[4] = $"passwordUserDB,{_encryptor.Encrypt(item.Split('=').LastOrDefault()?.Trim())}";                                
                            }
                            else if (item.StartsWith("User ID"))
                            {
                                contents[3] = $"userNameDB,{item.Split('=').LastOrDefault()?.Trim()}";                                
                            }
                            else if (item.StartsWith("Initial Catalog"))
                            {
                                contents[2] = $"dataBaseName,{item.Split('=').LastOrDefault()?.Trim()}";
                            }
                            else if (item.StartsWith("Data Source"))
                            {
                                contents[1] = $"serverNameDB,{item.Split('=').LastOrDefault()?.Trim()}";
                            }
                        }
                    }
                    string userFronendApplicationRoute = jsonObj["GeneralSettings"]["UserFronendApplicationRoute"];
                    var domainNameValue = userFronendApplicationRoute.Split(':')[1].Replace("//", "").Replace("//", "");
                    contents[0] = $"domainName,{domainNameValue}";             
                    string documentsFolder = jsonObj["FolderSettings"]["Documents"];
                    var baseDataFolderValue = documentsFolder.Replace("\\Documents", "");
                    contents[5] = $"baseDataFolder,{baseDataFolderValue}";
                    string htmlPath = Path.Combine(Folders.ManagementFrontendPath, "index.html");
                    if (File.Exists(htmlPath))
                    {
                        var doc = new HtmlDocument();
                        doc.Load(htmlPath);
                        var metaElements = doc.DocumentNode.SelectNodes("//meta");
                        var metaNode = metaElements.FirstOrDefault(x => x.Attributes.FirstOrDefault(y => y.Name.ToLower() == "app-version".ToLower()) != null);
                        contents[6] = $"version,{metaNode.Attributes.FirstOrDefault()?.Value}";
                        var metaNode2 = metaElements.FirstOrDefault(x => x.Attributes.FirstOrDefault(y => y.Name.ToLower() == "data-api-endpoint".ToLower()) != null);
                        var allowRDPToManagementSiteValue = !metaNode2.Attributes.FirstOrDefault()?.Value.Contains("localhost");
                        contents[7] = $"allowRDPToManagementSite,{allowRDPToManagementSiteValue}";
                    }

                }
                jsonPath = Path.Combine(Folders.WseAuthFolder, "appsettings.json");
                if (File.Exists(jsonPath))
                {
                    string json = File.ReadAllText(jsonPath);
                    dynamic jsonObj = JsonConvert.DeserializeObject(json);                    
                    contents[8] = $"ADUserGroupName,{Convert.ToString(jsonObj["ADGeneralSettings"]["ADUserGroupName"])}";
                    contents[9] = $"ADSignerGroupName,{Convert.ToString(jsonObj["ADGeneralSettings"]["ADSignerGroupName"])}";
                }
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                File.WriteAllLines(path, contents);
            }
            catch (Exception ex)
            {
                _logHandler?.Error($"CreateCsvDataFile failed",ex);
            }
        }

        private void LoadDataFromCsvToUI()
        {
            string path = Path.Combine(Path.GetTempPath(), "data.csv");
            _logHandler?.Debug($"LoadDataFromCsvToUI from path [{path}]");
            if (File.Exists(path))
            {
                _logHandler?.Debug($"LoadDataFromCsvToUI -File [{path}] exist");

                try
                {
                   
                    var lines = File.ReadAllLines(path);
                    
                    var line = lines.FirstOrDefault(x => x.StartsWith("domainName"));
                    domainName.Text = line?.Split(',').Last();
                    line = lines.FirstOrDefault(x => x.StartsWith("serverNameDB"));
                    sqlDataSource.Text = line?.Split(',').Last();
                    
                    line = lines.FirstOrDefault(x => x.StartsWith("version"));
                    previousVersion.Content = $"Previous version : {line?.Split(',').Last()}";

                    line = lines.FirstOrDefault(x => x.StartsWith("dataBaseName"));
                    sqlDbName.Text = line?.Split(',').Last();
                    
                    line = lines.FirstOrDefault(x => x.StartsWith("userNameDB"));
                    sqlUserId.Text = line?.Split(',').Last();
                    
                    line = lines.FirstOrDefault(x => x.StartsWith("passwordUserDB"));
                    var cipherPassword = line?.Split(',').Last();
                    sqlPassword.Password = _encryptor.Decrypt(cipherPassword);                    

                    line = lines.FirstOrDefault(x => x.StartsWith("baseDataFolder"));
                    baseDataFolder.Text = string.IsNullOrWhiteSpace(line?.Split(',').Last()) ? @"D:\Comda\Wesign\Data" : line?.Split(',').Last();
                    
                    line = lines.FirstOrDefault(x => x.StartsWith("allowRDPToManagementSite"));
                    exposeManagement.IsChecked = line?.Split(',').Last()?.ToLower() == "true";

                    
                    line = lines.FirstOrDefault(x => x.StartsWith("ADUserGroupName"));
                    _adDetails.ADUserGroupName = line?.Split(',').Last();

                    line = lines.FirstOrDefault(x => x.StartsWith("ADSignerGroupName"));
                    _adDetails.ADSignerGroupName= line?.Split(',').Last();
                }
                catch 
                {

                }
            }
            else
            {
                _logHandler?.Debug($"LoadDataFromCsvToUI -File [{path}] NOT exist");
            }
        }

        private void InitSslCerts()
        {
            _sslNameToThumbprint = new Dictionary<string, string>
            {
                ["No SSL"] = ""
            };
            //sslList.Items.Add("");
            var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);
            foreach (var cert in store.Certificates)
            {
                string name = !string.IsNullOrWhiteSpace(cert.FriendlyName) ? cert.FriendlyName : cert.Subject.ToString();
                _sslNameToThumbprint[name] = cert.Thumbprint;
            }
            store.Close();
            foreach (var item in _sslNameToThumbprint)
            {
                //sslList.Items.Add(item.Key);
            }
        }

        private void SilentInstallation(object sender, RoutedEventArgs e)
        {
            try
            {
                SilentInstallationHandler silentInstallationHandler = new SilentInstallationHandler(_logHandler);
                silentInstallationHandler.InstallExeFiles();
            }
            catch (Exception ex)
            {
                _logHandler.Error("Error while SilentInstallation. ", ex);
            }
        }

        private void Submit(object sender, RoutedEventArgs e)
        {          
            try
            {
                submitButton.IsEnabled = false;
                ValidateInput();
                UpdateDB(sender, e);
                UpdateAppSettings(sender, e);
                UpdateHtml(sender, e);
                UpdateIIS(sender, e);
                UpdateOTP(sender, e);
                UpdateWeConfigFiles(sender, e);

                CreateCsvDataFile();
                MessageBox.Show("Finish Installation Successfully");
                App.Current.Shutdown();
            }
            catch (Exception ex)
            {
                _logHandler.Error($"Error in Submit ", ex);
                submitButton.IsEnabled = true;
                if (!(ex is InvalidOperationException))
                {
                    MessageBox.Show("Failed to complete installation, please check out in logs");
                }
            }
        }

        private void InitDbButtons()
        {           
            var directories = Directory.GetDirectories(Path.Combine(Folders.BaseFolder, "DB_Script"), "*", SearchOption.AllDirectories);

            string version = string.Empty;
            string path = Path.Combine(Path.GetTempPath(), "data.csv");
            if (File.Exists(path))
            {
                var lines = File.ReadAllLines(path);
                var line = lines.FirstOrDefault(x => x.StartsWith("version"));
                version = line?.Split(',').Last();
            }
            for (int i = 0; i < directories.Length; i++)
            {
                string folderName = Path.GetFileName(directories[i]);
                string v = folderName.Split('-').Last().Split('t','o').First();
                if (string.IsNullOrWhiteSpace(version) || (!string.IsNullOrWhiteSpace(version) && v == version))
                {
                    Button newBtn = new Button
                    {
                        Content = folderName,
                        Name = $"btn_{i}",
                        Height = 40,
                        Width = 140,
                        IsEnabled = false
                    };

                    newBtn.Click += new RoutedEventHandler(RunUpdateScript);
                    sp.Children.Add(newBtn);
                }
            }

        }

        private void UpdateDB(object sender, RoutedEventArgs e)
        {
            try
            {
                ValidateDBSection();
                var connectionString = GenerateConnectionString();

                Handlers.DbHandler dbHandler = new Handlers.DbHandler(_logHandler, connectionString);
                dbHandler.CreateWeSignDB(sqlDbName.Text.Trim());
                string sqlScriptFile = Path.Combine(Folders.BaseFolder, "DB_Script", "CreateWeSignTables.sql");
                dbHandler.CreateTables(sqlScriptFile);
                
                _logHandler.Debug("Successfully finish UpdateDB.");
                if(((Button)sender).Content.ToString() == "Create DB")
                {
                    MessageBox.Show("Finish update DB successfully");
                }
            }
            catch (Exception ex)
            {
                _logHandler.Error("Error while UpdateDB. ", ex);
            }
        }

        private void RunUpdateScript(object sender, RoutedEventArgs e)
        {
            try
            {
                ValidateDBSection();

                string folderName = ((Button)sender).Content.ToString();
                string dir = Path.Combine(Folders.BaseFolder, "DB_Script", folderName);
                var scripts = Directory.GetFiles(dir, "*", SearchOption.AllDirectories);
                string updateScript = Path.Combine(Folders.BaseFolder, "DB_Script", folderName, scripts.ToList().FirstOrDefault());

                var connectionString = GenerateConnectionString();
                Handlers.DbHandler dbHandler = new Handlers.DbHandler(_logHandler, connectionString);
                dbHandler.CreateTables(updateScript);

                _logHandler.Debug($"Successfully finish UpdateDB. Script [{updateScript}]");
                MessageBox.Show("Finish update DB successfully");
                
            }
            catch (Exception ex)
            {
                _logHandler.Error("Error while RunUpdateScript. ", ex);
            }
        }

        private void UpdateConfigFiles(object sender, RoutedEventArgs e)
        {
            try
            {
                ValidateConfigSection();
                ValidateDBSection();
                UpdateAppSettings(sender, e);
                UpdateHtml(sender, e);
                UpdateWeConfigFiles(sender, e);

                CreateCsvDataFile(isUpdateMode: true);
                MessageBox.Show("Finish update config files successfully");
                foreach (Button button in sp.Children)
                {
                        button.IsEnabled = true;
                }
                createSitesButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                _logHandler.Error("Error while UpdateConfigFiles. ", ex);
                MessageBox.Show($"Error - {ex.Message}");
            }
        }

        private void UpdateAppSettings(object sender, RoutedEventArgs e)
        {
            try
            {
                var connectionStringData = GenerateConnectionString();

                string connectionString = connectionStringData.ShouldUseWindowsAuthentication ? connectionStringData.ConnectionStringWithWindowsAuthentication : connectionStringData.ConnectionStringWithDbUser;

                AppsettingsHandler appsettingsHandler = new AppsettingsHandler(_logHandler);

                bool.TryParse(ConfigurationManager.AppSettings["UseHttpsForConfigFiles"], out bool isSecure);
                appsettingsHandler.UpdateAppSettingsFiles(connectionString, domainName.Text.Trim(), CAIpAndName.Text.Trim(), sitePortInput.Text.Trim(), baseDataFolder.Text.Trim(), managementPortInput.Text.Trim(), _adDetails , isSecure);
                _logHandler.Debug("Successfully finish UpdateAppSettings.");
            }
            catch (Exception ex)
            {
                _logHandler.Error("Error while UpdateAppSettings. ", ex);
            }
        }

        private void UpdateHtml(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(domainName.Text.Trim()))
                {
                    throw new InvalidOperationException("Validation error - please fill in domain name");
                }
                HtmlSettingsHandler htmlSettingsHandler = new HtmlSettingsHandler(_logHandler);
                bool.TryParse(ConfigurationManager.AppSettings["UseHttpsForConfigFiles"], out bool isSecure);                
                htmlSettingsHandler.UpdateApiEndpoints(domainName.Text.Trim(), sitePortInput.Text.Trim(), managementPortInput.Text.Trim(), (bool)exposeManagement.IsChecked, isSecure);
                _logHandler.Debug("Successfully finish UpdateHtml.");
            }
            catch (Exception ex)
            {
                _logHandler.Error("Error while UpdateHtml. ", ex);
            }

        }

        

        private void UpdateIIS(object sender, RoutedEventArgs e)
        {
            try
            {
                ValidateIISSection();
                SitesDetails sitesDetails = new SitesDetails
                {
                    ShouldUseDefaultWebSite = (bool)useDefaultWebSite.IsChecked,
                    MainSiteName = siteNameInput.Text.Trim(),
                    MainSitePort = int.Parse(sitePortInput.Text.Trim()),
                    ManagementSiteName = managementSiteNameInput.Text.Trim(),
                    ManagementSitePort = int.Parse(managementPortInput.Text.Trim())
                };
                IISHandler iisHandler = new IISHandler(_logHandler, sitesDetails);
                iisHandler.InstallSitesAndApplications(_chosenSslThumbprint);
                _logHandler.Debug("Successfully finish UpdateIIS.");
                if (((Button)sender).Content.ToString() == "Create Sites")
                {
                    MessageBox.Show("Finish update IIS successfully");
                }
            }
            catch (Exception ex)
            {
                _logHandler.Error("Error while UpdateIIS. ", ex);
            }
        }

        private void UpdateOTP(object sender, RoutedEventArgs e)
        {
            try
            {
                var connectionString = GenerateConnectionString();
                Handlers.DbHandler dbHandler = new Handlers.DbHandler(_logHandler, connectionString);
                dbHandler.UpdateOtpConfiguration(OTP.IsChecked);

                _logHandler.Debug("Successfully finish UpdateOTP.");
            }
            catch (Exception ex)
            {
                _logHandler.Error("Error while UpdateOTP. ", ex);
            }
        }

        private void UpdateWeConfigFiles(object sender, RoutedEventArgs e)
        {
            WebConfigsHandler webConfigsHandler = new WebConfigsHandler(_logHandler);

            string userSoapWebConfig = Path.Combine(Folders.UserSoapServicePath, "web.config");            
            bool.TryParse(ConfigurationManager.AppSettings["UseHttpsForConfigFiles"], out bool isSecure);
            string protocol = isSecure ? "https" : "http";
            string appPort = sitePortInput.Text.Trim();
            if (isSecure)
            {
                appPort = ConfigurationManager.AppSettings["SecureAppPort"];
            }
            string userapi = ConfigurationManager.AppSettings["UserApiApplicationRoute"];
            string userApiEndpoint = $"{protocol}://{domainName.Text.Trim()}:{appPort}/{userapi}";
            webConfigsHandler.UpdateSoapServiceConfig(userSoapWebConfig, userApiEndpoint);

            string managementWebConfig = Path.Combine(Folders.ManagementBackendPath, "web.config");
            string signerWebConfig = Path.Combine(Folders.SignerBackendPath, "web.config");
            string userWebConfig = Path.Combine(Folders.UserBackendPath, "web.config");
            webConfigsHandler.UpdateBackendWebConfig(managementWebConfig);
            webConfigsHandler.UpdateBackendWebConfig(signerWebConfig);
            webConfigsHandler.UpdateBackendWebConfig(userWebConfig);

            string managementFrontWebConfig = Path.Combine(Folders.ManagementFrontendPath, "web.config");
            string signerFrontWebConfig = Path.Combine(Folders.SignerFrontendPath, "web.config");
            string userFrontWebConfig = Path.Combine(Folders.UserFrontendPath, "web.config");
            string managementapi = ConfigurationManager.AppSettings["ManagementApiApplicationRoute"];            
            string signerapi = ConfigurationManager.AppSettings["SignerApiApplicationRoute"];

            webConfigsHandler.UpdateFrontendWebConfig(managementFrontWebConfig, new List<string> { managementapi });            
            webConfigsHandler.UpdateFrontendWebConfig(userFrontWebConfig, new List<string> { userapi, signerapi , "signer", "auth", "pdfExternal", "HistoryServiceApi" });
        }

        private void ValidateInput()
        {
            ValidateConfigSection();
            ValidateDBSection();
            ValidateIISSection();
        }

        private void ValidateIISSection()
        {
            string errorMessage = "Please fill in missing inputs: ";
            bool showErrorMessage = false;
            
            IsValidInput(sitePortInput.Text.Trim(), "Main Site Port", ref errorMessage, ref showErrorMessage);
            IsValidInput(managementSiteNameInput.Text.Trim(), "Management Site Name", ref errorMessage, ref showErrorMessage);
            IsValidInput(managementPortInput.Text.Trim(), "Management Site Port", ref errorMessage, ref showErrorMessage);
            if (!int.TryParse(sitePortInput.Text.Trim(), out _) || !int.TryParse(managementPortInput.Text.Trim(), out _))
            {
                errorMessage = $"{errorMessage}\nPort should be valid number";
                showErrorMessage = true;
            }

            if (showErrorMessage)
            {
                MessageBox.Show(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }
        }
        private void ValidateDBSection()
        {
            string errorMessage = "Please fill in missing inputs: ";
            bool showErrorMessage = false;
            IsValidInput(sqlDbName.Text.Trim(), "DB Name", ref errorMessage, ref showErrorMessage);
            IsValidInput(sqlDataSource.Text.Trim(), "Server Name", ref errorMessage, ref showErrorMessage);
            IsValidInput(sqlUserId.Text.Trim(), "User Id", ref errorMessage, ref showErrorMessage);
            if (!windowsAuth.IsChecked ?? false)
            {
                IsValidInput(sqlPassword.Password, "Password", ref errorMessage, ref showErrorMessage);
            }
            if ((bool)useNewWebSite.IsChecked)
            {
                IsValidInput(siteNameInput.Text.Trim(), "Site Name", ref errorMessage, ref showErrorMessage);
            }

            if (showErrorMessage)
            {
                MessageBox.Show(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }
        }
        private void ValidateConfigSection()
        {
            string errorMessage = "Please fill in missing inputs: ";
            bool showErrorMessage = false;
            
            IsValidInput(domainName.Text.Trim(), "Domain Name", ref errorMessage, ref showErrorMessage);
            IsValidInput(baseDataFolder.Text.Trim(), "Data base folder", ref errorMessage, ref showErrorMessage);
            if ((bool)caOption1.IsChecked || (bool)caOption2.IsChecked)
            {
                IsValidInput(CAIpAndName.Text.Trim(), "CA Name", ref errorMessage, ref showErrorMessage);
            }
            Directory.CreateDirectory(baseDataFolder.Text.Trim());
            //if (!Directory.Exists(baseDataFolder.Text.Trim()))
            //{
            //    errorMessage = errorMessage == "Please fill in missing inputs: " ? string.Empty : $"{errorMessage}.\n\n";                
            //    errorMessage = $"{errorMessage}Base data folder [{baseDataFolder.Text.Trim()}] not exists";
            //    showErrorMessage = true;
            //}

            if (showErrorMessage)
            {
                MessageBox.Show(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }
        }

        private void IsValidInput(string inputName, string inputDescription, ref string errorMessage, ref bool showErrorMessage)
        {
            if (string.IsNullOrWhiteSpace(inputName))
            {
                errorMessage = $"{errorMessage}\n{inputDescription}";
                showErrorMessage = true;
            }
        }

        private ConnectionStringData GenerateConnectionString()
        {
            if (string.IsNullOrWhiteSpace(sqlDataSource.Text.Trim()))
            {
                throw new InvalidOperationException("Validation error - please fill in SQL Data Source");
            }
            ConnectionStringData result = new ConnectionStringData
            {
                ConnectionStringWithWindowsAuthentication = $"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog={sqlDbName.Text.Trim()};TrustServerCertificate=True;Data Source={sqlDataSource.Text.Trim().ToUpper()}",
                ConnectionStringWithDbUser = $"Password={sqlPassword.Password};Persist Security Info=True;User ID={sqlUserId.Text.Trim()};Initial Catalog={sqlDbName.Text.Trim()};TrustServerCertificate=True;Data Source={sqlDataSource.Text.Trim().ToUpper()}",
                ShouldUseWindowsAuthentication = (bool)windowsAuth.IsChecked
            };
            
            return result;
        }

        #region UI Handlers

        private void PCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (submitButton != null)
                {
                    submitButton.IsEnabled = false;
                }
            }
            catch (Exception) {
                // do nothing
            }
        }

        private void PCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (submitButton != null)
                {
                    submitButton.IsEnabled = true;
                }
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        private void useNewWebSite_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (siteNameInput != null)
                {
                    siteNameInput.IsEnabled = true;
                    sitePortInput.IsEnabled = true;
                }
            }
            catch (Exception) {
                // do nothing
            }
        }

        private void useDefaultWebSite_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (siteNameInput != null)
                {
                    siteNameInput.IsEnabled = false;
                    sitePortInput.IsEnabled = false;
                    if (string.IsNullOrWhiteSpace(sitePortInput.Text.Trim()))
                    {
                        sitePortInput.Text = "80";
                    }
                }
            }
            catch (Exception) { 
            // do nothing
             }
        }

        private void sslList_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            //if (sslList != null && sslList.SelectedValue != null && _sslNameToThumbprint.ContainsKey(sslList.SelectedValue.ToString()))
            //{
            //    _chosenSslThumbprint = _sslNameToThumbprint[sslList.SelectedValue.ToString()];
            //}

        }

        private void caOption2_Checked(object sender, RoutedEventArgs e)
        {
            CAIpAndName.IsEnabled = true;
        }

        private void caOption1_Checked(object sender, RoutedEventArgs e)
        {
            CAIpAndName.IsEnabled = true;
        }

        private void caOption3_Checked(object sender, RoutedEventArgs e)
        {
            if (CAIpAndName != null)
            {
                CAIpAndName.IsEnabled = false;
            }
        }

        #endregion

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            ResetPopUp();
        }

        private void ResetPopUp()
        {
            var offset = popup.HorizontalOffset;
            popup.HorizontalOffset = offset + 1;
            popup.HorizontalOffset = offset;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _logHandler.Debug($"Window_SizeChanged");

        }

        private async  void backupButton_Click(object sender, RoutedEventArgs e)
        {
            pbStatus.Visibility = Visibility.Visible;
            configurationSection.IsEnabled = false;
            backupMessage.Visibility = Visibility.Visible;
            backupButton.Visibility = Visibility.Collapsed;

            await Task.Run(()=>
            {
                try
                {
                    string currAppFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    string baseWeSignSitesFolder = Directory.GetParent(currAppFolder).FullName;
                    string backupFolder = Path.Combine(Directory.GetParent(baseWeSignSitesFolder).FullName, "Backup");
                    Directory.CreateDirectory(backupFolder);
                    BackupSitesSourceCode(backupFolder);
                    GenerateDbScriptIncludeTablesAndData(backupFolder);
                    MessageBox.Show("Finish BACK UP Successfully");
                }
                catch (Exception ex)
                {
                    _log?.Error($"BACK UP task failed", ex);
                    MessageBox.Show("BACK UP task failed, read logs for extra details");
                }     
            });

            backupButton.Visibility = Visibility.Visible;
            backupMessage.Visibility = Visibility.Collapsed;
            pbStatus.Visibility = Visibility.Collapsed;
            configurationSection.IsEnabled = true;

        }

        private void BackupSitesSourceCode(string backupFolder)
        {
            _log?.Debug($"Start backup sites source code to backupFolder [{backupFolder}]");
            
            string managmentBack = Path.Combine(backupFolder, $"Sites_{DateTime.Now:dd-MM-yy}", "ManagementBackend");
            Directory.CreateDirectory(managmentBack);
            CopyFilesRecursively(Folders.ManagementBackendPath, managmentBack);
            
            string signerBack = Path.Combine(backupFolder, $"Sites_{DateTime.Now:dd-MM-yy}", "SignerBackend");
            Directory.CreateDirectory(signerBack);
            CopyFilesRecursively(Folders.SignerBackendPath, signerBack);

            string userBack = Path.Combine(backupFolder, $"Sites_{DateTime.Now:dd-MM-yy}", "UserBackend");
            Directory.CreateDirectory(userBack);
            CopyFilesRecursively(Folders.UserBackendPath, userBack);

            string managmentFront = Path.Combine(backupFolder, $"Sites_{DateTime.Now:dd-MM-yy}", "ManagementFrontend");
            Directory.CreateDirectory(managmentFront);
            CopyFilesRecursively(Folders.ManagementFrontendPath, managmentFront);
            
            string signerFront = Path.Combine(backupFolder, $"Sites_{DateTime.Now:dd-MM-yy}", "SignerFrontend");
            Directory.CreateDirectory(signerFront);
            CopyFilesRecursively(Folders.SignerFrontendPath, signerFront);

            string userFront = Path.Combine(backupFolder, $"Sites_{DateTime.Now:dd-MM-yy}", "UserFrontend");
            Directory.CreateDirectory(userFront);
            CopyFilesRecursively(Folders.UserFrontendPath, userFront);

            string wseAuth = Path.Combine(backupFolder, $"Sites_{DateTime.Now:dd-MM-yy}", "WSEAuth");
            Directory.CreateDirectory(wseAuth);
            CopyFilesRecursively(Folders.WseAuthFolder, wseAuth);

            string smartCard = Path.Combine(backupFolder, $"Sites_{DateTime.Now:dd-MM-yy}", "SmartCardDesktopClient");
            Directory.CreateDirectory(smartCard);
            CopyFilesRecursively(Path.Combine(Directory.GetParent(Folders.ManagementBackendPath).FullName, "SmartCardDesktopClient"), smartCard);

            string dbscripts = Path.Combine(backupFolder, $"Sites_{DateTime.Now:dd-MM-yy}", "DB_Script");
            Directory.CreateDirectory(dbscripts);            
            CopyFilesRecursively(Path.Combine(Directory.GetParent(Folders.ManagementBackendPath).FullName, "DB_Script"), dbscripts);
            
            _log?.Info($"Successfully backup sites source code to [{backupFolder}]");
        }

        private void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        private void GenerateDbScriptIncludeTablesAndData(string backupFolder)
        {
            _log?.Debug($"Start backup db");
            
            Server server = new Server(new ServerConnection(new SqlConnection(_connectionString)));
            var parts = _connectionString.Split(';');
            string dbName = parts[3].Split('=').Last();
            Database database = server.Databases[dbName];
            ScriptingOptions options = new ScriptingOptions
            {
                ScriptData = true,
                ScriptSchema = true,
                ScriptDrops = false,
                Indexes = true,
                IncludeHeaders = true
            };

            var script = new StringBuilder();
            foreach (Table table in database.Tables)
            {
                if (table.Name.ToLower().Contains("logs"))
                {
                    continue;
                }
                foreach (var statement in table.EnumScript(options))
                {
                    script.Append(statement);
                    script.Append(Environment.NewLine);
                }
            }
            
            string destPath = Path.Combine(backupFolder, $"{dbName}_{DateTime.Now.ToString("dd-MM-yy_HH-mm")}.sql");
            File.WriteAllText(destPath, script.ToString());
            _log?.Info($"Successfully backup db to [{destPath}]");
        }

        private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            sqlPassword.Visibility = sqlPassword.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            textSqlPassword.Visibility = textSqlPassword.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
                
        private void sqlPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if(textSqlPassword.Text != sqlPassword.Password)
            {
                textSqlPassword.Text = sqlPassword.Password;            
            }
        }

        private void textSqlPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textSqlPassword.Text != sqlPassword.Password)
            {
                sqlPassword.Password = textSqlPassword.Text;
            }
        }

        private void textSqlPassword_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }
    }
}
