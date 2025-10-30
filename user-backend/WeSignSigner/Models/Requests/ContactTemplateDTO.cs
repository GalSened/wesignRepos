using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeSignSigner.Models.Requests
{
    public class CreateDocumentDTO
    {
        public Guid TemplateId{ get; set; }
        public string SignerMeans { get; set; }
        public string PhoneExtension { get; set; }
        public string Fullname { get; set; }
    }
}
