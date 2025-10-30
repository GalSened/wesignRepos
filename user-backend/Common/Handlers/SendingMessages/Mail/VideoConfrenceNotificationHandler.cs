using Common.Enums;
using Common.Interfaces.Emails;
using Common.Interfaces.MessageSending.Mail;
using Common.Models;
using Common.Models.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Models.Emails;

namespace Common.Handlers.SendingMessages.Mail
{
    public class VideoConfrenceNotificationHandler : IEmailType
    {
        private IEmailProvider _emailProvider;
        private IShared _shared;

        public VideoConfrenceNotificationHandler(IEmailProvider emailProvider, IShared shared)
        {
            _emailProvider = emailProvider;
            _shared = shared;
        }

        public async  Task SendAsync(SmtpConfiguration config, CompanyConfiguration companyConfiguration, MessageInfo messageInfo)
        {
            
            var email = new Email() { };
            email.To = messageInfo?.Contact.Email;
            email.HtmlBody.ClientName = messageInfo?.Contact?.Name;
            Resource resource = await _shared.InitEmail(email, messageInfo?.User, MessageType.VideoConfrenceNotification);

            email.Subject = resource.SentYoutVideoConfrenceSubject.Replace("{SENDER_NAME}", messageInfo.User.Name);
            email.HtmlBody.Link = $"{messageInfo?.Link}";
            email.HtmlBody.LinkText = resource.SentYoutVideoConfrenceLinkButton;
            email.HtmlBody.EmailText = $"<div style='direction:{email.HtmlBody.TextDirection};'>{resource.SentYoutVideoConfrenceBody.Replace("{DOC_NAME}",messageInfo.MessageContent )}</div>";
            email.DocumentCollection = null;
            await _emailProvider.Send(email, config);
        }
    }
}
