using Common.Enums.Users;
using Common.Interfaces;
using Common.Interfaces.Emails;
using Common.Interfaces.MessageSending.Mail;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Emails;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.Handlers.SendingMessages.Mail
{
    public class DeclineHandler : IEmailType
    {
        private IEmailProvider _emailProvider;
        private IShared _shared;
        private IConfiguration _configuration;

        public DeclineHandler(IEmailProvider emailProvider, IShared shared, IConfiguration configuration)
        {
            _emailProvider = emailProvider;
            _shared = shared;
            _configuration = configuration;
        }

        public async Task SendAsync(SmtpConfiguration config, CompanyConfiguration companyConfiguration, MessageInfo messageInfo)
        {
           
            var email = new Email();
            email.To = messageInfo?.User?.Email;
            email.HtmlBody.ClientName = messageInfo?.User?.Name;
            Resource resource = await _shared.InitEmail(email, messageInfo?.User, Enums.MessageType.Decline);           
            (string subject, string message) = await GetDeclineMessageAndSubject(messageInfo);
            email.Subject = subject;
            _shared.RemoveButton(email);
            email.HtmlBody.TemplateText = email.HtmlBody.TemplateText.Replace("<table bgcolor=\"#e64b3c\"", "<table");
            email.HtmlBody.EmailText = $"<div style='direction:{email.HtmlBody.TextDirection};'>{message}</div>";

            await _emailProvider.Send(email, config);
        }

        private async Task< (string, string)> GetDeclineMessageAndSubject(MessageInfo messageInfo)
        {
            var language = await _configuration.GetLanguage(messageInfo?.User);
            string subject = language == Language.en ? $"The document {messageInfo.DocumentCollection.Name} was declined by {messageInfo.Contact.Name}" : $"המסמך {messageInfo.DocumentCollection.Name} נדחה על ידי {messageInfo.Contact.Name}";
            string message = language == Language.en ? $"The signer has declined the document with the message:<br>{messageInfo.MessageContent}" : $"החותם דחה את המסמך עם ההודעה הבאה :<br>{messageInfo.MessageContent}";

            return (subject, message);
        }

    }
}
