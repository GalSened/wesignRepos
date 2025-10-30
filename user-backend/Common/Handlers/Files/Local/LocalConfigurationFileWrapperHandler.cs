
using Common.Enums;
using Common.Enums.Results;
using Common.Enums.Users;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.Files;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Emails;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.IO.Abstractions;
using System.Net;
using System.Reflection;
using System.Text;

namespace Common.Handlers.Files.Local
{
    public class LocalConfigurationFileWrapperHandler : IConfigurationFileWrapper
    {

        private const string DEFAULT_EMAIL_TEMPLATE = "Resources/EmailBody.html";
        private const string DEFAULT_LOGO = "Resources/Logo.png";


        private readonly IFileSystem _fileSystem;
        private readonly FolderSettings _folderSettings;
        private readonly IDataUriScheme _dataUriScheme;
        public LocalConfigurationFileWrapperHandler(IFileSystem fileSystem, IOptions<FolderSettings> folderSettings, IDataUriScheme dataUriScheme)
        {
            _fileSystem = fileSystem;
            _folderSettings = folderSettings.Value;
            _dataUriScheme = dataUriScheme;

        }

        public void DeleteCompanyEmailHtml(User user)
        {
            if (user == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidInput.GetNumericString());
            }
            string path = _fileSystem.Path.Combine(_folderSettings.EmailTemplates, $"{user.CompanyId}.html");
            if (_fileSystem.File.Exists(path))
            {
                _fileSystem.File.Delete(path);
            }
        }

