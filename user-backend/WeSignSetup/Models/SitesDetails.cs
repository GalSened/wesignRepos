namespace WeSignSetup.Models
{
    public class SitesDetails
    {
        public bool ShouldUseDefaultWebSite { get; set; }
        public string MainSiteName { get; set; }
        public int MainSitePort { get; set; }
        public string ManagementSiteName { get; set; }
        public int ManagementSitePort { get; set; }        
    }
}
