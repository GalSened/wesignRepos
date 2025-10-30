using System.Collections.Generic;

namespace WeSignManagement.Models.Companies.Responses
{
    public class AllTemplatesByUsageReportDTO
    {
        public IEnumerable<TemplatesByUsageReportDTO> TemplatesByUsageReports { get; set; }

    }
}
