using Common.Models.ManagementApp.Reports;

namespace WeSignManagement.Models.Companies.Responses
{
    public class UsageBySignatureTypeReportDTO
    {
        public string CompanyName { get; set; }
        public int GraphicFieldsCount { get; set; }
        public int SmartCardFieldsCount { get; set; }
        public int ServerFieldsCount { get; set; }

        public UsageBySignatureTypeReportDTO(UsageBySignatureTypeReport usageBySignatureTypeReport)
        {
            CompanyName = usageBySignatureTypeReport.CompanyName;
            GraphicFieldsCount = usageBySignatureTypeReport.GraphicFieldsCount;
            SmartCardFieldsCount = usageBySignatureTypeReport.SmartCardFieldsCount;
            ServerFieldsCount = usageBySignatureTypeReport.ServerFieldsCount;
        }
    }
}
