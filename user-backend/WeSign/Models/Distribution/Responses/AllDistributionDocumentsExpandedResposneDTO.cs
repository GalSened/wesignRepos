using Common.Enums.Documents;
using Common.Models;
using Common.Models.Documents;
using Common.Models.Documents.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeSign.Models.Documents.Responses;

namespace WeSign.Models.Distribution.Responses
{
    public class AllDistributionDocumentsExpandedResposneDTO
    {
        public int TotalPending { get; set; }
        public int TotalSigned { get; set; }
        public int TotalServerSigned { get; set; }
        public int TotalDeclined { get; set; }
        public int TotalFailed { get; set; }

        public IEnumerable<DistributionDocumentExpandedResposneDTO> DocumentCollections { get; set; }
        public int TotalCreatedButNotSent { get; internal set; }
        public int TotalViewed { get;  set; }
        public bool ShouldSignUsingSigner1AfterDocumentSigningFlow { get; set; }
    }

    public class DistributionDocumentExpandedResposneDTO : DistributionDocumentResposneDTO
    {
        public DocumentStatus DocumentStatus { get; set; }
        public IEnumerable<Guid> DocumentsIds { get; set; }
        public SignerResponseDTO Signer { get; set; }

        public DistributionDocumentExpandedResposneDTO() { }
        public DistributionDocumentExpandedResposneDTO(DocumentCollection documentCollection) : base(documentCollection)
        {
            DocumentStatus = documentCollection.DocumentStatus;
            DocumentsIds = GetDocumentsIds(documentCollection.Documents);
            Signer = new SignerResponseDTO(documentCollection.Signers.First());
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
    }
}
