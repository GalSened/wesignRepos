using Common.Interfaces.Emails;
using Common.Interfaces.MessageSending.Mail;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Emails;
using System;
using System.Threading.Tasks;

namespace Common.Handlers.SendingMessages.Mail
{
    public class OtpCodeHandler : IEmailType
    {
        private readonly IEmailProvider _emailProvider;

        private readonly IShared _shared;

        public OtpCodeHandler(IEmailProvider emailProvider, IShared shared)
        {
            _emailProvider = emailProvider;
            _shared = shared;
        }


        public async Task SendAsync(SmtpConfiguration config, CompanyConfiguration companyConfiguration, MessageInfo messageInfo)
        {
            var email = new Email();
            if (messageInfo.Contact == null)
            {
                email.To = messageInfo?.User.Email;
                email.HtmlBody.ClientName = messageInfo?.User.Name;
            }
            else
            {
                email.To = messageInfo?.Contact.Email;
                email.HtmlBody.ClientName = messageInfo?.Contact.Name;
            }

            Resource resource = await _shared.InitEmail(email, messageInfo?.User, Enums.MessageType.OtpCode);
            email.Subject = messageInfo?.MessageContent;
            _shared.RemoveButton(email);
            email.HtmlBody.TemplateText = email.HtmlBody.TemplateText.Replace("<table bgcolor=\"#e64b3c\"", "<table");
            email.HtmlBody.EmailText = $"<div style='direction:{email.HtmlBody.TextDirection};'>{email.Subject}</div>";

            await _emailProvider.Send(email, config);
        }
    }
}
