using System.Collections;
using System.Collections.Generic;

namespace WeSignManagement.Models.Companies.Responses
{
    public class AllCompaniesReportsDTO
    {
        public IEnumerable<CompanyReportDTO> companyReports { get; set; } 
    }
}