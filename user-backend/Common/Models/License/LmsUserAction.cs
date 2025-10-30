using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.License
{
    public class LmsUserAction
    {
        public Guid UserID { get; set; }
        public Guid CompanyID { get; set; }
        public string TransactionId { get; set; }
    }
}
