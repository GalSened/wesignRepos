using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.ManagementApp.Reports
{
    public class FreeTrialUserReport
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public int DocumentsUsage { get; set; }
        public int SMSUsage { get; set; }
        public int TemplatesUsage { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ExpirationDate { get; set; }

    }
}
