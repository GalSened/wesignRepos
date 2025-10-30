using Common.Enums;

namespace Common.Models.Configurations
{
    public class FileGateScannerConfiguration
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string Endpoint { get; set; }
        public FileGateScannerProviderType Provider { get; set; }
    }
}
