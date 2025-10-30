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
    public class MessageMQSignerIdentityConnectorHandler : IMessageMQSignerIdentityConnector
    {

        private readonly RabbitMQSettings _settings;
        private readonly ILogger _logger;
        private readonly IRabbitConnector _rabbitConnector;
        private readonly IHubContext<IdentityHub> _hubContext;
        private readonly string _channelName = "WSE_SignerIdentity";

        public MessageMQSignerIdentityConnectorHandler(IOptions<RabbitMQSettings> settings, ILogger logger,
           IHubContext<IdentityHub> hubContext, IRabbitConnector rabbitConnector)
        {
            _settings = settings.Value;
            _logger = logger;
            _rabbitConnector = rabbitConnector;
            _hubContext = hubContext;

            if (_settings != null && _settings.UseRabbitSync)
            {
                if (!string.IsNullOrWhiteSpace(_settings.IdentityQueueName))
                {
                    _channelName = _settings.IdentityQueueName;
                }
                rabbitConnector.InIt(Consumer_Received, _channelName);
            }
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            string data = Encoding.UTF8.GetString(e.Body.ToArray());
            var messageData = JsonConvert.DeserializeObject<BaseIdentityEvent>(data);
      

            switch (messageData.Function)
            {
                case IdentityEvents.IDENTITY_DONE:
                    {
                        await _hubContext.Clients.GroupExcept(messageData.RoomToken, new string[] { messageData.ConnectionId }).
                         SendAsync(IdentityEvent.ON_IDENTITY_DONE, messageData.Token);

                        break;
                    }
                case IdentityEvents.CONNECT:
                    {
                        await _hubContext.Groups.AddToGroupAsync(messageData.ConnectionId, messageData.RoomToken);
                        break;
                    }
                default:
                    {
                        _logger.Error("RabbitMQSignerIdentity unknown operation from type {Function}", messageData.Function);
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
