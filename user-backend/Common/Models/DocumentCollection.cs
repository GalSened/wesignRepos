namespace Common.Models
{
    using Common.Enums.Documents;
    using Common.Models.Documents;
    using Common.Models.Documents.Signers;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class DocumentCollection
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid GroupId { get; set; }
        public Guid DistributionId { get; set; }
        public string Name { get; set; }
        public IEnumerable<Document> Documents { get; set; }
        public DocumentStatus DocumentStatus { get; set; }
        public DocumentMode Mode { get; set; }
        public string UserNote { get; set; }
        public IEnumerable<Signer> Signers { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime SignedTime { get; set; }
        public string RedirectUrl { get; set; }
        public string CallbackUrl { get; set; }
        public DocumentNotifications Notifications { get; set; }
        public IEnumerable<Appendix> SenderAppendices { get; set; }
        public bool IsWillDeletedIn24Hours { get; set; }
        public User User { get; set; }
        public IEnumerable<SignerTokenMapping> TokensMapping { get; set; }
        public bool ShouldSignUsingSigner1AfterDocumentSigningFlow { get; set; }
        public bool ShouldEnableMeaningOfSignature { get; set; }
        
        public string SenderIP { get; set; }

        public DocumentCollection()
        {
            Documents = new List<Document>();
            Signers = new List<Signer>();
            Notifications = new DocumentNotifications();
            TokensMapping = Enumerable.Empty<SignerTokenMapping>();
            DocumentStatus = DocumentStatus.Created;
        }
    }
}
