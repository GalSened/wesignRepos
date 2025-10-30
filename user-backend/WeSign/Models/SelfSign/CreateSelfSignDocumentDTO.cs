using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSign.Models.SelfSign
{
    public class CreateSelfSignDocumentDTO
    {
        public string Base64File { get; set; }
        public string Name { get; set; }        
        public string SourceTemplateId { get; set; }
    }
}
