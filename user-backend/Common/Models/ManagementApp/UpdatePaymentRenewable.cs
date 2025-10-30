using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.ManagementApp
{
    public class UpdatePaymentRenewable
    {

        public string Email { get; set; }
        public string TransactionId { get; set; }
        public string OldTransactionId { get; set; }
    }
}
