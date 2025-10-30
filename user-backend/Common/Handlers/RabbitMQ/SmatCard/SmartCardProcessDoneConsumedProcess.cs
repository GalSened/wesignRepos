using Common.Handlers.RabbitMQ.Models;
using Common.Hubs;
using Common.Interfaces.RabbitMQ;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Handlers.RabbitMQ.SmatCard
{
    public class SmartCardProcessDoneConsumedProcess : ISmartCardConsumedProcess
    {
        private IHubContext<SmartCardSigningHub> _hubContext;

        public SmartCardProcessDoneConsumedProcess(IHubContext<SmartCardSigningHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task DoProcess(BaseSmartCardEvent baseSmartCardEvent)
        {
            if (baseSmartCardEvent.IsProcessDone)

            {
                await _hubContext.Clients.Group(baseSmartCardEvent.RoomToken).SendAsync(SmartCardEventsConstants.GET_SMARTCARD_SIGNING_RESULT,
                    true, baseSmartCardEvent.Data.ToString());
            }
            else
            {
                await _hubContext.Clients.Group(baseSmartCardEvent.RoomToken).SendAsync(SmartCardEventsConstants.GET_SMARTCARD_SIGNING_RESULT, false);
            }
        }
    }
}
