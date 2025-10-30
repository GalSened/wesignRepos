using Common.Enums;
using Common.Models.Documents.Signers;
using WeSign.Models.Documents;

namespace WeSign.Models.Signers
{
    public class ReplaceSignerWithDetailsDTO : ReplaceSignerDTO
    {
        public Notes NewNotes { get; set; }
        public OtpMode NewOtpMode { get; set; }
        public string NewOtpIdentification { get; set; }
        public AuthMode NewAuthenticationMode { get; set; }
    }
}