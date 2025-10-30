using Microsoft.Extensions.Options;
using HistoryIntegratorService.Common.Enums;
using HistoryIntegratorService.Common.Extensions;
using HistoryIntegratorService.Common.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ILogger = Serilog.ILogger;
using HistoryIntegratorService.Common.Models;

namespace HistoryIntegratorService.BL.Handlers
{
    public class RabbitMqHandler : IRabbitMQ
    {
        public EventHandler<BasicDeliverEventArgs> OnMessage { get; set; }
        private ILogger _logger;
        private ConnectionFactory _factory;
        private GeneralSettings _settings;
        private IConnection _connection;
        private IModel _channel;
        private readonly int _defaultConnectionRetries;

        public RabbitMqHandler(ILogger logger, IOptions<GeneralSettings> options)
        {
            _logger = logger;
            _settings = options.Value;
            _defaultConnectionRetries = _settings.RabbitMq.ConnectionRetries;
            InitConnection();
        }

        public void Consume(CancellationToken cancellationToken, string channelName)
        {
            if (_connection == null || !_connection.IsOpen)
            {
                _logger.Error(ResultCode.RabbitMqConnectionClose.GetNumericString());
                throw new InvalidOperationException(ResultCode.RabbitMqConnectionClose.GetNumericString());
            }
            if (OnMessage == null)
            {
                _logger.Error(ResultCode.InvalidRabbitMqMessageEvent.GetNumericString());
                throw new InvalidOperationException(ResultCode.InvalidRabbitMqMessageEvent.GetNumericString());
            }
            _channel = _connection.CreateModel();
            var queueName = _channel.QueueDeclare(queue: channelName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null).QueueName;
            _channel.ExchangeDeclare(channelName, "fanout");
            _channel.QueueBind(queue: queueName,
                exchange: channelName,
                routingKey: "");
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += OnMessage;
            _channel.BasicConsume(queue: queueName,
                                        autoAck: true,
                                        consumer: consumer);
        }

        private void InitConnection()
        {
            if (string.IsNullOrWhiteSpace(_settings.RabbitMq.HostName))
            {
                _logger.Error(ResultCode.InvalidRabbitMqHostname.GetNumericString());
                throw new InvalidOperationException(ResultCode.InvalidRabbitMqHostname.GetNumericString());
            }
            if (!_settings.RabbitMq.Port.HasValue)
            {
                _logger.Error(ResultCode.InvalidRabbitMqPort.GetNumericString());
                throw new InvalidOperationException(ResultCode.InvalidRabbitMqPort.GetNumericString());
            }
            if (string.IsNullOrWhiteSpace(_settings.RabbitMq.UserName) || string.IsNullOrWhiteSpace(_settings.RabbitMq.Password))
            {
                _logger.Error(ResultCode.InvalidRabbitMqUsernamePassword.GetNumericString());
                throw new InvalidOperationException(ResultCode.InvalidRabbitMqUsernamePassword.GetNumericString());
            }

            try
            {
                _factory = new ConnectionFactory()
                {
                    HostName = _settings.RabbitMq.HostName,
                    Port = _settings.RabbitMq.Port.Value,
                    UserName = _settings.RabbitMq.UserName,
                    Password = _settings.RabbitMq.Password
                };
                if (!string.IsNullOrWhiteSpace(_settings.RabbitMq.Uri))
                {
                    _factory.Uri = new Uri(_settings.RabbitMq.Uri);
                }
                if (_settings.RabbitMq.SslEnabled)
                {
                    _factory.Ssl.Enabled = true;
                }
                TryToCreateConnection();
                if (_connection == null || !_connection.IsOpen)
                {
                    _logger.Error("Max attempts to connect to rabbit reached");
                    throw new InvalidOperationException(ResultCode.MaximumRabbitMqConnectionRetriesAttempted.GetNumericString());
                }
                _settings.RabbitMq.ConnectionRetries = _defaultConnectionRetries;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "RabbitMQ failed to connect");

            }
        }

        private void TryToCreateConnection()
        {
            while ((_connection == null || !_connection.IsOpen) && _settings.RabbitMq.ConnectionRetries > 0)
            {
                try
                {
                    _connection = _factory.CreateConnection();
                }
                catch (Exception ex)
                {
                    _settings.RabbitMq.ConnectionRetries--;
                    _logger.Error(ex, "RabbitMQ attempt to connect failed");
                    Thread.Sleep(_settings.RabbitMq.DelayBetweenConnectionAttemptsMS);
                }
            }
        }
    }
}
