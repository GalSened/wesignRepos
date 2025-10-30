using Common.Enums;

namespace WeSignManagement.Models.Configurations
{
    public class SmsDetailsDTO
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string From { get; set; }
        public ProviderType Provider { get; set; }
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
    }
}
