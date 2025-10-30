using Common.Models.Documents.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Documents
{
    public class DocumentCollectionData
    {
        public DocumentCollection DocumentCollection { get; set; }
        public SignerTokenMapping SignerTokenMapping { get; set; }
        public int TotalCount { get; set; }
        public Guid SignerId {get; set;}
    }
}
