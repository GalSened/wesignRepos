using Common.Enums;
using Common.Interfaces;
using Common.Interfaces.Emails;
using Common.Interfaces.MessageSending.Mail;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Emails;
using System.Threading.Tasks;

namespace Common.Handlers.SendingMessages.Mail
{
    public class AfterSigningHandler : IEmailType
    {
        private readonly IEmailProvider _emailProvider;
        private readonly IShared _shared;
        private readonly IConfiguration _configuration;

        public AfterSigningHandler(IEmailProvider emailProvider, IShared shared, IConfiguration configuration)
        {
            _emailProvider = emailProvider;
            _shared = shared;
            _configuration = configuration;
        }

        public async Task SendAsync(SmtpConfiguration config, CompanyConfiguration companyConfiguration, MessageInfo messageInfo)
        {
            var email = new Email() { };
            email.To = messageInfo?.Contact?.Email;
            email.HtmlBody.ClientName = messageInfo?.Contact?.Name;

            Resource resource = await _shared.InitEmail(email, messageInfo?.User, MessageType.AfterSigning);
            _shared.LoadEmailAttachments(messageInfo?.DocumentCollection, email, shouldSendSignedDocument: true);
            ValidateAttachmentsSize(email, resource, config.AttachmentMaxSize);
            
            email.Subject = $"{resource.SubjectThankYou} {messageInfo?.DocumentCollection.Name}";
            email.HtmlBody.Link = $"{messageInfo?.Link}";
            email.HtmlBody.LinkText = resource.SignedDocumentText;
            email.HtmlBody.EmailText = $"<div style='direction:{email.HtmlBody.TextDirection.ToString()};'>{messageInfo?.MessageContent}</div>";

            if (email.DocumentCollection == null)
            {
                email.DocumentCollection = messageInfo.DocumentCollection;
            }

            await _emailProvider.Send(email, config);
        }
        
        private void ValidateAttachmentsSize(Email email, Resource resource, int attachmentMaxSize)
        {
            foreach (var attachment in email.Attachments)
            {
                //TODO validate size of stram attachment equal to size of attachmentMaxSize which is in byte size
                if (attachmentMaxSize > 0 && attachment.ContentStream.Length > attachmentMaxSize)
                {
                    email.HtmlBody.AttentionText = resource.AttentionText;
                    email.HtmlBody.AttachmentText = resource.AttachmentText;
                    email.HtmlBody.AttachmentSpace = resource.AttachmentSpace;

                    return;
                }
            }
        }


    }
}
