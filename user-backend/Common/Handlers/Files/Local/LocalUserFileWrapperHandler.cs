using Common.Interfaces;
using Common.Interfaces.Files;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Settings;
using IO.ClickSend.ClickSend.Model;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Common.Handlers.Files.Local
{
    public class LocalUserFileWrapperHandler : IUserFileWrapper
    {
        private const string DEFAULT_LOGO = "Resources/Logo.png";

        private readonly IFileSystem _fileSystem;
        private readonly FolderSettings _folderSettings;
        

        public LocalUserFileWrapperHandler( IFileSystem fileSystem,
             IOptions<FolderSettings> folderSettings)
        {
            
            _fileSystem = fileSystem;
            _folderSettings = folderSettings.Value;
            
        }

        public void DeleteCertificate(User user)
        {

            var userCertificate = _fileSystem.Path.Combine(_folderSettings.UserCertificates, $"{user.Id}.pfx");
            if (_fileSystem.File.Exists(userCertificate))
            {                              
                _fileSystem.File.Delete(userCertificate);
            }
        }



        public void SaveCertificate(User contact, byte[] cert)
        {
            var certPath = _fileSystem.Path.Combine(_folderSettings.UserCertificates, $"{contact?.Id}.pfx");
            _fileSystem.File.WriteAllBytes(certPath, cert);
        }
        public byte[] ReadCertificate(User user)
        {
            string certificatePath = _fileSystem.Path.Combine(_folderSettings.UserCertificates, $"{user.Id}.pfx");          
            return _fileSystem.File.ReadAllBytes(certificatePath);
        }

        public void SetCompanyLogo(User user)
        {
            user.CompanyLogo = $"data:image/png;base64,{GetCompanyLogo(user)}";
            
        }

        public bool IsCertificateExist(User user)
        {
            var contactCertificate = _fileSystem.Path.Combine(_folderSettings.UserCertificates, $"{user?.Id}.pfx");
            return _fileSystem.File.Exists(contactCertificate);
        }


        private string GetCompanyLogo(User user)
        {
            if (user == null)
            {
                throw new Exception($"Null input - user is null");
            }
            string currentFolder = _fileSystem.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string companyLogoPath = GetCompanyLogoPath(user.CompanyId);
            if (string.IsNullOrWhiteSpace(companyLogoPath))
            {
                companyLogoPath = _fileSystem.Path.Combine(currentFolder, DEFAULT_LOGO);
            }
            return Convert.ToBase64String(_fileSystem.File.ReadAllBytes(companyLogoPath));
        }

        private string GetCompanyLogoPath(Guid companyId)
        {
            string companyLogoPath = _fileSystem.Path.Combine(_folderSettings.CompaniesLogo, $"{companyId}.png");

            return _fileSystem.File.Exists(companyLogoPath) ? companyLogoPath : "";
        }
    }
}
