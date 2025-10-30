using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models.Settings
{
    public class RabbitMQSettings
    {
        public bool UseRabbitSync { get; set; }
        public string HostName{get; set;}
        public string Uri { get; set; }
        public bool SslEnabled { get; set; }
        public string User { get; set; }
        public string Psss { get; set; }
        public string LiveQueueName { get; set; }
        public string SmartCardQueueName { get; set; }
        public string AgentQueueName { get; set; }
        public int Port { get; set; }
        public string IdentityQueueName { get; set; }
        public string DocumentsCollectionQueueName { get; set; } = "DocumentsCollectionQueueDefulaltName";

    }
}
