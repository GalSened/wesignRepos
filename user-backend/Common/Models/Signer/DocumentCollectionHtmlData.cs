using Common.Models.Documents.Signers;
using Common.Models.Files.PDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class DocumentCollectionHtmlData
    {
        public SignerTokenMapping SignerTokenMapping { get; set; }
        public List<FieldData> FieldsData { get; set; }
        public string HTML { get; set; }
        public string JS { get; set; }
    }
}
