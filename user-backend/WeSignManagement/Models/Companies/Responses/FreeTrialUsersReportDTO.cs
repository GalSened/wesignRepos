using Common.Models.ManagementApp.Reports;
using System;
using System.Collections.Generic;

namespace WeSignManagement.Models.Companies.Responses
{
    public class FreeTrialUsersReportDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public int DocumentsUsage { get; set; }
        public int SMSUsage { get; set; }
        public int TemplatesUsage { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        

        public FreeTrialUsersReportDTO(FreeTrialUserReport freeTrialUserReport)
        {
            Name = freeTrialUserReport.Name;
            Email = freeTrialUserReport.Email;
            UserName = freeTrialUserReport.UserName;
            DocumentsUsage = freeTrialUserReport.DocumentsUsage;
            SMSUsage = freeTrialUserReport.SMSUsage;
            TemplatesUsage = freeTrialUserReport.TemplatesUsage;
            CreationDate = freeTrialUserReport.CreationDate;
            ExpirationDate = freeTrialUserReport.ExpirationDate;
        }
    }

    
}
