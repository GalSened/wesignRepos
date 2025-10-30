using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common.Models.Configurations;
using Common.Models.Emails;
using Common.Models.Sms;

namespace Common.Interfaces.ManagementApp
{
    public interface IAppConfigurations
    {
        Task<Configuration> Read();
        Task Update(Configuration appConfiguration);
        void SendSmsTestMessage(SmsConfiguration smsConfiguration, Sms smsInfo);
        Task SendSmtpTestMessage(SmtpConfiguration smtpConfiguration, Email email);
    }
}
