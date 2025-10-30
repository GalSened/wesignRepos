using Common.Enums;
using Common.Extensions;
using Common.Interfaces.Emails;
using Common.Interfaces.MessageSending.Mail;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents.Signers;
using Common.Models.Emails;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Common.Handlers.SendingMessages.Mail
{
    public class SignerViewDocumentNotificationHandler : IEmailType
    {
        private readonly IEmailProvider _emailProvider;
        private readonly IShared _shared;

        public SignerViewDocumentNotificationHandler(IEmailProvider emailProvider, IShared shared)
        {
            _emailProvider = emailProvider;
            _shared = shared;
        }

        public async Task SendAsync(SmtpConfiguration config, CompanyConfiguration companyConfiguration, MessageInfo messageInfo)
        {
            var email = new Email() { };
            if (email.DocumentCollection == null)
            {
                email.DocumentCollection = messageInfo.DocumentCollection;
            }
            email.To = messageInfo?.User.Email;
            email.HtmlBody.ClientName = messageInfo?.User.Name;
            Resource resource = await _shared.InitEmail(email, messageInfo?.User, MessageType.SignerViewDocumentNotification);
            string notificationText = $"{_shared.GetContactNameFormat(messageInfo?.Contact)} {resource.HasViewed} {messageInfo?.DocumentCollection?.Name}";
            email.Subject = notificationText;
            _shared.RemoveButton(email);
            email.HtmlBody.TemplateText = email.HtmlBody.TemplateText.Replace("<table bgcolor=\"#e64b3c\"", "<table");
            email.HtmlBody.EmailText = $"<div style='direction:{email.HtmlBody.TextDirection.ToString()};'>{notificationText}</div>";

            await _emailProvider.Send(email, config);
        }
    }
}
