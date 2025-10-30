using Common.Enums;
using Common.Interfaces.Emails;
using Common.Interfaces.MessageSending.Mail;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Emails;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Handlers.SendingMessages.Mail
{
    public class SignerNoteNotificationHandler : IEmailType
    {
        private readonly IEmailProvider _emailProvider;
        private readonly IShared _shared;

        public SignerNoteNotificationHandler(IEmailProvider emailProvider, IShared shared)
        {
            _emailProvider = emailProvider;
            _shared = shared;
        }

        public async Task SendAsync(SmtpConfiguration config, CompanyConfiguration companyConfiguration, MessageInfo messageInfo)
        {
            if (messageInfo == null || !(messageInfo is SignerNoteMessageInfo))
            {
                return;
            }
            var signerNoteMessageInfo = messageInfo as SignerNoteMessageInfo;
            var email = new Email() { };
            email.To = messageInfo?.User.Email;
            Resource resource = await _shared.InitEmail(email, messageInfo?.User, MessageType.SignerNoteNotification);
            email.Subject = resource.SignerNoteNotificationSubject.Replace("{SIGNER_NAME}", messageInfo.Contact.Name).Replace("{DOC_NAME}", messageInfo.DocumentCollection.Name);
            _shared.RemoveButton(email);
            email.HtmlBody.TemplateText = email.HtmlBody.TemplateText.Replace("<table bgcolor=\"#e64b3c\"", "<table");
            var emailContext = resource.SignerNoteNotificationContext.Replace("{SENDER_NAME}", signerNoteMessageInfo.Contact.Name)
                .Replace("{SIGNER_NAME}", messageInfo.Contact.Name)
                .Replace("{DOC_NAME}", messageInfo.DocumentCollection.Name);
            email.HtmlBody.EmailText = $"<div style='direction:{email.HtmlBody.TextDirection.ToString()};'>{emailContext}<BR /><BR />{signerNoteMessageInfo.Notes}</div>";
            await _emailProvider.Send(email, config);
        }
    }
}
