using Common.Models;
using Common.Models.ManagementApp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.ManagementApp
{
    public interface IPayment
    {
        Task UserPay(UserPayment userPayment);
        Task UpdateRenwablePayment(UpdatePaymentRenewable userPayment);
        Task UnsubscribeCompany(Company company);
        Task UpdateCompanyTransactionAndExpirationTime(Company company);
    }
}
