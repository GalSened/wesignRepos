using System;

namespace Common.Models.ManagementApp.Reports
{
    public class TemplatesByUsageReport
    {
        public string TemplateName { get; set; }
        public string CompanyName { get; set; }
        public Guid GroupId { get; set; }
        public string GroupName { get; set; }
        public int UsageCount { get; set; }
    }
}
