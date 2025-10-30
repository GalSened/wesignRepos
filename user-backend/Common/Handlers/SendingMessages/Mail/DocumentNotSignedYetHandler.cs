using Common.Enums;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.Emails;
using Common.Interfaces.MessageSending.Mail;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Emails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Handlers.SendingMessages.Mail
{
    public class DocumentNotYetSignedHandler : IEmailType
    {
       private readonly IEmailProvider _emailProvider;
       private readonly IShared _shared;
        private readonly IAppendices _appendices;

        public DocumentNotYetSignedHandler(IEmailProvider emailProvider, IShared shared, IAppendices appendices)
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

            Resource resource = await _shared.InitEmail(email, messageInfo?.User, MessageType.BeforeSigning);
            email.Subject = $"{messageInfo?.User.Name} {resource.SendYouDocument} {messageInfo?.DocumentCollection?.Name}";
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

        private void AddAppendices(MessageInfo messageInfo, Email email)
        {
            var documentAttachments = _appendices.ReadForMail(messageInfo.DocumentCollection.Id);
            documentAttachments.ForEach(x => email.Attachments.Add(new EmailAttachement(x.Name, x.ContentStream, Guid.Parse(x.ContentId))));
            //documentAttachments.ForEach(x => email.Attachments.Add(x));

            var signer = messageInfo.DocumentCollection.Signers.FirstOrDefault(x => x.Contact.Id == messageInfo.Contact.Id);
            var signerAttachments = _appendices.ReadForMail(messageInfo.DocumentCollection.Id, signer.Id);
            documentAttachments.ForEach(x => email.Attachments.Add(new EmailAttachement(x.Name, x.ContentStream, Guid.Parse(x.ContentId))));
            //signerAttachments.ForEach(x => email.Attachments.Add(x));
        }

    }
}

