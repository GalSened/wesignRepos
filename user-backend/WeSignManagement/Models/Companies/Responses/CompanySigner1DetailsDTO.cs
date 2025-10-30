namespace WeSignManagement.Models.Companies.Responses
{
    public class CompanySigner1DetailsDTO
    {
        public string CertId { get; set; }
        public string CertPassword { get; set; }
        public bool ShouldSignAsDefaultValue { get; set; }
        public bool ShouldShowInUI { get; set; }

        public Signer1ConfigurationDTO Signer1Configuration { get; set; }

        public CompanySigner1DetailsDTO()
        {
            this.Signer1Configuration = new Signer1ConfigurationDTO();
        }
    }
}
