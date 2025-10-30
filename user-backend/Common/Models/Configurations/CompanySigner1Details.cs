namespace Common.Models.Configurations
{
    public class CompanySigner1Details
    {
        public string CertId { get; set; }
        public string CertPassword { get; set; }
        public bool ShouldSignAsDefaultValue { get; set; }
        public bool ShouldShowInUI { get; set; }

        public Signer1Configuration Signer1Configuration { get; set; }


    }
}
