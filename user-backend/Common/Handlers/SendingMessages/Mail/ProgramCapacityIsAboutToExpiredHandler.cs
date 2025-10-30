using Common.Enums;
using Common.Interfaces.Emails;
using Common.Interfaces.MessageSending.Mail;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Emails;
using System;
using System.Threading.Tasks;

namespace Common.Handlers.SendingMessages.Mail
{
    public class ProgramCapacityIsAboutToExpiredHandler : IEmailType
    {
        private readonly IEmailProvider _emailProvider;
        private readonly IShared _shared;
        public ProgramCapacityIsAboutToExpiredHandler(IEmailProvider emailProvider, IShared shared)
        {
            _emailProvider = emailProvider;
            _shared = shared;
        }
        public async Task SendAsync(SmtpConfiguration config, CompanyConfiguration companyConfiguration, MessageInfo messageInfo)
        {
            var email = new Email();
            email.To = messageInfo?.User.Email;
            email.HtmlBody.ClientName = messageInfo?.User.Name;
            Resource resource = await _shared.InitEmail(email, messageInfo?.User, Enums.MessageType.ProgramIsAboutToExipred);
            var parts = messageInfo?.MessageContent.Split(';');
            var programParm = (ProgramCapacityType)Enum.Parse(typeof(ProgramCapacityType), parts[0]) == ProgramCapacityType.SMS ? resource.ProgramSMS :
                (ProgramCapacityType)Enum.Parse(typeof(ProgramCapacityType), parts[0]) == ProgramCapacityType.Documents? resource.ProgramDocuments : resource.ProgramVisualIdentification;
            string message = $"{programParm} {resource.CapacityIsOverThan} {parts[1]} {resource.Percentages}";
            email.Subject = message;
            _shared.RemoveButton(email);
            email.HtmlBody.TemplateText = email.HtmlBody.TemplateText.Replace("<table bgcolor=\"#e64b3c\"", "<table");
            email.HtmlBody.EmailText = $"<div style='direction:{email.HtmlBody.TextDirection.ToString()};'>{message}</div>";

            await _emailProvider.Send(email, config);
        }
    }
}
