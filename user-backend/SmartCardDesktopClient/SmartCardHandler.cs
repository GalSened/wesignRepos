using CTHashSigner;
using CTInterfaces;
using CTXmlSigner;
using Microsoft.AspNetCore.SignalR.Client;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartCardDesktopClient
{
    public class SmartCardHandler : ApplicationContext
    {
        public string userPinCode = "";

        private Logger _logger;
        private NotifyIcon _notifyIcon;

        private Dictionary<CardProvider, string> _providerToDllName;

        private X509Certificate2 _xcer;

        private string _selectedDll = "";
        private string _selectedSlotLavel = "";

        private bool _slotById = false;

        private string _host;
        private string _roomId;
        private HubConnection _connection;
        public bool AppClosed = false;
        static PasswordForm _form = null;
        private bool signProcessDoneSuccessfully = false;

        public Timer timer = new Timer();

        public SmartCardHandler(string roomId, string host)
        {

            _logger = new Logger();
            _roomId = roomId;
            _host = host;

            _providerToDllName = new Dictionary<CardProvider, string>
            {
                [CardProvider.Athena] = "athenaCSP.dll",
                [CardProvider.Gemalto] = "eTokenMD.dll",
                [CardProvider.None] = "etpkcs11.dll",
                [CardProvider.NXP] = "asepkcs.dll"
            };

            InitTrayUI();

            InitHubConnection();

            timer = new Timer()
            {
                Interval = 90000,
                Enabled = false,
            };

            timer.Tick += new EventHandler(OnTimerEvent);
        }

        private void OnTimerEvent(object sender, EventArgs e)
        {
            _logger?.Debug("Exit request from timer");
            DoExit(null, null);
        }

        private void InitTrayUI()
        {
            MenuItem exitMenuItem = new MenuItem("Exit", new EventHandler(DoExit));
            _notifyIcon = new NotifyIcon();
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources", "comsigntrust_fav_rL6_icon.ico");

            _notifyIcon.Icon = new System.Drawing.Icon(path);
            _notifyIcon.ContextMenu = new ContextMenu(new MenuItem[] { exitMenuItem });
            _notifyIcon.Visible = true;
            PopupBallonTip("Choose Your Certificate");
        }

        public void PopupBallonTip(string title)
        {
            _notifyIcon.BalloonTipTitle = title;
            _notifyIcon.BalloonTipText = "Open minimize application";
            _notifyIcon.BalloonTipIcon = ToolTipIcon.Warning;
            _notifyIcon.ShowBalloonTip(300000);
            _notifyIcon.BalloonTipClicked += new EventHandler(BalloonTipClickedHandler);
        }

        private void BalloonTipClickedHandler(object sender, EventArgs e)
        {
            if (sender is NotifyIcon && ((NotifyIcon)sender).BalloonTipTitle == "Insert Pin Code")
            {

            }
        }

        public void ShowPinCodeInsertionForm()
        {
            if (AppClosed)
                return;

            PopupBallonTip("Insert Pin Code");
            _logger.Debug($"ShowPinCodeInsertionForm...").GetAwaiter().GetResult();

            if (_form != null)
            {
                _form.Dispose();
            }
            _form = new PasswordForm(_logger, _xcer, this);
            _form.WindowState = FormWindowState.Minimized;
            _form.Show();
            _form.WindowState = FormWindowState.Normal;
        }

        public void ShowCertificateSelctionUI()
        {
            LoadCertFromUserSelection();
            ShowPinCodeInsertionForm();
        }

        private void DoExit(object sender, EventArgs e)
        {
            // We must manually tidy up and remove the icon before we exit.
            // Otherwise it will be left behind until the user mouses over.

            _logger.Debug("In Do Exit").GetAwaiter().GetResult();
            _notifyIcon.Visible = false;
            AppClosed = true;
            _logger.Debug("In Do Exit stop timer").GetAwaiter().GetResult();
            timer.Stop();
            _logger.Debug("In Do Exit hub Disconnerct").GetAwaiter().GetResult();
            TryToDisconnect();
            _logger.Debug("In Do Exit dispose form").GetAwaiter().GetResult();
            if (_form != null)
            {
                _form.Close();
                _form.Dispose();
            }
            if (timer != null)
            {
                timer.Dispose();
            }
            _logger.Debug("In Do Exit tray icon").GetAwaiter().GetResult();
            _notifyIcon.Dispose();
            _logger.Debug("In Do Exit close app ").GetAwaiter().GetResult();
            Application.Exit();
        }

        public void SigningHubProcess()
        {
            if (!IsValidCertificateAndPassword())
            {
                PopupBallonTip("One of the selected parameters are wrong please select again");
                ShowCertificateSelctionUI();
                return;
            }

            Task.Run(async () =>
            {
                try
                {
                    await SigningProcess();
                }
                catch (Exception ex)
                {
                    _logger.Error($"SigningHubProcess - Error in SigningProcess - {ex}").GetAwaiter().GetResult();
                    DoExit(null, null);
                }
            });
        }

        private bool IsValidCertificateAndPassword()
        {
            try
            {
                byte[] hash = SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes("TEST"));
                var result = SignHash(hash);
                _logger.Debug($"IsValidCertificateAndPassword - result from signHash - {result.Result}").GetAwaiter().GetResult();
                return result.Result == CTResult.SUCCESS;
            }

            catch (Exception ex)
            {
                _logger.Error($"#IsValidCertificateAndPassword,  {ex}").GetAwaiter().GetResult();
            }

            return false;
        }

        private Task SigningProcess()
        {
            _connection.On<byte[], string, string/*, bool?*/>("GetHash",
                   async (byte[] hash, string fieldName, string roomId/*, bool? isGovDoc*/) =>
                   {
                       try
                       {
                           if (roomId == _roomId)
                           {
                               //XML_SIG_CONTEXT sigContext = null;
                               //bool isGovDocVal = isGovDoc.HasValue ? isGovDoc.Value : false;
                               //if (isGovDocVal)
                               //{
                               //    sigContext = PrepareGovSignature(hash, fieldName);
                               //    hash = sigContext.Hash;
                               //}
                               _logger.Debug($"SigningProcess - Get Hash - hash size- {hash.Length}").GetAwaiter().GetResult();
                               //_logger.Debug($"SigningProcess - is gov doc - {isGovDocVal}").GetAwaiter().GetResult();
                               var response = SignHash(hash/*, isGovDoc ?? false*/);
                               _logger.Debug($"SigningProcess - Sign Hash result - {Enum.GetName(typeof(CTResult), response.Result)}").GetAwaiter().GetResult();
                               if (response.Result == CTResult.SUCCESS)
                               {
                                   //NOT INCLUDE GOVERMENT FEATURE
                                   _logger.Debug($"SigningProcess - Call to SetSignatureAsync- {_roomId}").GetAwaiter().GetResult();
                                   await _connection.InvokeAsync("SetSignatureAsync", _roomId, response.SignedHash, fieldName);

                                   //INCLUDE GOVERNMENT FEATURE
                                   //if (isGovDocVal)
                                   //{
                                   //    _logger.Debug($"SigningProcess - Call to SetGovSignature- {_roomId}").GetAwaiter().GetResult();
                                   //    _logger.Debug($"SigningProcess - {response.SignedHash.Length}").GetAwaiter().GetResult();
                                   //    sigContext.Hash = response.SignedHash;
                                   //    XmlSigner signer = new XmlSigner();
                                   //    var res = signer.SetSignature(sigContext);
                                   //    _logger.Debug($"SetSignature -  {Enum.GetName(typeof(CTResult), res)}").GetAwaiter().GetResult();
                                   //    await _connection.InvokeAsync("SetGovSignature", _roomId, sigContext.Xml, fieldName);


                                   //}
                                   //else
                                   //{
                                   //    _logger.Debug($"SigningProcess - Call to SetSignatureAsync- {_roomId}").GetAwaiter().GetResult();
                                   //    await _connection.InvokeAsync("SetSignatureAsync", _roomId, response.SignedHash, fieldName);
                                   //}
                               }
                               else
                               {
                                   _logger.Debug($"SigningProcess - Call to SendErrorMessage - {_roomId}").GetAwaiter().GetResult();
                                   await _connection.InvokeAsync("SendErrorMessage", _roomId, "Failed to SignHash " + Enum.GetName(typeof(CTResult), response.Result));
                               }
                           }
                       }

                       catch (Exception ex)
                       {
                           try
                           {
                               _logger.Error($"SigningProcess - Desktop client Failed to SignHash! {ex}").GetAwaiter().GetResult();
                               await _connection.InvokeAsync("SendErrorMessage", _roomId, ex.ToString());
                               DoExit(null, null);
                           }
                           catch (Exception)
                           {
                               DoExit(null, null);
                           }
                       }
                   });

            _connection.On<string, string>("GetMessage",
                 (string message, string roomId) =>
            {
                if (roomId == _roomId)
                {
                    if (message.ToLower().Contains("success"))
                    {

                        signProcessDoneSuccessfully = true;
                        _logger.Debug($"SigningProcess - Desktop client GetMessage from SmartCardSigningHub : {message}").GetAwaiter().GetResult();
                    }

                    else
                    {
                        _logger.Error($"SigningProcess - Desktop client GetMessage from SmartCardSigningHub : {message}").GetAwaiter().GetResult();
                    }

                    DoExit(null, null);
                }
            });

            _logger.Debug($"SigningProcess - invoke connection to roomId- {_roomId}").GetAwaiter().GetResult();
            return _connection.InvokeAsync("Connect", _roomId);
        }

        public void TryToDisconnect()
        {
            try
            {
                if (!signProcessDoneSuccessfully)
                {
                    _connection.InvokeAsync("SendErrorMessage", _roomId, $"Desktop client send a disconnection request");

                }
            }

            catch (Exception err)
            {
                _logger.Error($"TryToDisconnect  {err}").GetAwaiter().GetResult();
            }
        }

        private async Task InitHubConnection()
        {
            _logger.Debug($"InitHubConnection - Connecting to hub at : {_host}/smartcardsocket").GetAwaiter().GetResult();
            _connection = new HubConnectionBuilder().WithUrl($"{_host}/smartcardsocket").WithAutomaticReconnect().Build();

            try
            {
                await _connection.StartAsync();
                _logger.Info("InitHubConnection - Desktop client connected!").GetAwaiter().GetResult();
            }

            catch (Exception ex)
            {
                try
                {
                    _connection = new HubConnectionBuilder().WithUrl($"{_host}/smartcardsocket",
                    Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling).WithAutomaticReconnect().Build();
                    await _connection.StartAsync();
                    _logger.Info("InitHubConnection - Desktop client connected! using LongPolling").GetAwaiter().GetResult();
                }

                catch (Exception ex2)
                {

                    _logger.Error($"InitHubConnection - Desktop client Failed to connected! {ex}").GetAwaiter().GetResult();
                    _logger.Error($"InitHubConnection - Desktop client Failed to connected!  using LongPolling {ex2}").GetAwaiter().GetResult();

                    try
                    {
                        await _connection.InvokeAsync("SendErrorMessage", _roomId, $"InitHubConnection - Desktop client Failed to connected! {ex2.Message}");
                    }

                    catch (Exception err)
                    {
                        _logger.Error($"InitHubConnection - Exit application ,  {err}").GetAwaiter().GetResult();
                        DoExit(null, null);
                    }
                }
            }
        }

        public void LoadCertFromUserSelection()
        {
            _logger.Debug($"LoadCertFromUserSelection - open store certificate for client to select certificate").GetAwaiter().GetResult();

            using (var xstore = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                xstore.Open(OpenFlags.MaxAllowed);
                var certs = xstore.Certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, true);
                List<X509Certificate2> certToRemove = new List<X509Certificate2>();

                foreach (var cert in certs)
                {
                    if (!cert.HasPrivateKey)
                    {
                        certToRemove.Add(cert);
                    }
                }

                if (certToRemove.Count > 0)
                {
                    certs.RemoveRange(certToRemove.ToArray());
                }

                X509Certificate2Collection selected;

                do
                {
                    selected = X509Certificate2UI.SelectFromCollection(certs, "Please choose...", "", X509SelectionFlag.SingleSelection);
                    if (selected.Count == 0 || !selected[0].HasPrivateKey)
                    {

                        var result = MessageBox.Show($"A certificate is not selected or the selected certificate is without a private key.{Environment.NewLine}To Exit this application press (Yes),{Environment.NewLine}To Choose a different certificate press (No)?", "Selected Certificate issues",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, MessageBoxOptions.DefaultDesktopOnly);
                        if (result == DialogResult.Yes)
                        {
                            _logger.Error("There is no certificate with private key chosen, exit app...").GetAwaiter().GetResult();
                            DoExit(null, null);
                            return;
                        }

                        else
                        {
                            PopupBallonTip("Choose Your Certificate");
                        }
                    }
                } while (selected == null || selected.Count == 0 || !selected[0].HasPrivateKey);

                _logger.Debug($"LoadCertFromUserSelection - selected certificate FriendlyName=[{selected[0].FriendlyName}]").GetAwaiter().GetResult();
                _xcer = selected[0];
            }
        }

        public SignHashResult SignHash(byte[] hash, bool isGovDoc = false)
        {
            if (string.IsNullOrWhiteSpace(_selectedDll) || string.IsNullOrWhiteSpace(_selectedSlotLavel))
            {
                SignHashResult signHashResult = null;
                var drivers = GetDriversName();

                foreach (var driver in drivers)
                {
                    try
                    {
                        if (driver.Value.Count > 1)
                        {
                            foreach (var slot in driver.Value)
                            {
                                try
                                {

                                    var itemsWithTheSameName = driver.Value.GroupBy(x => x.Label ?? "Comsign").Where(x => x.Count() > 1).Select(x => x.Key);

                                    if (itemsWithTheSameName.Any())
                                    {
                                        _logger.Debug($"SignHash entered if itemsWithSameName - isGovDoc - {isGovDoc}").GetAwaiter().GetResult();
                                        signHashResult = DoSignHash(hash, driver.Key, (slot?.Id ?? 0).ToString(), isGovDoc, true);
                                    }
                                    else
                                    {
                                        _logger.Debug($"SignHash entered else itemsWithSameName - isGovDoc - {isGovDoc}").GetAwaiter().GetResult();
                                        signHashResult = DoSignHash(hash, driver.Key, slot?.Label ?? "Comsign", isGovDoc);
                                    }

                                    if (signHashResult.Result == CTResult.SUCCESS)
                                    {
                                        _selectedDll = driver.Key;
                                        if (itemsWithTheSameName.Any())
                                        {
                                            _slotById = true;
                                            _selectedSlotLavel = (slot?.Id ?? 0).ToString();
                                        }

                                        else
                                        {
                                            _selectedSlotLavel = slot?.Label ?? "Comsign";
                                        }

                                        return signHashResult;
                                    }
                                }

                                catch (Exception ex)
                                {
                                    _logger.Error($"SigningHubProcess - Error in SignHash  Searching for Correct Driver - {ex}").GetAwaiter().GetResult();
                                }
                            }
                        }
                        else
                        {
                            _logger.Debug($"SignHash entered else driver.Value.Count > 1 - isGovDoc - {isGovDoc}").GetAwaiter().GetResult();
                            signHashResult = DoSignHash(hash, driver.Key, driver.Value?.FirstOrDefault()?.Label ?? "Comsign", isGovDoc);
                            if (signHashResult.Result == CTResult.SUCCESS)
                            {
                                _selectedDll = driver.Key;
                                _selectedSlotLavel = driver.Value?.FirstOrDefault()?.Label ?? "Comsign";
                                return signHashResult;
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        _logger.Error($"SigningHubProcess - Error in SignHash  Searching for Correct Driver - {ex}").GetAwaiter().GetResult();
                    }
                }

                _logger.Info("Driver not found for signing !!!!!").GetAwaiter().GetResult();
                return signHashResult;

            }

            return DoSignHash(hash, _selectedDll, _selectedSlotLavel, isGovDoc, _slotById);
        }

        private SignHashResult DoSignHash(byte[] hash, string dll, string slotIdentifire, bool isGovDoc = false, bool signBySlotId = false)
        {
            var certData = new CERT_DATA()
            {
                BuildChain = true,
                SignCert = _xcer
            };
            CRED_DATA credData = null;
            if (signBySlotId)
            {
                credData = new CRED_DATA()
                {
                    Driver = dll,
                    Slot = int.Parse(slotIdentifire),
                    PIN = userPinCode,
                    SlotFind = SLOT_FIND_BY.SLOT_ID
                };
            }

            else
            {
                credData = new CRED_DATA()
                {
                    Driver = dll,
                    Label = slotIdentifire,
                    PIN = userPinCode,
                    SlotFind = SLOT_FIND_BY.LABEL
                };
            }
            ISign iSign = new CSign();
            SIGN_DATA sIGN_DATA = new SIGN_DATA()
            {
                DigestAlg = CTDigestAlg.SHA256,
                RsaSigMode = RsaSigMode.PKCS1
            };
            if (isGovDoc)
            {
                _logger.Debug($"SignHash - Start hash using SignRaw").GetAwaiter().GetResult();
                (CTResult result, byte[] signedHash) = iSign.SignRaw(credData, certData, sIGN_DATA, hash);
                _logger.Debug($"SignHash - SignRaw result [{result}]").GetAwaiter().GetResult();
                return new SignHashResult(result, signedHash);
            }
            else
            {
                _logger.Debug($"SignHash - Start hash using SignCMS").GetAwaiter().GetResult();
                (CTResult result, byte[] signedHash) = iSign.SignCMS(credData, certData, sIGN_DATA, null, hash, null);
                _logger.Debug($"SignHash - SignCMS result [{result}]").GetAwaiter().GetResult();
                return new SignHashResult(result, signedHash);
            }
        }

        private XML_SIG_CONTEXT PrepareGovSignature(byte[] fileContent, string fileName)
        {
            _logger.Debug("PrepareGovSignature - starting process").GetAwaiter().GetResult();
            XmlSigner xmlSigner = new XmlSigner();
            CERT_DATA cert_data = new CERT_DATA
            {
                SignCert = _xcer,
                BuildChain = true
            };
            XML_SIG_CONTEXT sigContext = new XML_SIG_CONTEXT
            {
                CertData = cert_data,
                SigLevel = XADES_SIG_LEVEL.XMLDsig,
                DigestAlg = CTDigestAlg.SHA256,
                XmlEncoding = Encoding.UTF8,
                SignCertificate = cert_data.SignCert,
                CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl,
                PreserveWhitespace = false
            };
            string name = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            _logger.Debug($"PrepareGovSignature - working on file {fileName}").GetAwaiter().GetResult();
            CTResult res = xmlSigner.PrepareMechesSignature(fileContent, name, extension, sigContext);
            _logger.Debug($"PrepareGovSignature - {res}").GetAwaiter().GetResult();
            return sigContext;
        }

        protected Dictionary<string, List<SLOT_DATA>> GetDriversName()
        {

            Dictionary<string, List<SLOT_DATA>> result = new Dictionary<string, List<SLOT_DATA>>();
            var providerName = (_xcer.PrivateKey as RSACryptoServiceProvider).CspKeyContainerInfo.ProviderName;
            _logger.Debug($"GetDriverName - Selected signing driver provider name [{providerName}]").GetAwaiter().GetResult();
            CardProvider cardProvider = providerName.Contains("eToken") ? CardProvider.Gemalto : providerName.StartsWith("Athena") ? CardProvider.Athena : CardProvider.None;
            _logger.Debug($"GetDriverName - cardProvider enum [{cardProvider}]").GetAwaiter().GetResult();
            var dll = _providerToDllName[cardProvider];
            _logger.Debug($"GetDriverName - DLL for use [{dll}]").GetAwaiter().GetResult();

            foreach (var provider in _providerToDllName)
            {
                try
                {

                    if (TryGetSlot(provider.Value, out List<SLOT_DATA> slotDataList))
                    {
                        dll = provider.Value;
                        result.Add(dll, slotDataList);
                    }
                }

                catch (Exception ex)
                {
                    _logger.Error($"GetDriverName - Make sure that smart card drivers installed...{ex}").GetAwaiter().GetResult();
                }
            }

            Dictionary<string, List<SLOT_DATA>> processedResult = new Dictionary<string, List<SLOT_DATA>>();
            foreach (var pair in result)
            {


                CSign iSign1 = new CSign();
                _logger.Debug($"GetDriverName -for DLL [{dll}]").GetAwaiter().GetResult();


                iSign1.GetSlots(pair.Key, out List<SLOT_DATA> slotData);
                processedResult.Add(pair.Key, slotData);

                _logger.Debug($"GetDriverName - {pair.Key} : iSign1.GetSlots slotDataList result count [{slotData.Count()}]").GetAwaiter().GetResult();
            }

            return processedResult;
        }

        private bool TryGetSlot(string dll, out List<SLOT_DATA> slotDataList)
        {
            try
            {
                CSign iSign1 = new CSign();

                _logger.Debug($"TryGetSlot - for DLL [{dll}]").GetAwaiter().GetResult();
                iSign1.GetSlots(dll, out slotDataList);
                _logger.Debug($"TryGetSlot - iSign1.GetSlots slotDataList result count [{slotDataList.Count}]").GetAwaiter().GetResult();
                if (slotDataList.Count > 0)
                {
                    _logger.Debug($"TryGetSlot - found slot for DLL [{dll}]: found {slotDataList.Count} slots").GetAwaiter().GetResult();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"TryGetSlot - Make sure that smart card drivers installed... {ex}").GetAwaiter().GetResult();
            }
            slotDataList = new List<SLOT_DATA>();
            _logger.Debug($"TryGetSlot - NOT found slot for DLL [{dll}]").GetAwaiter().GetResult();
            return false;
        }
    }

    public enum CardProvider
{
    None = 0,
    Athena = 1,
    Gemalto = 2,
    NXP = 3
}
}