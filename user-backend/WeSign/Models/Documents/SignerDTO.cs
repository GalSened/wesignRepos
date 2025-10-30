namespace WeSign.Models.Documents
{
    using Common.Enums;
    using Common.Enums.Documents;
    using Common.Models.Documents;
    using Common.Models.Documents.Signers;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SignerDTO
    {
        public Guid ContactId { get; set; }
        public SendingMethod SendingMethod { get; set; }
        public string ContactMeans { get; set; }
        public string PhoneExtension { get; set; } = "+972";
        public string ContactName { get; set; }
        public IEnumerable<SignerAttachmentDTO> SignerAttachments { get; set; }
        //TODO if one of attribute of SignerFieldDTO set to same value all attribute should fill in also
        public IEnumerable<SignerFieldDTO> SignerFields { get; set; }
        public int LinkExpirationInHours { get; set; }
        public string  SenderNote { get; set; }
        public IEnumerable<Appendix> SenderAppendices { get; set; }
        public string OtpIdentification { get; set; }
        public OtpMode OtpMode { get; set; }
        public AuthMode AuthenticationMode { get; set; }

        public SignerDTO()
        {
            SignerAttachments = new List<SignerAttachmentDTO>();
            SignerFields = new List<SignerFieldDTO>();
        }
    }
}