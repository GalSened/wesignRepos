using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models.ManagementApp.Reports
{
    public class GroupUtilizationReport
    {
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public long PeriodicDocumentUsage { get; set; }
        public long PeriodicSMSUsage { get; set; }


       
    }
}
