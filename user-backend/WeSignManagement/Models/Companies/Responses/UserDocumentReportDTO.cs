using Common.Models.ManagementApp.Reports;

namespace WeSignManagement.Models.Companies.Responses
{
    public class UserDocumentReportDTO
    {
        public string ContactName { get; set; }
        public int DocumentAmount { get; set; }

        public UserDocumentReportDTO(UserDocumentsReport userDocReport)
        {
            ContactName = userDocReport.ContactName;
            DocumentAmount = userDocReport.DocumentAmount;
        }
    }
}
