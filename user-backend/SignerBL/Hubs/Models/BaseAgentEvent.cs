using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignerBL.Hubs.Models
{
    public  class BaseAgentEvent
    {
        public string Function { get; set; }
        public string RoomToken { get; set; }
        public string ConnectionId { get; set; }
        public string Link { get; internal set; }
    }
}
