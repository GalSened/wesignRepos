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
    public class SingleSignerSignedNotificationHandler : IEmailType
    {
        private readonly IEmailProvider _emailProvider;
        private readonly IShared _shared;

        public SingleSignerSignedNotificationHandler(IEmailProvider emailProvider, IShared shared)
        {
            _emailProvider = emailProvider;
            _shared = shared;
        }

        public async Task SendAsync(SmtpConfiguration config, CompanyConfiguration companyConfiguration, MessageInfo messageInfo)
        {
            var email = new Email() { };
            email.To = messageInfo?.User.Email;
            email.HtmlBody.ClientName = messageInfo?.User.Name;
            Resource resource = await _shared.InitEmail(email, messageInfo?.User, Enums.MessageType.SingleSignerSignedNotification);
            string notificationText = $"{_shared.GetContactNameFormat(messageInfo?.Contact)} {resource.HasSigned} {messageInfo?.DocumentCollection?.Name}";
            email.Subject = notificationText;
            _shared.RemoveButton(email);
            email.HtmlBody.TemplateText = email.HtmlBody.TemplateText.Replace("<table bgcolor=\"#e64b3c\"", "<table");
            email.HtmlBody.EmailText = $"<div style='direction:{email.HtmlBody.TextDirection.ToString()};'>{notificationText}</div>";

            await _emailProvider.Send(email, config);
        }
    }
}
