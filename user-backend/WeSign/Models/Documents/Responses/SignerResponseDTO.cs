
namespace WeSign.Models.Documents.Responses
{
    using Common.Enums.Contacts;
    using Common.Enums.Documents;
    using Common.Models.Documents.Signers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using WeSign.Models.Contacts.Responses;

    public class SignerResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public SignerStatus Status { get; set; }
        public DateTime? TimeSent { get; set; }
        public DateTime TimeViewed { get; set; }
        public DateTime TimeSigned { get; set; }
        public DateTime TimeRejected { get; set; }
        public SendingMethod SendingMethod { get; set; }
        public string SignerNote { get; set; }
        public string UserNote { get; set; }
        public IEnumerable<SignerAttachmentResponseDTO> Attachments { get; set; }
        public ContactResponseDTO Contact { get; set; }
        public SignerAuthenticationResponseDTO SignerAuthentication { get; set; }

        public SignerResponseDTO(Signer signer)
        {
            if (signer != null)
            {
                Id = signer.Id;
                Name = signer?.Contact?.Name;
                SendingMethod = signer.SendingMethod;
                SignerNote = signer.Notes?.SignerNote;
                UserNote = signer.Notes?.UserNote;
                Status = signer.Status;
                TimeSent = signer.TimeLastSent;
                TimeViewed = signer.TimeViewed;
                TimeSigned = signer.TimeSigned;
                TimeRejected = signer.TimeRejected;
                Attachments = GetSignerAttachments(signer.SignerAttachments);
                Contact = new ContactResponseDTO(signer.Contact);
                SignerAuthentication = new SignerAuthenticationResponseDTO(signer.SignerAuthentication);
            }
        }
        private IEnumerable<SignerAttachmentResponseDTO> GetSignerAttachments(IEnumerable<SignerAttachment> signerAttachments)
        {
            var result = new List<SignerAttachmentResponseDTO>();
            foreach (var signerAttachment in signerAttachments ?? Enumerable.Empty<SignerAttachment>())
            {
                result.Add(new SignerAttachmentResponseDTO(signerAttachment));
            }
            return result;
        }
    }
}
