using Common.Interfaces.Emails;
using Common.Interfaces.MessageSending.Mail;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Emails;
using System.Threading.Tasks;

namespace Common.Handlers.SendingMessages.Mail
{
    public class ProgramIsAboutToExipredHandler : IEmailType
    {
        private readonly IEmailProvider _emailProvider;
        private readonly IShared _shared;

        public ProgramIsAboutToExipredHandler(IEmailProvider emailProvider, IShared shared)
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
            string message = $"{resource.ProgramIsAboutExpiredIn} {messageInfo?.MessageContent} {resource.Days}";
            email.Subject = message;
            _shared.RemoveButton(email);
            email.HtmlBody.TemplateText = email.HtmlBody.TemplateText.Replace("<table bgcolor=\"#e64b3c\"", "<table");
            email.HtmlBody.EmailText = $"<div style='direction:{email.HtmlBody.TextDirection.ToString()};'>{message}</div>";

            await _emailProvider.Send(email, config);
        }
    }
}
