using Common.Models.Documents.Signers;
using Common.Models.Files.PDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSign.Models.Distribution.Requests
{
    public class CreateDistributionDocumentsDTO
    {
        public string Name { get; set; }
        public Guid TemplateId { get; set; }
        public IEnumerable<BaseSigner> Signers { get; set; }
        public bool SignDocumentWithServerSigning { get; set; }
        public bool ShouldEnableMeaningOfSignature { get; set; }
        
    }
}
