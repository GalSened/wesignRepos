using Common.Enums;
using Common.Interfaces.Emails;
using Common.Interfaces.MessageSending.Mail;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Emails;
using System.Threading.Tasks;

namespace Common.Handlers.SendingMessages.Mail
{
    internal class SharedDocumentNotificationHandler : IEmailType
    {
        private IEmailProvider _emailProvider;
        private IShared _shared;

        public SharedDocumentNotificationHandler(IEmailProvider emailProvider, IShared shared)
        {
            _emailProvider = emailProvider;
            _shared = shared;
        }

        public async Task SendAsync(SmtpConfiguration config, CompanyConfiguration companyConfiguration, MessageInfo messageInfo)
        {
            var email = new Email() { };           
            email.To = messageInfo?.Contact.Email;
            email.HtmlBody.ClientName = messageInfo?.Contact.Name;            
            Resource resource = await _shared.InitEmail(email, messageInfo?.User, MessageType.SharedDocumentNotification);
            _shared.LoadEmailAttachments(messageInfo?.DocumentCollection, email, true);
            email.HtmlBody.Link = $"{messageInfo?.Link}";
            email.HtmlBody.LinkText = resource.ReviewDocument;
            email.HtmlBody.EmailText = $"";
            string notificationText = $"{messageInfo?.User.Name} {resource.SharedADocumentWithYou} {messageInfo?.DocumentCollection?.Name}";
            email.Subject = notificationText;
           
            await _emailProvider.Send(email, config);
        }
    }
}
