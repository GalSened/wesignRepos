using Common.Handlers.RabbitMQ.Models;
using Common.Hubs;
using Common.Interfaces;
using Common.Interfaces.RabbitMQ;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Handlers.RabbitMQ.SmatCard
{
    public class SmartCardConnectConsumedProcess : ISmartCardConsumedProcess
    {
        private ISmartCardSigningProcess _smartCardSigningProcess;
        private IHubContext<SmartCardSigningHub> _hubContext;

        public SmartCardConnectConsumedProcess(ISmartCardSigningProcess smartCardSigningProcess, IHubContext<SmartCardSigningHub> hubContext)
        {
            _smartCardSigningProcess = smartCardSigningProcess;
            _hubContext = hubContext;
        }

        public async Task DoProcess(BaseSmartCardEvent baseSmartCardEvent)
        {
            var smartCardProcessInput = _smartCardSigningProcess.GetSmartCardProcessInputByToken(baseSmartCardEvent.RoomToken);
            if (smartCardProcessInput != null)
            {
                if(!smartCardProcessInput.Clients.Contains(baseSmartCardEvent.ConnectionId))
                {
                    smartCardProcessInput.Clients.Add(baseSmartCardEvent.ConnectionId);
                    _smartCardSigningProcess.UpdateSmartCardInput(baseSmartCardEvent.RoomToken, smartCardProcessInput);
                    await _hubContext.Groups.AddToGroupAsync(baseSmartCardEvent.ConnectionId, baseSmartCardEvent.RoomToken);
                }                
            }
          
            await _hubContext.Clients.GroupExcept(baseSmartCardEvent.RoomToken, new string[] { baseSmartCardEvent.ConnectionId }).
                SendAsync(SmartCardEventsConstants.DESKTOP_CLIENT_JOIN, $"{baseSmartCardEvent.ConnectionId}", baseSmartCardEvent.RoomToken);
        }
    }
}
