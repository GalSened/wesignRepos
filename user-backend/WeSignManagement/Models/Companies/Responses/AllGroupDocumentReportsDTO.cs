using System.Collections.Generic;

namespace WeSignManagement.Models.Companies.Responses
{
    public class AllGroupDocumentReportsDTO
    {
        public IEnumerable<GroupDocumentReportDTO> groupDocumentReports { get; set; }
    }
}
