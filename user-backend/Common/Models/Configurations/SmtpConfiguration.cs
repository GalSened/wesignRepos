namespace Common.Models.Configurations
{
    public class SmtpConfiguration
    {
        public string From { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public bool EnableSsl { get; set; }
        public int AttachmentMaxSize { get; set; }
    }
}
