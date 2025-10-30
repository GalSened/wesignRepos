using Common.Handlers.RabbitMQ.Models;
using Common.Hubs;
using Common.Interfaces;
using Common.Interfaces.RabbitMQ;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Common.Handlers.RabbitMQ.SmatCard
{
    public class SmartCardCreateGroupConsumedProcess : ISmartCardConsumedProcess
    {
        private ISmartCardSigningProcess _smartCardSigningProcess;
        private IHubContext<SmartCardSigningHub> _hubContext;

        public SmartCardCreateGroupConsumedProcess(ISmartCardSigningProcess smartCardSigningProcess, IHubContext<SmartCardSigningHub> hubContext)
        {
            _smartCardSigningProcess = smartCardSigningProcess;
            _hubContext = hubContext;
        }

        public async Task DoProcess(BaseSmartCardEvent baseSmartCardEvent)
        {
            var smartcardInput = _smartCardSigningProcess.GetSmartCardProcessInputByToken(baseSmartCardEvent.RoomToken);
            if (smartcardInput == null)
            {
                var inputSmartCardInput = JsonConvert.DeserializeObject<SmartCardInput>(baseSmartCardEvent.Data.ToString());
                _smartCardSigningProcess.CreateGroup(baseSmartCardEvent.RoomToken, inputSmartCardInput);
            }
            await _hubContext.Groups.AddToGroupAsync(baseSmartCardEvent.ConnectionId, baseSmartCardEvent.RoomToken);
            await _hubContext.Clients.Group(baseSmartCardEvent.RoomToken).SendAsync(SmartCardEventsConstants.GET_ROOM_ID, baseSmartCardEvent.RoomToken);
        }
    }
}
