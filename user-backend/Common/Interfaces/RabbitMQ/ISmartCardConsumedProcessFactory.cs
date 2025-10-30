using Common.Handlers.RabbitMQ.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces.RabbitMQ
{
    public interface ISmartCardConsumedProcessFactory
    {

        ISmartCardConsumedProcess CreateProcess(BaseSmartCardEvent baseSmartCardEvent);
    }
}
