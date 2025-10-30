namespace DAL.DAOs.Documents
{
    using Common.Enums.Documents;
    using Common.Models;
    using Common.Models.Documents;
    using DAL.DAOs.Documents.Signers;
    using DAL.DAOs.Users;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    [Table("DocumentCollections")]
    public class DocumentCollectionDAO
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid GroupId { get; set; }
        public Guid DistributionId { get; set; }
        public string Name { get; set; }
        public DocumentStatus Status { get; set; }
        public DocumentMode Mode { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime SignedTime { get; set; }
        public string RedirectUrl { get; set; }
        public string CallbackUrl { get; set; }
        public bool? ShouldSend { get; set; }
        public bool? ShouldSendSignedDocument { get; set; }
        public bool? ShouldSignUsingSigner1AfterDocumentSigningFlow { get; set; }
        public bool ShouldEnableMeaningOfSignature { get; set; }
        public virtual ICollection<DocumentDAO> Documents { get; set; }
        public virtual ICollection<SignerDAO> Signers { get; set; }
        public virtual UserDAO User { get; set; }
        public virtual ICollection<SignerTokenMappingDAO> TokensMapping { get; set; }
        public string SenderIP { get; set; }

        public DocumentCollectionDAO()
        { }

        public DocumentCollectionDAO(DocumentCollection documentCollection)
        {
            Id = documentCollection.Id == Guid.Empty ? default : documentCollection.Id;
            UserId = documentCollection.UserId == Guid.Empty ? default : documentCollection.UserId;
            GroupId = documentCollection.GroupId;
            DistributionId = documentCollection.DistributionId;
            Name = documentCollection.Name;
            Status = documentCollection.DocumentStatus;
            Mode = documentCollection.Mode;
            CreationTime = documentCollection.CreationTime;
            SignedTime = documentCollection.SignedTime;
            RedirectUrl = documentCollection.RedirectUrl;
            CallbackUrl = documentCollection.CallbackUrl;
            ShouldSend = documentCollection.Notifications?.ShouldSendDocumentForSigning;
            ShouldSendSignedDocument = documentCollection.Notifications?.ShouldSendSignedDocument;
            Documents = documentCollection?.Documents.Select(d => new DocumentDAO(d)).ToList();
            Signers = documentCollection?.Signers.Select(s => new SignerDAO(s)).ToList();
            TokensMapping = documentCollection?.TokensMapping.Select(d => new SignerTokenMappingDAO(d)).ToList();
            ShouldSignUsingSigner1AfterDocumentSigningFlow = documentCollection.ShouldSignUsingSigner1AfterDocumentSigningFlow;
            ShouldEnableMeaningOfSignature = documentCollection.ShouldEnableMeaningOfSignature;
            SenderIP = documentCollection.SenderIP;
            
        }

        public DocumentCollectionDAO(DeletedDocumentCollection documentCollection)
        {
            Id = documentCollection.Id == Guid.Empty ? default : documentCollection.Id;
            UserId = documentCollection.UserId == Guid.Empty ? default : documentCollection.UserId;
            GroupId = documentCollection.GroupId;
            DistributionId = documentCollection.DistributionId;
            Status = documentCollection.DocumentStatus;
            CreationTime = documentCollection.CreationTime;
            Documents = documentCollection?.Documents.Select(d => new DocumentDAO(d)).ToList();

        }
    }
}
