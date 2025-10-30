using Common.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Documents.SplitSignature
{
    public class DocumentSplitSignatureDataProcessInput
    {
        public Guid Id { get; set; }
        public List<SignatureFieldData> SignatureFields { get; set; } = new List<SignatureFieldData>();
    }
}
