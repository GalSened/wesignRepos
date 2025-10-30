using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Documents.SplitSignature
{
    public class DocumentSplitFileDataProcessInput
    {
        public Guid Id { get; set; }
        public byte[] Data { get; set; }
    }
}
