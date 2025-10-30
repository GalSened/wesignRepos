using Common.Handlers.RabbitMQ.Models;
using Common.Interfaces.DB;
using Common.Interfaces.RabbitMQ;
using Common.Models.Settings;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using Serilog;
using SignerBL.Hubs;
using SignerBL.Hubs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SignerBL.Handlers.RabbitMQ
{
    public class MessageAgentConnectorHandler : IMessageMQAgentConnector
    {

        private readonly RabbitMQSettings _settings;
        private readonly ILogger _logger;
        private readonly IRabbitConnector _rabbitConnector;
        private readonly IHubContext<AgentHub> _hubContext;
        
        private string _channelName = "WSE_Agent";


        public MessageAgentConnectorHandler(IOptions<RabbitMQSettings> settings, ILogger logger,
           IHubContext<AgentHub> hubContext, IRabbitConnector rabbitConnector)
        {
            _settings = settings.Value;
            _logger = logger;
            _rabbitConnector = rabbitConnector;
            _hubContext = hubContext;
            
            if (_settings != null && _settings.UseRabbitSync)
            {
                if (!string.IsNullOrWhiteSpace(_settings.AgentQueueName))
                {
                    _channelName = _settings.AgentQueueName;
                }
                rabbitConnector.InIt(Consumer_Received, _channelName);
            }
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            string data = Encoding.UTF8.GetString(e.Body.ToArray());
            var messageData = JsonConvert.DeserializeObject<BaseAgentEvent>(data);
            messageData.RoomToken = messageData.RoomToken?.ToLower();
            switch (messageData.Function)
            {
                case AgentEvents.PING:
                    {
                        await _hubContext.Clients.Group(messageData.RoomToken).SendAsync(AgentEvents.PING);
                        break;
                    }
                case AgentEvents.CONNECT:
                    {
                        await _hubContext.Groups.AddToGroupAsync(messageData.ConnectionId, messageData.RoomToken);
                        break;
                    }
                case AgentEvents.SEND_LINK:
                    {
                        await _hubContext.Clients.GroupExcept(messageData.RoomToken, new string[] { messageData.ConnectionId }).
                            SendAsync(AgentEvents.ON_LINK_CHANGE, messageData.Link);

                        break;
                    }
                case AgentEvents.MOVE_TO_AD:
                    {
                        await _hubContext.Clients.GroupExcept(messageData.RoomToken, new string[] { messageData.ConnectionId }).
                          SendAsync(AgentEvents.ON_MOVE_TO_AD, messageData.Link);                        
                        break;
                    }
                default:
                    {
                        _logger.Error("RabbitMQAgent unknown operation from type {Function}", messageData.Function);
                        break;
                    }

            }
          
        }

        public bool IsRabbitActive
        {
            get
            {
                return _rabbitConnector.IsRabbitActive();
            }
        }

        public void SendLiveMessage<T>(T message)
        {
            if (_settings != null && _settings.UseRabbitSync)
            {
                _rabbitConnector.SendMessage(message);
            }
        }
    }
}
