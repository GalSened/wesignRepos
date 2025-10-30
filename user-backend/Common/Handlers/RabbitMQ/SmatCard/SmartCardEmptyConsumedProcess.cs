using Common.Handlers.RabbitMQ.Models;
using Common.Interfaces.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Handlers.RabbitMQ.SmatCard
{
    public class SmartCardEmptyConsumedProcess : ISmartCardConsumedProcess
    {
        public  Task DoProcess(BaseSmartCardEvent baseSmartCardEvent)
        {
            return Task.CompletedTask;
        }
    }
}
