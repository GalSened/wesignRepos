using Common.Enums;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.RabbitMQ;
using Common.Models.Settings;
using CsvHelper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Net.Sf.Pkcs11.Objects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using SignerBL.Hubs;
using SignerBL.Hubs.Models;
using System;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SignerBL.Handlers.RabbitMQ
{
    public class MessageQLiveConnectorHandler : IMessageMQLiveConnector
    {
        private RabbitMQSettings _settings;
        private ILogger _logger;
        private IRabbitConnector _rabbitConnector;
        private IHubContext<LiveHub> _hubContext;
    
        private string _channelName = "WSE_Live";
      

        
        public bool IsRabbitActive  { get {
                return _rabbitConnector.IsRabbitActive();
            } }

        public MessageQLiveConnectorHandler(IOptions<RabbitMQSettings> settings,  ILogger logger,
            IHubContext<LiveHub> hubContext, IRabbitConnector rabbitConnector)
        {
            _settings = settings.Value;
           _logger = logger;
            _rabbitConnector = rabbitConnector;
            _hubContext = hubContext;
            if (_settings != null && _settings.UseRabbitSync)
            {
                if (!string.IsNullOrWhiteSpace(_settings.LiveQueueName))
                {
                    _channelName = _settings.LiveQueueName;
                }
                rabbitConnector.InIt(Consumer_Received, _channelName);
            }
        }

        public void SendLiveMessage<T>(T message)
        {
            if (_settings != null && _settings.UseRabbitSync)
            {
                _rabbitConnector.SendMessage(message);
            }            
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
         

            BaseLiveEvent messageData = JsonConvert.DeserializeObject<BaseLiveEvent>(Encoding.UTF8.GetString(e.Body.ToArray()));
          
            string documentCollectionToken = messageData.DocumentCollectionToken;
            string sendByUser = messageData.ConnectionId;
            switch (messageData.Function)
            {
                case LiveEvents.ON_CONNECT:
                    {
                        await _hubContext.Groups.AddToGroupAsync(sendByUser, documentCollectionToken);
                        break;
                    }
                case LiveEvents.ON_SCROLL:
                    {
                        var dataItem = JsonConvert.DeserializeObject<ScrollParams>(messageData.Data.ToString());
                     
                        await _hubContext.Clients.GroupExcept(documentCollectionToken, new string[] { sendByUser }).SendAsync(LiveEvents.ON_SCROLL,
                            dataItem.Left, dataItem.Top,
                           dataItem.Page);
                        break;
                    }
                case LiveEvents.ON_FIELD_DATA_CHANGED:
                    {
                        var dataItem = JsonConvert.DeserializeObject<FieldRequest>(messageData.Data.ToString());
                        await _hubContext.Clients.GroupExcept(documentCollectionToken, new string[] { sendByUser }).
                            SendAsync(LiveEvents.ON_FIELD_DATA_CHANGED, messageData.DocumentId, dataItem);                            
                        break;
                    }
                case LiveEvents.ON_ZOOM:
                    {
                        await _hubContext.Clients.GroupExcept(documentCollectionToken, new string[] { sendByUser }).SendAsync(LiveEvents.ON_ZOOM,
                          messageData.Data );
                        break;
                    }
                case LiveEvents.ON_CHANGE_BACKGROUD:
                    {
                        await _hubContext.Clients.GroupExcept(documentCollectionToken, new string[] { sendByUser }).SendAsync(LiveEvents.ON_CHANGE_BACKGROUD,
                            messageData.Data);
                        break;
                    }
                case LiveEvents.ON_FINISH_SIGNING:
                    {
                        await _hubContext.Clients.GroupExcept(documentCollectionToken, new string[] { sendByUser }).SendAsync(LiveEvents.ON_FINISH_SIGNING,
                           messageData.Data);
                        break;
                    }
                case LiveEvents.ON_FINISH_AS_SENDER:
                    {
                        await _hubContext.Clients.GroupExcept(documentCollectionToken, new string[] { sendByUser }).SendAsync(LiveEvents.ON_FINISH_AS_SENDER);
                        break;
                    }
                case LiveEvents.ON_PING_TO_OTHERS:
                    {
                        await _hubContext.Clients.GroupExcept(documentCollectionToken, new string[] { sendByUser }).SendAsync(LiveEvents.ON_PING_TO_OTHERS,
                          messageData.Data);
                        break;
                    }
                case LiveEvents.ON_SIGNER_DECLINE:
                    {
                        await _hubContext.Clients.GroupExcept(documentCollectionToken, new string[] 
                        { sendByUser }).SendAsync(LiveEvents.ON_SIGNER_DECLINE);
                        break;
                    }
                default:
                    {
                        _logger.Error("RabbitMQLive unknown operation from type {Function}", messageData.Function);
                        break;
                    }
            }

        }      
    }
}
