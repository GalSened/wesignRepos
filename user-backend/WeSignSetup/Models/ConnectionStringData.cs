namespace WeSignSetup.Models
{
    public class ConnectionStringData
    {
        public bool ShouldUseWindowsAuthentication { get; set; }
        public string ConnectionStringWithWindowsAuthentication { get; set; }
        public string ConnectionStringWithDbUser { get; set; }
    }
}
