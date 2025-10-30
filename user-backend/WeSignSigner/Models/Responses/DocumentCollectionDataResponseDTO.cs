namespace WeSignSigner.Models.Responses
{
    using Common.Enums;
    using Common.Enums.Documents;
    using Common.Enums.Users;
    using Common.Models;
    using Common.Models.Documents.Signers;
    using System.Collections.Generic;
    using System.Linq;

    public class DocumentCollectionDataResponseDTO
    {
        public IEnumerable<DocumentCountResponseDTO> Documents { get; set; }
        public int TotalPagesCount { get; set; }
        public DocumentCollectionDataDTO DocumentCollection { get; set; }
        public SenderDataDTO Sender { get; set; }
        public SignerDataDTO Signer { get; set; }
        public OtpMode OtpMode { get; set; }
        public Language Language { get; set; }
        public bool OauthNeeded { get; set; } = false;

        public DocumentCollectionDataResponseDTO()
        {
            DocumentCollection = new DocumentCollectionDataDTO();
            Sender = new SenderDataDTO();
            Signer = new SignerDataDTO();
            Documents = new List<DocumentCountResponseDTO>();
        }
        public DocumentCollectionDataResponseDTO(DocumentCollection documentCollection, SignerTokenMapping signerTokenMapping, System.Guid signerId)
        {
            DocumentCollection = new DocumentCollectionDataDTO();
            Sender = new SenderDataDTO();
            Signer = new SignerDataDTO();
            Documents = new List<DocumentCountResponseDTO>();
            DocumentCollection.Id = documentCollection.Id;
            DocumentCollection.Name = documentCollection.Name;
            DocumentCollection.Mode = documentCollection.Mode;
            Sender.Name = documentCollection.User?.Name;
            Sender.SignatureColor = documentCollection.User?.UserConfiguration?.SignatureColor;
            Sender.Note = documentCollection.Signers.FirstOrDefault(x => x.Id == signerId)?.Notes?.UserNote;
            Sender.Appendices = documentCollection.SenderAppendices.Select(x => x.Name);
            Sender.Base64Logo = documentCollection.User.CompanyLogo;
            Signer.Name = !string.IsNullOrWhiteSpace( signerTokenMapping.AuthName) ? signerTokenMapping.AuthName : documentCollection.Signers.FirstOrDefault(x => x.Id == signerId)?.Contact.Name;
            Signer.Means = GetSignerMeans(documentCollection.Signers.FirstOrDefault(x => x.Id == signerId));
            Signer.Seal = documentCollection.Signers.FirstOrDefault(x => x.Id == signerId)?.Contact?.Seals?.FirstOrDefault()?.Base64Image;
            Signer.IsLastSigner = documentCollection.Signers.LastOrDefault()?.Id == signerId;
            Signer.Attachments = documentCollection.Signers.FirstOrDefault(x => x.Id == signerId)?.SignerAttachments;
            Signer.Note = documentCollection.Signers.FirstOrDefault(x => x.Id == signerId).Notes?.SignerNote;
            OtpMode = documentCollection.Signers.FirstOrDefault(x => x.Id == signerId)?.SignerAuthentication?.OtpDetails?.Mode ?? OtpMode.None;
            Language = documentCollection.User?.UserConfiguration?.Language ?? Language.en;
            OauthNeeded = (documentCollection.Signers.FirstOrDefault(x => x.Id == signerId)?.SignerAuthentication?.AuthenticationMode ?? AuthMode.None) == AuthMode.ComsignVisualIDP;
        }

        private string GetSignerMeans(Signer signer)
        {
            if (signer?.SendingMethod == SendingMethod.Email)
            {
                return signer?.Contact?.Email;
            }
            if (signer?.SendingMethod == SendingMethod.SMS)
            {
                return signer?.Contact?.Phone;
            }
            return string.Empty;
        }
    }
}