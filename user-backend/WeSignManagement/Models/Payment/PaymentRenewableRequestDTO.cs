using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSignManagement.Models.Payment
{
    public class PaymentRenewableRequestDTO
    {
           
        public string Email { get; set; }
        public string TransactionId { get; set; }
        public string OldTransactionId { get; set; }
    }
}
