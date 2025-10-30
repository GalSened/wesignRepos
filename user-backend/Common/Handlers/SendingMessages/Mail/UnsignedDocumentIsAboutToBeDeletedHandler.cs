using Common.Enums;
using Common.Interfaces.Emails;
using Common.Interfaces.MessageSending.Mail;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents.Signers;
using Common.Models.Emails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Handlers.SendingMessages.Mail
{
    public class UnsignedDocumentIsAboutToBeDeletedHandler : IEmailType
    {
        private readonly IEmailProvider _emailProvider;
        private readonly IShared _shared;

        public UnsignedDocumentIsAboutToBeDeletedHandler(IEmailProvider emailProvider, IShared shared)
        {
            _emailProvider = emailProvider;
            _shared = shared;
        }

        public async Task SendAsync(SmtpConfiguration config, CompanyConfiguration companyConfiguration, MessageInfo messageInfo)
        {
            var email = new Email() { };
            email.To = messageInfo?.User?.Email;
            
            Resource resource = await _shared.InitEmail(email, messageInfo?.User, MessageType.UnsignedDocumentIsAboutToBeDeleted);
            email.Subject = $"{resource.DocumentAboutToBeDeleted}";
            email.HtmlBody.EmailText = $"<div style='direction:{email.HtmlBody.TextDirection};'>{messageInfo?.MessageContent}</div>";
            email.HtmlBody.Link = messageInfo.Link;
            email.HtmlBody.ClientName = $"<h1 style=\"color:red\">{resource.DeletionNotice}</h1>";
            email.HtmlBody.LinkText = resource.LoginToTheSystem; 
            
            await _emailProvider.Send(email, config);
        }
    }
   
}
