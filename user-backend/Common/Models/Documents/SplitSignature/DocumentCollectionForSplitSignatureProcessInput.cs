using Common.Models.Documents.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Documents.SplitSignature
{
    public class DocumentCollectionForSplitSignatureProcessInput
    {

        public Guid CollectionId { get; set; }
        public List<DocumentSplitSignatureDataProcessInput> Documents { get; set; }
        public List<DocumentSplitFileDataProcessInput> DocumentsData { get; set; }
        public SignerTokenMapping SignerTokenMapping { get; set; }
        public string AccessToken { get; set; }
        public string SignerCredId { get; set; }
        public string CertificateInfo { get; set; }
        public string CollectionName { get; set; }
        public int NumberOfSignatures { get; set; } = 0;
        public int NumberOfSignaturesSigned { get; set; } = 0;

        public DocumentCollectionForSplitSignatureProcessInput()
        {

            Documents = new List<DocumentSplitSignatureDataProcessInput>();
            DocumentsData = new List<DocumentSplitFileDataProcessInput>();
        }

    }
}
