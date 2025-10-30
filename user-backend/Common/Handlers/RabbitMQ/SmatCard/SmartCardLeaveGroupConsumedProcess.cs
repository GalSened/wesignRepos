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
    public class SmartCardLeaveGroupConsumedProcess : ISmartCardConsumedProcess
    {
        private ISmartCardSigningProcess _smartCardSigningProcess;
        private IHubContext<SmartCardSigningHub> _hubContext;

        public SmartCardLeaveGroupConsumedProcess(ISmartCardSigningProcess smartCardSigningProcess, IHubContext<SmartCardSigningHub> hubContext)
        {
            _smartCardSigningProcess = smartCardSigningProcess;
            _hubContext = hubContext;
        }

        public async Task DoProcess(BaseSmartCardEvent baseSmartCardEvent)
        {
            var smartCardProcessInput = _smartCardSigningProcess.GetSmartCardProcessInputByToken(baseSmartCardEvent.RoomToken);
            if (smartCardProcessInput != null)
            {
                foreach (var client in smartCardProcessInput.Clients)
                {
                    await _hubContext.Groups.RemoveFromGroupAsync(client, baseSmartCardEvent.RoomToken);
                }

            }
        }
    }
}
