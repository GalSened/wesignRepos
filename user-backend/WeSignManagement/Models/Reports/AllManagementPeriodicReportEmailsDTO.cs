using Common.Models.ManagementApp;
using System.Collections.Generic;

namespace WeSignManagement.Models.Reports
{
    public class AllManagementPeriodicReportEmailsDTO
    {
        public IEnumerable<ManagementPeriodicReportEmail> periodicReportEmails { get; set; }
    }
}