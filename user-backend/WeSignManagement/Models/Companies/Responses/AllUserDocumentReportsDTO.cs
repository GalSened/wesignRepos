using System.Collections;
using System.Collections.Generic;

namespace WeSignManagement.Models.Companies.Responses
{
    public class AllUserDocumentReportsDTO
    {
        public IEnumerable<UserDocumentReportDTO>  userDocumentReports { get; set; }
    }
}
