using Common.Enums.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Documents.SplitSignature
{
    public class SplitDocumentProcess
    {
        public string Url { get; set; }
        public SplitSignProcessStep ProcessStep { get; set; }
    }
}
