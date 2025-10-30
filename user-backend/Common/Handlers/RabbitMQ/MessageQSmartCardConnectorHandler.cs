using Common.Handlers.RabbitMQ.Models;
using Common.Handlers.RabbitMQ.SmatCard;
using Common.Hubs;
using Common.Interfaces;
using Common.Interfaces.RabbitMQ;
using Common.Models.Settings;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Twilio.Types;

namespace Common.Handlers.RabbitMQ
{
    public class MessageQSmartCardConnectorHandler : IMessageQSmartCardConnector
    {
        private RabbitMQSettings _settings;
        private ILogger _logger;
        private IRabbitConnector _rabbitConnector;
        private IHubContext<SmartCardSigningHub> _hubContext;
        private ISmartCardSigningProcess _smartCardSigningProcess;
        private ISmartCardConsumedProcessFactory _smartCardConsumedProcessFactory;
        private string _channelName = "WSE_SmartCard";
        public bool IsRabbitActive
        {
            get
            {
                return _rabbitConnector.IsRabbitActive();
            }
        }

        public MessageQSmartCardConnectorHandler(IOptions<RabbitMQSettings> settings, ILogger logger,
           IHubContext<SmartCardSigningHub> hubContext, IRabbitConnector rabbitConnector,
            ISmartCardSigningProcess smartCardSigningProcess,
            ISmartCardConsumedProcessFactory smartCardConsumedProcessFactory)
        {
            _settings = settings.Value;
            _logger = logger;
            _rabbitConnector = rabbitConnector;
            _hubContext = hubContext;
            _smartCardSigningProcess = smartCardSigningProcess;
            _smartCardConsumedProcessFactory = smartCardConsumedProcessFactory;
            if (_settings != null && _settings.UseRabbitSync)
            {
                if (!string.IsNullOrWhiteSpace(_settings.SmartCardQueueName))
                {
                    _channelName = _settings.SmartCardQueueName;
                }
                rabbitConnector.InIt(Consumer_Received, _channelName);
                
            }
            smartCardSigningProcess.SetItemRemoveEvent(ItemRemoveEvent);
        }

        public void ItemRemoveEvent(string roomToken, SmartCardInput smartCardInput)
        {
            smartCardInput?.Clients?.ForEach(client =>
            {
                _hubContext.Groups.RemoveFromGroupAsync(client, roomToken);
            });
        }
        private  async Task Consumer_Received(object sender, BasicDeliverEventArgs e)
        {

            string data = Encoding.UTF8.GetString(e.Body.ToArray());
            BaseSmartCardEvent messageData = JsonConvert.DeserializeObject<BaseSmartCardEvent>(data);
            
            var process = _smartCardConsumedProcessFactory.CreateProcess(messageData);
            try
            {
               await process.DoProcess(messageData);
            }
            catch (Exception ex)
            {
                if (process is SmartCardSendHashConsumedProcess)
                {
                    _logger.Error(ex, "SmartCardSigningHub - Error in SendHashToDesktopClient");
                    await _hubContext.Clients.Group(messageData.RoomToken).SendAsync(SmartCardEventsConstants.GET_MESSAGE,
                        $"SmartCardSigningHub - Error in SendHashToDesktopClient - {ex.Message}");
                    // need to send message to all !!!!!!!!!
                    BaseSmartCardEvent smartCardEvent = new BaseSmartCardEvent()
                    {
                        Function = SmartCardEventsConstants.SEND_MESSAGE,
                        RoomToken = messageData.RoomToken,
                        Data = $"SmartCardSigningHub - Error in SendHashToDesktopClient - {ex.Message}",
                        ConnectionId = messageData.ConnectionId

                    };
                    SendSmartCardSigningProcessMessage(smartCardEvent);
                }
            }


        }

               
      
        public void SendSmartCardSigningProcessMessage<T>(T message)
        {
            if (_settings != null && _settings.UseRabbitSync)
            {                
                _rabbitConnector.SendMessage(message);
            }
        }
    }
}
