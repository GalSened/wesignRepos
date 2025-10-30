using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.RabbitMQ
{
    public  interface IMessageMQLiveConnector
    {
        bool IsRabbitActive { get; }
        void SendLiveMessage<T>(T message);
    }
}
