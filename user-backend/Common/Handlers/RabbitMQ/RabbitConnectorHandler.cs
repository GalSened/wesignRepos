using Common.Interfaces;
using Common.Interfaces.RabbitMQ;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Handlers.RabbitMQ
{
    public class RabbitConnectorHandler : IRabbitConnector
    {
        private RabbitMQSettings _settings;
        private ILogger _logger;
        private IEncryptor _encryptor;
        private ConnectionFactory _factory = null;
        private IConnection _pubConnection = null;
        private IConnection _subConnection = null;
        private string _channelName = "" ;
        private IChannel _pubChannel = null;
        private IChannel _subChannel = null;
        static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
        private int numOfRetrys = 5;
        public bool IsConnectionOpen { get ; set; } = false;
        private AsyncEventHandler<BasicDeliverEventArgs> _consumer_Received;

        public RabbitConnectorHandler(IOptions<RabbitMQSettings> settings,
             ILogger logger, IEncryptor encryptor)
        {
            _settings = settings.Value;
            _logger = logger;
            _encryptor = encryptor;
        }


        public  Task InIt(AsyncEventHandler< BasicDeliverEventArgs> consumer_Received, string channelName)
        {
            _channelName = channelName;
            _consumer_Received = consumer_Received;
             return OpenConnectionIfNeeded();
            
        }

        public bool IsRabbitActive()
        {
            return _settings != null && _settings.UseRabbitSync && numOfRetrys > 0;
        }


        public async Task SendMessage<T>(T message)
        {
            if (await OpenConnectionIfNeeded())
            {

                if (IsConnectionOpen)
                {
                    var json = JsonConvert.SerializeObject(message);
                    var body = Encoding.UTF8.GetBytes(json);
                   await _pubChannel.BasicPublishAsync(exchange: _channelName, routingKey: "", body: body);
                }
                else 
                {
                    _logger.Warning("Rabbit Connector is closed");
                }
            }
        }


        private async Task<bool> OpenConnectionIfNeeded()
        {
            try
            {
                if (!IsConnectionOpen || _factory == null || _pubConnection == null
                    || _subConnection == null)
                {

                    await OpenConnection();
                }
                if (!_pubConnection.IsOpen || !_subConnection.IsOpen)
                {
                    IsConnectionOpen = false;
                    await OpenConnection();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while trying to connect RabbitMQ");
                numOfRetrys--;
                
                return false;
            }
            return _pubConnection.IsOpen && _subConnection.IsOpen;

        }

        private async Task CreateConnectionAndChannel()
        {


            _factory = new ConnectionFactory
            {
                AutomaticRecoveryEnabled = true,

            };
            if (!string.IsNullOrWhiteSpace(_settings.HostName))
            {
                _factory.HostName = _settings.HostName;
            }
            if (!string.IsNullOrWhiteSpace(_settings.Uri))
            {
                _factory.Uri = new Uri(_settings.Uri);
            }
            
            if (_settings.SslEnabled)
            {
                _factory.Ssl.Enabled = _settings.SslEnabled;
            }
            _factory.Port = _settings.Port;
            //_factory.HostName = _settings.HostName;

            if (!string.IsNullOrWhiteSpace(_settings.User))
            {
                _factory.UserName = _settings.User;
            }
            if (!string.IsNullOrWhiteSpace(_settings.Psss))
            {
                _factory.Password =  _encryptor.Decrypt(_settings.Psss);
            }
           
            if (string.IsNullOrWhiteSpace(_channelName))
            {
                _channelName = "DefualtQueue";
            }
           //_factory.DispatchConsumersAsync = true;
            _pubConnection = await _factory.CreateConnectionAsync();
            _subConnection = await _factory.CreateConnectionAsync();
            
            _pubChannel = await _pubConnection.CreateChannelAsync();
            _subChannel = await _subConnection.CreateChannelAsync();
        }

        private async Task OpenConnection()
        {
            try
            {
                await _semaphoreSlim.WaitAsync();
                if (!IsConnectionOpen && numOfRetrys > 0)
                {
                   await CreateConnectionAndChannel();

                     await _pubChannel.ExchangeDeclareAsync(_channelName, "fanout");

                    var queueName = (await _subChannel.QueueDeclareAsync()).QueueName;
                    await _subChannel.ExchangeDeclareAsync(_channelName, "fanout");
                    await _subChannel.QueueBindAsync(queue: queueName,
                                       exchange: _channelName,
                                       routingKey: "");
                    var consumer = new AsyncEventingBasicConsumer(_subChannel);

                    consumer.ReceivedAsync += _consumer_Received;
                   await _subChannel.BasicConsumeAsync(queue: queueName,
                                        autoAck: true,
                                        consumer: consumer);


                    IsConnectionOpen = true;
                    numOfRetrys = 5;
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }

        }

 
    }
}
