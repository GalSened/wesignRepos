using System.Collections.Generic;

namespace WeSignManagement.Models.Companies.Responses
{
    public class AllCompanyUsersReportDTO
    {
        public IEnumerable<CompanyUserReportDTO>companyUsersReports { get; set; }
    }
}
