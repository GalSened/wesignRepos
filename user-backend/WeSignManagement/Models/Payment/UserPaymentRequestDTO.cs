using Common.Enums.Program;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSignManagement.Models.Payment
{
    public class UserPaymentRequestDTO
    {
        public Guid ProgramID { get; set; }
        public string UserEmail { get; set; }
        public ProgramResetType ProgramResetType { get; set; } = ProgramResetType.DocumentsLimitOnly;
        public int MonthToAdd { get; set; }
        public string PaymentTransactionId { get; set; }        
    }
}
