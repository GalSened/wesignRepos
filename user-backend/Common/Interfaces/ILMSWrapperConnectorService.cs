using Common.Models.License;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface ILMSWrapperConnectorService
    {
        Task<bool> UnsubscribeUser(LmsUserAction unsubscribeUser);
        Task<bool> CheckUser(LmsUserAction unsubscribeUser);
        Task<string> GetURLForChangePaymentRule(LmsUserAction lmsUserAction);
    }
}
