using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.RabbitMQ
{
    public interface IRabbitConnector
    {
        bool IsConnectionOpen { get; set; }

        Task InIt(AsyncEventHandler<BasicDeliverEventArgs> consumer_Received, string _channelName);
        bool IsRabbitActive();
         Task SendMessage<T>(T message);
    }
}