        public void DeleteCompanyLogo(User user)
        {
            if (user == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidInput.GetNumericString());
            }
            string path = _fileSystem.Path.Combine(_folderSettings.CompaniesLogo, $"{user?.CompanyId}.png");
            if (_fileSystem.File.Exists(path))
            {
                _fileSystem.File.Delete(path);
            }
        }

        public void DeleteCompanyResorces(Company company)
        {
            if (company == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidInput.GetNumericString());
            }
            string companylogoPath = _fileSystem.Path.Combine(_folderSettings.CompaniesLogo, $"{company.Id}.png");
            if (_fileSystem.File.Exists(companylogoPath))
            {
                _fileSystem.File.Delete(companylogoPath);
            }

            string beforeSigningPath = _fileSystem.Path.Combine(_folderSettings.EmailTemplates, "BeforeSigning", $"{company.Id}.html");
            if (_fileSystem.File.Exists(beforeSigningPath))
            {
                _fileSystem.File.Delete(beforeSigningPath);
            }

            string afterSigningPath = _fileSystem.Path.Combine(_folderSettings.EmailTemplates, "AfterSigning", $"{company.Id}.html");
            if (_fileSystem.File.Exists(afterSigningPath))
            {
                _fileSystem.File.Delete(afterSigningPath);
            }
        }

        public string GetCompanyEmailTemplate(Company company, MessageType messageType)
        {
            if (company == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidInput.GetNumericString());
            }
            string base64string = "";
            string emailTemplatePath = GetCompanyEmailPath(company.Id, messageType);
            if (!string.IsNullOrWhiteSpace(emailTemplatePath))
            {
                if (_fileSystem.File.Exists(emailTemplatePath))
                {
                    var emailTemplateBytes = _fileSystem.File.ReadAllBytes(emailTemplatePath);
                    base64string = emailTemplateBytes.Length > 0 ? $"data:text/html;base64,{Convert.ToBase64String(emailTemplateBytes)}" : "";
                }
            }

            return base64string;
        }

        public string GetCompanyLogo(Guid companyId)
        {
            string base64image = "";
            string logoPath = GetCompanyLogoPath(companyId);
            if (!string.IsNullOrWhiteSpace(logoPath))
            {
                var logoBytes = _fileSystem.File.ReadAllBytes(logoPath);
                base64image = logoBytes.Length > 0 ? $"data:image/png;base64,{Convert.ToBase64String(logoBytes)}" : "";
            }

            return base64image;
        }

        public string GetDefaultEmailTemplate()
        {
            string currentFolder = _fileSystem.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string emailTemplate = _fileSystem.Path.Combine(currentFolder, DEFAULT_EMAIL_TEMPLATE);
            if (!_fileSystem.File.Exists(emailTemplate))
            {
                throw new Exception($"Email body template not exist in system [{emailTemplate}]");
            }
            return Encoding.UTF8.GetString(_fileSystem.File.ReadAllBytes(emailTemplate));
        }

        public string GetDefaultLogo()
        {
            string currentFolder = _fileSystem.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string logo = _fileSystem.Path.Combine(currentFolder, DEFAULT_LOGO);
            if (!_fileSystem.File.Exists(logo))
            {
                throw new Exception($"Default logo not exist in system  [{logo}]");
            }
            return Convert.ToBase64String(_fileSystem.File.ReadAllBytes(logo));

        }

        public string GetEmailTemplate(User user, MessageType messageType)
        {
            if (user == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidInput.GetNumericString());
            }

            string emailTemplatePath = GetCompanyEmailPath(user.CompanyId, messageType);
            string result;
            if (string.IsNullOrWhiteSpace(emailTemplatePath) || !_fileSystem.File.Exists(emailTemplatePath))
            {
                result = GetDefaultEmailTemplate();
            }
            else
            {
                result = Encoding.UTF8.GetString(_fileSystem.File.ReadAllBytes(emailTemplatePath));
            }
            return result;
        }



        public string GetLogo(User user)
        {
            if (user == null)
            {
                throw new Exception($"Null input - user is null");
            }

            string logoPath = GetCompanyLogoPath(user.CompanyId);
            string logoResult;
            if (string.IsNullOrWhiteSpace(logoPath))
            {
                logoResult = GetDefaultLogo();

            }
            else
            {
                logoResult = Convert.ToBase64String(_fileSystem.File.ReadAllBytes(logoPath));
            }
            return logoResult;

        }

        public Resource ReadEmailsResource(Language language)
        {
            string currentFolder = _fileSystem.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string jsonPath = _fileSystem.Path.Combine(currentFolder, $"Resources/Emails.{language}.json");
            if (!_fileSystem.File.Exists(jsonPath))
            {
                throw new InvalidOperationException($"Email Json not exist in system [{jsonPath}]");
            }
            string json = WebUtility.HtmlDecode(_fileSystem.File.ReadAllText(jsonPath, Encoding.UTF8));
            return JsonConvert.DeserializeObject<Resource>(json);
        }

        public void SaveCompanyLogo(User user, string base64Logo)
        {
            if (user == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidInput.GetNumericString());
            }

            string path = _fileSystem.Path.Combine(_folderSettings.CompaniesLogo, $"{user.CompanyId}.png");
            _fileSystem.Directory.CreateDirectory(_folderSettings.CompaniesLogo);
            if (!string.IsNullOrWhiteSpace(base64Logo))
            {
                _fileSystem.File.WriteAllBytes(path, _dataUriScheme.GetBytes(base64Logo));
            }
        }


        public void UpdateCompanyEmailHtml(User user, EmailHtmlBodyTemplates emailHtmlBodyTemplates)
        {
            if (user == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidInput.GetNumericString());
            }
            _fileSystem.Directory.CreateDirectory(_fileSystem.Path.Combine(_folderSettings.EmailTemplates, "BeforeSigning"));
            _fileSystem.Directory.CreateDirectory(_fileSystem.Path.Combine(_folderSettings.EmailTemplates, "AfterSigning"));

            string beforeSigningPath = _fileSystem.Path.Combine(_folderSettings.EmailTemplates, "BeforeSigning", $"{user.CompanyId}.html");
            if (!string.IsNullOrWhiteSpace(emailHtmlBodyTemplates?.BeforeSigningBase64String))
            {
                _fileSystem.File.WriteAllBytes(beforeSigningPath, _dataUriScheme.GetBytes(emailHtmlBodyTemplates.BeforeSigningBase64String));
            }
            else
            {
                _fileSystem.File.Delete(beforeSigningPath);
            }

            string afterSigningPath = _fileSystem.Path.Combine(_folderSettings.EmailTemplates, "AfterSigning", $"{user.CompanyId}.html");
            if (!string.IsNullOrWhiteSpace(emailHtmlBodyTemplates?.AfterSigningBase64String))
            {
                _fileSystem.File.WriteAllBytes(afterSigningPath, _dataUriScheme.GetBytes(emailHtmlBodyTemplates.AfterSigningBase64String));
            }
            else
            {
                _fileSystem.File.Delete(afterSigningPath);
            }
        }

        private string GetCompanyEmailPath(Guid companyId, MessageType messageType)
        {
            return (messageType == MessageType.BeforeSigning || messageType == MessageType.AfterSigning) ?
            _fileSystem.Path.Combine(_folderSettings.EmailTemplates, messageType.ToString(), $"{companyId}.html") : "";
        }

        private string GetCompanyLogoPath(Guid companyId)
        {
            string companyLogoPath = _fileSystem.Path.Combine(_folderSettings.CompaniesLogo, $"{companyId}.png");

            return _fileSystem.File.Exists(companyLogoPath) ? companyLogoPath : "";
        }
    }
}
