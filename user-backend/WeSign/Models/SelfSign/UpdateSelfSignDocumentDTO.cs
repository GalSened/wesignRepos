namespace WeSign.Models.SelfSign
{
    using Common.Models.Files.PDF;
    using Common.Enums.Documents;
    using System;
    using Common.Models.Documents.Signers;

    public class UpdateSelfSignDocumentDTO
    {
        public Guid DocumentCollectionId { get; set; }
        public Guid DocumentId { get; set; }
        public string Name { get; set; }
        public PDFFields Fields { get; set; }
        public DocumentOperation Operation { get; set; }
        public SignerAuthentication signerAuthentication { get; set; }
        public bool UseForAllFields { get; set; }
    }
}
