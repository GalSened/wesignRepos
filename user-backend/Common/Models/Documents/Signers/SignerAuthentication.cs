using Common.Models.Configurations;

namespace Common.Models.Documents.Signers
{ 
    public class SignerAuthentication
    {
        public Signer1Credential Signer1Credential { get; set; }
        public OtpDetails OtpDetails { get; set; }
        public Signer1Configuration Signer1Configuration { get; set; }
        //public string Signer1Endpoint { get; set; }
        public AuthMode AuthenticationMode { get; set; }
    }

    public enum AuthMode
    {
        None = 0,
        IDP = 1,
        ComsignVisualIDP = 2,
        ComsignSamIDP = 3
    }
    
}
