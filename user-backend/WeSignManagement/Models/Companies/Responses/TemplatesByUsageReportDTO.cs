using Common.Models.ManagementApp.Reports;
using System;

namespace WeSignManagement.Models.Companies.Responses
{
    public class TemplatesByUsageReportDTO
    {
        public string TemplateName { get; set; }
        public string CompanyName { get; set; }
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public int UsageCount { get; set; }

        public TemplatesByUsageReportDTO(TemplatesByUsageReport templatesByUsageReport)
        {
            TemplateName = templatesByUsageReport.TemplateName; 
            CompanyName = templatesByUsageReport.CompanyName;
            GroupId = templatesByUsageReport.GroupId;
            GroupName = templatesByUsageReport.GroupName;
            UsageCount = templatesByUsageReport.UsageCount;
        }
    }
}
