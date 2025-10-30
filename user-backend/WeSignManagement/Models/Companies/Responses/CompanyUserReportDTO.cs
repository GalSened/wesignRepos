using Common.Models.ManagementApp.Reports;

namespace WeSignManagement.Models.Companies.Responses
{
    public class CompanyUserReportDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public int DocumentAmounts { get; set; }
        public string GroupName { get; set; }

        public CompanyUserReportDTO(CompanyUserReport companyUserReport)
        {
            this.UserName = companyUserReport.UserName;
            this.Email = companyUserReport.Email;
            this.DocumentAmounts = companyUserReport.DocumentAmounts;
            this.GroupName = companyUserReport.GroupName;
                
        }
    }
}
