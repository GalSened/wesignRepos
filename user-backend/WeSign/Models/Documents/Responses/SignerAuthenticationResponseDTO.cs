using Common.Models.Documents.Signers;

namespace WeSign.Models.Documents.Responses
{
    public class SignerAuthenticationResponseDTO
    {
        public OtpDetails OtpDetails { get; set; }
        public AuthMode AuthenticationMode { get; set; }

        public SignerAuthenticationResponseDTO(SignerAuthentication signerAuthentication)
        {
            OtpDetails = signerAuthentication.OtpDetails;
            AuthenticationMode = signerAuthentication.AuthenticationMode;
        }
    }
}
