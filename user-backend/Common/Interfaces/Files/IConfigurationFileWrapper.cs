using Common.Enums;
using Common.Enums.Users;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Emails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.Files
{
    public interface IConfigurationFileWrapper
    {
        string GetCompanyLogo(Guid companyId);
        string GetEmailTemplate(User user, MessageType messageType);
        string GetDefaultEmailTemplate();
        string GetDefaultLogo();
        string GetCompanyEmailTemplate(Company company, MessageType messageType);
        string GetLogo(User user);
        Resource ReadEmailsResource(Language language);
        void SaveCompanyLogo(User user, string base64Logo);
        void UpdateCompanyEmailHtml(User user, EmailHtmlBodyTemplates emailHtmlBodyTemplates);
        void DeleteCompanyLogo(User user);
        void DeleteCompanyEmailHtml(User user);
        void DeleteCompanyResorces(Company company);
    }
}
