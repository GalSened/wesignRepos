using Common.Enums.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class DocumentOperationNotification
    {
        public DocumentNotification NotificationType { get; set; }
        public Guid DocumentCollectionId { get; set; }
        public string DocumentName { get; set; }    
        public DocumentStatus DocumentStatus { get; set; }
        public DateTime OccuranceTimeStamp { get; set; }
        public Guid CompanyId { get; set; }
      
        public string CompanyName { get; set; }
        
        public Guid SignerId { get; set; }
        public string SignerName { get; set; }
        public string SignerMessage { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public List<string> TemplatesIds { get; set;} = new List<string>();
        

    }

    public class DocumentOperationNotificationExtraInfo : DocumentOperationNotification
    {      
        public Guid GroupId { get; set; }       
        public Guid ContactId { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }


    }


}
