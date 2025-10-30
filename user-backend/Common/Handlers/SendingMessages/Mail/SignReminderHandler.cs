using Common.Enums;
using Common.Interfaces.Emails;
using Common.Interfaces.MessageSending.Mail;
using Common.Interfaces;
using Common.Models.Configurations;
using Common.Models.Emails;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Handlers.SendingMessages.Mail
{
    public class SignReminderHandler : IEmailType
    {
        private readonly IEmailProvider _emailProvider;
        private readonly IShared _shared;
        private readonly IAppendices _appendices;

        public SignReminderHandler(IEmailProvider emailProvider, IShared shared, IAppendices appendices)
        {
            _emailProvider = emailProvider;
            _shared = shared;
            _appendices = appendices;
        }

        public async Task SendAsync(SmtpConfiguration config, CompanyConfiguration companyConfiguration, MessageInfo messageInfo)
        {
            var email = new Email() { };
            email.To = messageInfo?.Contact?.Email;
            email.HtmlBody.ClientName = messageInfo?.Contact?.Name;

            Resource resource = await _shared.InitEmail(email, messageInfo?.User, MessageType.SignReminder);
            email.Subject = $"{resource.SignReminder} {messageInfo?.DocumentCollection?.Name}";
            email.HtmlBody.Link = $"{messageInfo?.Link}";
            email.HtmlBody.LinkText = resource.SigningLinkText;
            email.HtmlBody.EmailText = $"<div style='direction:{email.HtmlBody.TextDirection};'>{messageInfo?.MessageContent}</div>";
            email.DocumentCollection = messageInfo?.DocumentCollection;

            //AddAppendices(messageInfo, email);
            var attachements = GetAllAppendices(messageInfo, email);
            email.Attachments = attachements.Select(a => a).ToList();

            await _emailProvider.Send(email, config);
        }

        private List<EmailAttachement> GetAllAppendices(MessageInfo messageInfo, Email email)
        {
            var appendices = new List<EmailAttachement>();

            var documentAttachments = _appendices.ReadForMail(messageInfo.DocumentCollection.Id);
            foreach (var documentAttachment in documentAttachments)
            {
                var emailAttachment = new EmailAttachement(
                    documentAttachment.Name,
                    documentAttachment.ContentStream,
                    Guid.Parse(documentAttachment.ContentId));
                appendices.Add(emailAttachment);
            }

            var signer = messageInfo.DocumentCollection.Signers.FirstOrDefault(x => x.Contact.Id == messageInfo.Contact.Id);
            var signerAttachments = _appendices.ReadForMail(messageInfo.DocumentCollection.Id, signer.Id);

            foreach (var signerAppendices in signerAttachments)
            {
                var signerAttachment = new EmailAttachement(
                    signerAppendices.Name,
                    signerAppendices.ContentStream,
                    Guid.Parse(signerAppendices.ContentId));
                appendices.Add(signerAttachment);
            }

            return appendices;
        }
    }
}
    
