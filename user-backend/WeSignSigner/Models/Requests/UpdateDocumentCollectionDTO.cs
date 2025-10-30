using Common.Enums.Documents;
using Common.Models.Documents.Signers;
using System.Collections.Generic;

namespace WeSignSigner.Models.Requests
{
    public class UpdateDocumentCollectionDTO
    {

        public string SignerNote { get; set; }
        public IEnumerable<SignerAttachment> SignerAttachments { get; set; }
        public IEnumerable<UpdateDocumentDTO> Documents { get; set; }
        public DocumentOperation Operation { get; set; }
        public SignerAuthentication SignerAuthentication { get; set; }
        public bool UseForAllFields { get; set; }
    }
}
