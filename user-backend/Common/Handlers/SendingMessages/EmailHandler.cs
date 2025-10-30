using Common.Interfaces;
using Common.Interfaces.MessageSending;
using Common.Models;
using Common.Models.Configurations;
using Common.Interfaces.MessageSending.Mail;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Handlers.SendingMessages
{
    public class EmailHandler : IMessageSender
    {     
        private readonly IConfiguration _configuration;
        private readonly IEmailTypeHandler _emailTypeHandler;


        public EmailHandler(IConfiguration configuration, IEmailTypeHandler emailTypeHandler)
        {
            _configuration = configuration;
            _emailTypeHandler = emailTypeHandler;
        }

        public  Task Send(Configuration appConfiguration, CompanyConfiguration companyConfiguration, MessageInfo messageInfo)
        {
            var smtpConfiguration = _configuration.GetSmtpConfiguration(messageInfo?.User, appConfiguration, companyConfiguration);
            var emailType = _emailTypeHandler.ExecuteCreation(messageInfo.MessageType);

           return emailType.SendAsync(smtpConfiguration, companyConfiguration, messageInfo);         
        }

        public async Task SendBatch(Configuration appConfiguration, CompanyConfiguration companyConfiguration, List<MessageInfo> messageInfo)
        {
            // for now send one by one

            foreach (var message in messageInfo)
            {
               await Send(appConfiguration, companyConfiguration, message);
            }
        }
    }
}
