using Common.Models;
using Common.Models.Configurations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Interfaces.MessageSending
{
    public interface IMessageSender
    {
        Task Send(Configuration appConfiguration, CompanyConfiguration companyConfiguration, MessageInfo messageInfo);
        Task SendBatch(Configuration appConfiguration, CompanyConfiguration companyConfiguration, List<MessageInfo> messageInfo);
    }
}