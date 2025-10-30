using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSign.Models.Documents
{
    public class CreateSimpleDocumentDTO
    {
        public Guid TemplateId { get; set; }
        public string DocumentName { get; set; }
        public string SignerName { get; set; }
        public string SignerMeans { get; set; }
        public string SignerExtention { get; set; }
        public string RediretUrl { get; set; }
        public string CallBackUrl { get; set; }
        public string SignerOTPMeans { get; set; }
        public bool ShouldSend { get; set; } = true;
        public bool ShouldSendSignedDocument { get; set; } = true;
    }
}
