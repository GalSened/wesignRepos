namespace WeSign.Models.Documents.Responses
{
    using Common.Models.Documents.Signers;
    using System;
    using System.Collections.Generic;

    public class CreateDocumentCollectionResposneDTO
    {
        public Guid DocumentCollectionId { get; set; }
        public IEnumerable<SignerLink> SignerLinks { get; set; }
    }
}
