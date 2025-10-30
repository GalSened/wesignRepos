namespace Common.Handlers
{
    using CERTCLILib;
    using CERTENROLLLib;
    using Certificate.Interfaces;
    using Common.Enums.Results;
    using Common.Extensions;
    using Common.Interfaces;
    using Common.Interfaces.Files;
    using Common.Models;
    using Common.Models.Configurations;
    using Common.Models.Settings;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using MimeKit.Cryptography;
    using Serilog;
    using System;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Caching;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;


    public class CertificatesHandler : ICertificate
    {
        private const int CR_DISP_ISSUED = 0x3;
        private const string PFX_PASSWORD = "123456";
        private const string X509_MEM_KEY = "GlobalX509MemoryKey";


        private readonly ILogger _logger;
        private readonly ICertificateCreator _certificateCreator;
        private readonly IFilesWrapper _filesWrapper;
        private readonly IMemoryCache _memoryCache;
        private readonly GeneralSettings _generalSettings;


        public CertificatesHandler(IOptions<GeneralSettings> generalSettings, ILogger logger,
            ICertificateCreator certificateCreator, IFilesWrapper filesWrapper, IMemoryCache memoryCache)
        {

            _generalSettings = generalSettings.Value;
            _logger = logger;
            _certificateCreator = certificateCreator;
            _filesWrapper = filesWrapper;
            _memoryCache = memoryCache;

        }

        #region Contact

        public void Create(Contact contact, CompanyConfiguration companyConfiguration)
        {
            if (!string.IsNullOrWhiteSpace(_generalSettings.CA) || companyConfiguration.IsPersonzliedPFX)
            {
                if (contact == null || contact?.Id == null || contact?.Id == Guid.Empty)
                {
                    throw new InvalidOperationException(ResultCode.InvalidContactId.GetNumericString());
                }

                if (!_filesWrapper.Contacts.IsCertificateExist(contact))
                {
                    string dn = CreateDN(contact.Name, contact.Email, contact.Phone);
                    var cert = CreateCertificate(dn);
                    _filesWrapper.Contacts.SaveCertificate(contact, cert);
                    _logger.Information($"Successfully create certificate for contact [{contact.Id}]");
                }


            }
        }

        public void Delete(Contact contact)
        {
            if (_filesWrapper.Contacts.IsCertificateExist(contact))
            {
                _filesWrapper.Contacts.DeleteCertificate(contact);
            }

        }

        public X509Certificate2 Get(Contact contact, CompanyConfiguration companyConfiguration)
        {

            if (!companyConfiguration.IsPersonzliedPFX && ShouldUseGlobalCertificate())
            {
                return GetGlobalCert();
            }
            if (contact == null || contact?.Id == null || contact?.Id == Guid.Empty)
            {
                throw new InvalidOperationException(ResultCode.InvalidContactId.GetNumericString());
            }
            if (!_filesWrapper.Contacts.IsCertificateExist(contact))
            {
                Create(contact, companyConfiguration);
            }
            byte[] certBytes = _filesWrapper.Contacts.ReadCertificate(contact, companyConfiguration);

            return new X509Certificate2(certBytes, PFX_PASSWORD, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
        }



        #endregion

        #region User

        public void Create(User user, CompanyConfiguration companyConfiguration)
        {
            if (!string.IsNullOrWhiteSpace(_generalSettings.CA) || companyConfiguration.IsPersonzliedPFX)
            {
                if (user.Id == Guid.Empty)
                {
                    throw new Exception($"Invalid user id [{user.Id}]");
                }

                const int maxAttempts = 3;
                for (int attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    if (_filesWrapper.Users.IsCertificateExist(user))
                    {
                        return;
                    }

                    try
                    {
                        string dn = CreateDN(user.Name, user.Email, "");
                        var cert = CreateCertificate(dn);
                        _filesWrapper.Users.SaveCertificate(user, cert);

                        if (_filesWrapper.Users.IsCertificateExist(user))
                        {
                            _logger.Information($"Successfully create certificate for user [{user.Id}]");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, $"Attempt {attempt} to create certificate failed for user [{user.Id}]");
                    }
                }
                throw new Exception($"Failed to create certificate for user [{user.Id}] after {maxAttempts} attempts.");
            }
        }

        public void Delete(User user)
        {
            if (_filesWrapper.Users.IsCertificateExist(user))
            {
                _filesWrapper.Users.DeleteCertificate(user);
            }
        }

        public X509Certificate2 Get(User user, CompanyConfiguration companyConfiguration)
        {

            if (!companyConfiguration.IsPersonzliedPFX && ShouldUseGlobalCertificate())
            {
                return GetGlobalCert();
            }

            if (!_filesWrapper.Users.IsCertificateExist(user))
            {
                string dn = CreateDN(user.Name, user.Email, "");
                var cert = CreateCertificate(dn);
                _filesWrapper.Users.SaveCertificate(user, cert);
            }
            byte[] pfxBytes = _filesWrapper.Users.ReadCertificate(user);

            return new X509Certificate2(pfxBytes, PFX_PASSWORD, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
        }

        #endregion

        public byte[] CreateCertificate(string dn)
        {
            byte[] pfx;

            if (!string.IsNullOrWhiteSpace(_generalSettings.CA))
                pfx = Encoding.UTF8.GetBytes(CreateCertificate(dn, PFX_PASSWORD));
            else
                pfx = _certificateCreator.Create(dn, PFX_PASSWORD);

            return pfx;

        }

        #region Private Functions

        private bool ShouldUseGlobalCertificate()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string globalPfx = assembly.GetManifestResourceNames().FirstOrDefault(x => x.ToLower().EndsWith(".pfx"));

            return string.IsNullOrWhiteSpace(_generalSettings.CA) && !string.IsNullOrWhiteSpace(globalPfx);
        }

        private X509Certificate2 GetGlobalCert()
        {
            X509Certificate2 cer = _memoryCache.Get<X509Certificate2>(X509_MEM_KEY);

            if (cer != null)
            {
                return cer;
            }

            var assembly = Assembly.GetExecutingAssembly();
            string globalPfx = assembly.GetManifestResourceNames().FirstOrDefault(x => x.ToLower().EndsWith(".pfx"));
            if (string.IsNullOrWhiteSpace(globalPfx))
            {
                throw new Exception("Cannot find global PFX for WeSign application");
            }

            using var stream = assembly.GetManifestResourceStream(globalPfx);
            byte[] certBytes = new byte[stream.Length];
            stream.Read(certBytes, 0, certBytes.Length);

            cer = X509CertificateLoader.LoadPkcs12(certBytes, PFX_PASSWORD,
               X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet, Pkcs12LoaderLimits.DangerousNoLimits);

            _memoryCache.Set(X509_MEM_KEY, cer, TimeSpan.FromMinutes(15));

            return cer;


        }


        private string CreateDN(string name, string email = "", string phone = "")
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(name))
            {
                sb.Append("CN=").Append(name);
            }
            if (!string.IsNullOrWhiteSpace(email))
            {
                if (sb.Length == 0) sb.Append("E=");
                else sb.Append(" ,E=");

                sb.Append(email);
            }
            if (!string.IsNullOrWhiteSpace(phone))
            {
                if (sb.Length == 0) sb.Append("2.5.4.20=");
                else sb.Append(" ,2.5.4.20=");

                sb.Append(phone);
            }
            return sb.ToString();
        }

        private string CreateCertificate(string dn, string password)
        {
            CX509CertificateRequestPkcs10 objPkcs10 = new CX509CertificateRequestPkcs10();
            CX509PrivateKey objPrivateKey = new CX509PrivateKey();
            CCspInformation objCSP = new CCspInformation();
            CCspInformations objCSPs = new CCspInformations();
            CX500DistinguishedName objDN = new CX500DistinguishedName();
            CX509Enrollment objEnroll = new CX509Enrollment();
            CX509ExtensionKeyUsage objExtensionKeyUsage = new CX509ExtensionKeyUsage();
            try
            {
                objCSP.InitializeFromName(_generalSettings.CryptographicServiceEProvider);
                objCSPs.Add(objCSP);
                objPrivateKey.Length = 2048;
                objPrivateKey.KeySpec = X509KeySpec.XCN_AT_SIGNATURE;
                objPrivateKey.KeyUsage = X509PrivateKeyUsageFlags.XCN_NCRYPT_ALLOW_SIGNING_FLAG;
                objPrivateKey.MachineContext = false;
                objPrivateKey.ExportPolicy = X509PrivateKeyExportFlags.XCN_NCRYPT_ALLOW_EXPORT_FLAG;
                objPrivateKey.CspInformations = objCSPs;
                objPrivateKey.Create();

                objPkcs10.InitializeFromPrivateKey(X509CertificateEnrollmentContext.ContextUser, objPrivateKey, "");
                objExtensionKeyUsage.InitializeEncode(CERTENROLLLib.X509KeyUsageFlags.XCN_CERT_NON_REPUDIATION_KEY_USAGE);
                objPkcs10.X509Extensions.Add((CX509Extension)objExtensionKeyUsage);

                objDN.Encode(dn, X500NameFlags.XCN_CERT_NAME_STR_NONE);

                objPkcs10.Subject = objDN;

                objEnroll.InitializeFromRequest(objPkcs10);

                string strRequest = objEnroll.CreateRequest(EncodingType.XCN_CRYPT_STRING_BASE64);

                var request = new CCertRequest();

                var cr = request.Submit((int)EncodingType.XCN_CRYPT_STRING_BASE64, strRequest, "", _generalSettings.CA);
                if (cr == CR_DISP_ISSUED)
                {
                    string certBase64 = request.GetCertificate((int)EncodingType.XCN_CRYPT_STRING_BASE64);
                    objEnroll.InstallResponse(InstallResponseRestrictionFlags.AllowUntrustedRoot, certBase64, EncodingType.XCN_CRYPT_STRING_BASE64, null);
                    string pfx = objEnroll.CreatePFX(password.ToString(), PFXExportOptions.PFXExportChainWithRoot, EncodingType.XCN_CRYPT_STRING_BASE64);
                    DeleteCert(certBase64);

                    return pfx;
                }
                else
                {
                    throw new Exception(request.GetDispositionMessage());
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error while CreateCertificate,{NewLine}{Exception}", Environment.NewLine, ex);
                _logger.Error(ex, "{NewLine} CertError", Environment.NewLine);
                throw;
            }
        }



        private void DeleteCert(string base64Cert)
        {
            if (base64Cert == string.Empty) return;
            var bytes = Convert.FromBase64String(base64Cert);
            var cert = new X509Certificate2(bytes, PFX_PASSWORD);
            using (var store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.MaxAllowed);
                var certificateCollection = store.Certificates.Find(X509FindType.FindByThumbprint, cert.Thumbprint, false);
                if (certificateCollection.Count > 0)
                {
                    store.Remove(certificateCollection[0]);

                }
            }
        }
        #endregion
    }
}
