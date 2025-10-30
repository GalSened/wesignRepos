namespace WeSign.Models.Documents.Responses
{
    using Common.Enums.Documents;
    using Common.Models;
    using Common.Models.Documents;
    using Common.Models.Documents.Signers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using WeSign.Models.Users.Responses;

    public class DocumentCollectionResposneDTO
    {
        public Guid DocumentCollectionId { get; set; }
        public Guid DistributionId { get; set; }
        public string Name { get; set; }
        public DocumentStatus DocumentStatus { get; set; }
        public DocumentMode Mode { get; set; }
        public IEnumerable<Guid> DocumentsIds { get; set; }
        public IEnumerable<SignerResponseDTO> Signers { get; set; }
        public DateTimeOffset? CreationTime { get; set; }
        public DateTimeOffset? SignedTime { get; set; }
        public bool IsWillDeletedIn24Hours { get; set; }
        public UserResponseDTO User { get; set; }
        public bool ShouldSignUsingSigner1AfterDocumentSigningFlow { get; set; }
        public DocumentCollectionResposneDTO() { }

        public DocumentCollectionResposneDTO(DocumentCollection documentCollection)
        {
            if (documentCollection != null)
            {
                DocumentCollectionId = documentCollection.Id;
                DistributionId = documentCollection.DistributionId;
                Name = documentCollection.Name;
                DocumentStatus = documentCollection.DocumentStatus;
                Mode = documentCollection.Mode;
                Signers = GetSigners(documentCollection.Signers);
                CreationTime = ConvertToSafeOffset(documentCollection.CreationTime);
                SignedTime = ConvertToSafeOffset(documentCollection.SignedTime);
                IsWillDeletedIn24Hours = documentCollection.IsWillDeletedIn24Hours;
                DocumentsIds = GetDocumentsIds(documentCollection.Documents);
                User = new UserResponseDTO(documentCollection.User, null);
                ShouldSignUsingSigner1AfterDocumentSigningFlow = documentCollection.ShouldSignUsingSigner1AfterDocumentSigningFlow;
            }
        }

        private DateTimeOffset? ConvertToSafeOffset(DateTime dateTime)
        {
            if (dateTime <= DateTime.MinValue || dateTime >= DateTime.MaxValue)
                return null;

            return new DateTimeOffset(dateTime);
        }

        private IEnumerable<Guid> GetDocumentsIds(IEnumerable<Document> documents)
        {
            var result = new List<Guid>();
            foreach (var document in documents ?? Enumerable.Empty<Document>())
            {
                result.Add(document.Id);
            }
            return result;
        }

        private IEnumerable<SignerResponseDTO> GetSigners(IEnumerable<Signer> signers)
        {
            var result = new List<SignerResponseDTO>();
            foreach (var signer in signers ?? Enumerable.Empty<Signer>())
            {
                result.Add(new SignerResponseDTO(signer));
            }
            return result;
        }
    }
}
