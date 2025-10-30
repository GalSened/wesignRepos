namespace Common.Models.Documents.Signers
{
    using Common.Enums.Contacts;
    using Common.Enums.Documents;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Signer
    {
        public Guid Id { get; set; }
        public Contact Contact { get; set; }
        public SignerStatus Status { get; set; }
        public DateTime? TimeSent { get; set; }
        public DateTime TimeViewed { get; set; }
        public DateTime TimeSigned { get; set; }
        public DateTime TimeRejected { get; set; }
        public DateTime TimeLastSent { get; set; }
        public SendingMethod SendingMethod { get; set; }
        public IEnumerable<Appendix> SenderAppendices { get; set; }
        public IEnumerable<SignerAttachment> SignerAttachments { get; set; }
        public Notes Notes { get; set; }
        public IEnumerable<SignerField> SignerFields { get; set; }
        public int LinkExpirationInHours { get; set; }
        public int IdentificationAttempts { get; set; }
        public SignerAuthentication SignerAuthentication { get; set; }
        public string IPAddress { get; set; }
        public string DeviceInformation { get; set; }

        public string FirstViewIPAddress { get; set; }

        public Signer()
        {
            SignerAttachments = new List<SignerAttachment>(); 
            SignerFields = new List<SignerField>();
            Notes = new Notes();
        }

    }
}
