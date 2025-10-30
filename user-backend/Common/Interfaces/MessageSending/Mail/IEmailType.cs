using Common.Models;
using Common.Models.Configurations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.MessageSending.Mail
{
    public interface IEmailType
    {
        Task SendAsync(SmtpConfiguration config, CompanyConfiguration companyConfiguration, MessageInfo messageInfo);
    }
}
