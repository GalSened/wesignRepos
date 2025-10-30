using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Documents.SplitSignature
{
    public class SignatureFieldData
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public byte[] PrepareSignaturePdfResult { get; set; }
        public byte[] Hash { get; set; }
        
        public byte[] SignedHash { get; set; }
        public bool IsPdfSignedByThisSignature { get; set; }
        public byte[] SignedCms { get; internal set; }
    }
}
