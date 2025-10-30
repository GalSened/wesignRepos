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
    public class SmartCardSendMessageConsumedProcess : ISmartCardConsumedProcess
    {
        private IHubContext<SmartCardSigningHub> _hubContext;

        public SmartCardSendMessageConsumedProcess(IHubContext<SmartCardSigningHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task DoProcess(BaseSmartCardEvent baseSmartCardEvent)
        {
            await _hubContext.Clients.GroupExcept(baseSmartCardEvent.RoomToken, new string[] { baseSmartCardEvent.ConnectionId }).
                SendAsync(SmartCardEventsConstants.GET_MESSAGE, baseSmartCardEvent.Data.ToString(), baseSmartCardEvent.RoomToken);
        }
    }
}
