namespace Common.Models.License
{
    public class LmsLicenseParams
    {
        public string Templates { get; set; }
        public string Users { get; set; }
        public string DocumentsPerMonth { get; set; }
        public string SmsPerMonth { get; set; }
        public string VisualIdentificationsPerMonth { get; set; }

        public string ShouldShowSelfSign { get; set; }
        public string ShouldShowGroupSign { get; set; }
        public string ShouldShowLiveMode { get; set; }
        public string ShouldShowContacts { get; set; }
        public string UseActiveDirectory { get; set; }
    }
}
