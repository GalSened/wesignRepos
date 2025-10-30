namespace WeSignManagement.Models.Configurations
{
    public class SmtpDetailsDTO
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string From { get; set; }
        public bool EnableSsl { get; set; }
        public int Port { get; set; }
        public string Server { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
    }
}
