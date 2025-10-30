using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.RabbitMQ
{
    public interface IMessageQSmartCardConnector
    {
        bool IsRabbitActive { get; }
        void SendSmartCardSigningProcessMessage<T>(T message);
    }
}
