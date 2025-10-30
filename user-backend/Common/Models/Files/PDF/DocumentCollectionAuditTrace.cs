using Common.Models;
using Common.Models.Documents.Signers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Files.PDF
{
    public class DocumentCollectionAuditTrace
    {
        public Guid CollectionId { get; set; }
        public string CollectionName { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserPhone { get; set; }
        public DateTime CreationTime { get; set; }
        public string CreationIp { get; set; }
        public List<AuditTraceSigner> AuditTraceSigners { get; set; } = new List<AuditTraceSigner>();
    }
    public class AuditTraceSigner
    {
        public string Name { get; set; }
        public string Means { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string FirstViewIPAddress { get; set; }
        public string SignedFromIpAddress { get; set; }
        public string DeviceInformation { get; set; }
        public string DocumentPassword { get; set; }
        public string DocumentOTP { get; set; }
        public string DocumentIDP { get; set; }
        public DateTime TimeLastSent { get; set; } 
        public DateTime TimeSent { get; set; }
        public DateTime TimeSigned { get; set; }
        public DateTime TimeViewed { get; set; }
        public DateTime TimeRejected { get; set; }
        
    }


}

