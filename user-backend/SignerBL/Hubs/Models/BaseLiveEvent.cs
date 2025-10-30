using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignerBL.Hubs.Models
{
    public  class BaseLiveEvent
    {
        public string Function { get; set; }
        public string DocumentCollectionToken { get; set; }
        public string ConnectionId { get; set; }
        public string DocumentId { get; set; }
        public object Data { get; set; }

    }

   
    
}
