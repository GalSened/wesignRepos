using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Handlers.RabbitMQ.Models
{
    public class BaseSmartCardEvent
    {
        public string Function { get; set; }
        public string DocumentCollectionToken { get; set; }
        public string ConnectionId { get; set; }
        public string DocumentId { get; set; }
        public object Data { get; set; }
        public string RoomToken { get;  set; }
        public bool IsProcessDone { get; set; }
    }
}
