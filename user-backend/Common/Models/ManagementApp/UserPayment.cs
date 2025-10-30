using Common.Enums.Program;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.ManagementApp
{
    public class UserPayment
    {
        public Guid ProgramID { get; set; }
        public string UserEmail { get; set; }
        public ProgramResetType ProgramResetType { get; set; } = ProgramResetType.DocumentsLimitOnly;
        public int MonthToAdd { get; set; }
        public string TransactionId { get; set; }
    }
}
