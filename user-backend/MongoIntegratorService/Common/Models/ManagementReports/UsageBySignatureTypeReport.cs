namespace HistoryIntegratorService.Common.Models.ManagementReports
{
    public class UsageBySignatureTypeReport
    {
        public string CompanyName { get; set; }
        public int GraphicFieldsCount { get; set; }
        public int SmartCardFieldsCount { get; set; }
        public int ServerFieldsCount { get; set; }
    }
}
