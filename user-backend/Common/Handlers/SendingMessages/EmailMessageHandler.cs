namespace Common.Handlers.SendingMessages
{
    using Common.Consts;
    using Common.Enums;
    using Common.Enums.Users;
    using Common.Interfaces.DB;
    using Common.Interfaces.Emails;
    using Common.Interfaces.Files;
    using Common.Models;
    using Common.Models.Emails;
    using Common.Models.Settings;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.IO.Abstractions;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    public class EmailMessageHandler : IEmail
    {


        private readonly GeneralSettings _generalSettings;
        private const string IMAGE_MIME_TYPE = "data:image/png;base64,";
        private readonly IEmailProvider _emailProvider;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IFilesWrapper _filesWrapper;

        public EmailMessageHandler(IOptions<GeneralSettings> generalSettings,
            IEmailProvider emailProvider, IServiceScopeFactory scopeFactory, IFilesWrapper filesWrapper)
        {
            
            _emailProvider = emailProvider;
            _generalSettings = generalSettings.Value;
            _scopeFactory = scopeFactory;
            _filesWrapper = filesWrapper;
        }

        public async Task<string> Activation(User user, bool sendEmail = true)
        {
            string link = $"{_generalSettings.UserFronendApplicationRoute}/login/activate/{user.Id}";
            if (sendEmail)
            {
                var email = new Email() { };
                email.To = user.Email;
                email.HtmlBody.ClientName = user.Name;

                email.HtmlBody.Link = link;
                Resource resource = InitEmail(email, user);
                email.Subject = resource.ActivationSubject;
                email.HtmlBody.LinkText = resource.ActivationLinkText;
                email.HtmlBody.EmailText = $"<div style='direction:{email.HtmlBody.TextDirection};'>{resource.ActivationText}</div>";
                email.DisplayName = user.Name;
                
            
                var config = await GetConfiguration();
                await _emailProvider.Send(email, config?.SmtpConfiguration);

            }
            return link;
        }

        public async Task<string> ResetPassword(User user, string token)
        {

            var email = new Email() { };
            email.To = user.Email;
            email.HtmlBody.ClientName = user.Name;
            //TODO calculate link
            string link = $"{_generalSettings.UserFronendApplicationRoute}/login/resetpass/{token}";
            email.HtmlBody.Link = link;
            Resource resource = InitEmail(email, user);
            email.Subject = resource.ForgetPasswordSubject;
            email.HtmlBody.LinkText = resource.ForgetPasswordLinkText;
            email.HtmlBody.EmailText = $"{resource.ForgetPasswordText}<BR />";
            email.DisplayName = user.Name;

            var config = await  GetConfiguration();
            await _emailProvider.Send(email, config.SmtpConfiguration);

            return link;
        }

        public async Task AllParticipantesSignedNotification(User user)
        {
            var email = new Email() { };
            email.To = user.Email;
            email.HtmlBody.ClientName = user.Name;
            Resource resource = InitEmail(email, user);
            //TODO set correct contactsList and documentName
            string documentName = "";
            string contactsList = "";
            string notificationText = $"{resource.TheDocument} {documentName} {resource.CompletedByAllParticipants}";
            email.Subject = notificationText;
            RemoveButton(email);
            email.HtmlBody.TemplateText = email.HtmlBody.TemplateText.Replace("<table bgcolor=\"#e64b3c\"", "<table");
            email.HtmlBody.EmailText = $"<div style='direction:{email.HtmlBody.TextDirection.ToString()};'>{notificationText} {contactsList}</div>";
            email.DisplayName = user.Name;

            var config =  await GetConfiguration();
            await _emailProvider.Send(email, config.SmtpConfiguration);
        }

        public async Task SignerSignedNotification(User user)
        {
            var email = new Email() { };
            email.To = user.Email;
            email.HtmlBody.ClientName = user.Name;
            Resource resource = InitEmail(email, user);
            //TODO set correct means and documentName
            string documentName = "";
            string means = "";
            string notificationText = $"{means} {resource.HasSigned} {documentName}";
            email.Subject = notificationText;
            RemoveButton(email);
            email.HtmlBody.TemplateText = email.HtmlBody.TemplateText.Replace("<table bgcolor=\"#e64b3c\"", "<table");
            email.HtmlBody.EmailText = $"<div style='direction:{email.HtmlBody.TextDirection.ToString()};'>{notificationText}</div>";
            email.DisplayName = user.Name;

            Models.Configurations.Configuration config = await GetConfiguration(); 
            await _emailProvider.Send(email, config.SmtpConfiguration);
        }

        private  Task<Models.Configurations.Configuration> GetConfiguration()
        {
            using var scope = _scopeFactory.CreateScope();
            IConfigurationConnector configurationConnector = scope.ServiceProvider.GetService<IConfigurationConnector>();
            return configurationConnector.Read();

        }

        #region Helper Functions

        /// <summary>
        /// Load Email body and logo.
        /// In addition, Get constant strings from language json files and init common htmlBody parameters.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        private Resource InitEmail(Email email, User user)
        {

            //TODO support html and logo per company if exists

            Resource resource = _filesWrapper.Configurations.ReadEmailsResource(user.UserConfiguration.Language);
           
      
            

            email.HtmlBody.TemplateText =  _filesWrapper.Configurations.GetDefaultEmailTemplate() ;
            email.HtmlBody.Logo = $"{IMAGE_MIME_TYPE}{_filesWrapper.Configurations.GetDefaultLogo()}";
            email.HtmlBody.Date = GetDateByLanguage(user.UserConfiguration.Language);
            var textDirection = user.UserConfiguration.Language == Language.en ? TextDirection.LTR : TextDirection.RTL;
            email.HtmlBody.TextDirection = textDirection;
            email.HtmlBody.ClientName = $"{resource.Hello}<br>{email.HtmlBody.ClientName}";
            email.HtmlBody.CopyrightText = resource.Copyright;
            email.HtmlBody.AttachmentDoNotReply = resource.AttachmentDoNotReply;
            email.HtmlBody.DigitalText = resource.Digital;
            email.HtmlBody.VisitText = resource.Visit;

            email.DisplayName = user.Name;

            return resource;
        }

        private string GetDateByLanguage(Language userLanguage)
        {
            if (userLanguage == Language.he)
            {
                var provider = CultureInfo.CreateSpecificCulture("he-IL");
                return DateTime.Now.ToString(Consts.DATE_FORMAT, provider);
            }
            return DateTime.Now.ToString(Consts.DATE_FORMAT);
        }

        private static void RemoveButton(Email email)
        {
            int startIndexHyperlink = email.HtmlBody.TemplateText.IndexOf("<a");
            int endIndexHyperlink = email.HtmlBody.TemplateText.IndexOf("a>");
            string temp = email.HtmlBody.TemplateText.Remove(startIndexHyperlink, endIndexHyperlink + 2 - startIndexHyperlink);

            int tempIndex = temp.IndexOf("class=\"cta-button\" bgcolor=\"");
            temp = temp.Remove(tempIndex + 28, 7);

            email.HtmlBody.TemplateText = temp;
        }

        #endregion
    }
}
