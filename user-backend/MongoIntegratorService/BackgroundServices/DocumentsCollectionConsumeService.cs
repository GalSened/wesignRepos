using Microsoft.Extensions.Options;
using HistoryIntegratorService.Common.Enums;
using HistoryIntegratorService.Common.Extensions;
using HistoryIntegratorService.Common.Interfaces;
using HistoryIntegratorService.Common.Models;
using Newtonsoft.Json;
using RabbitMQ.Client.Events;
using System.Text;

namespace HistoryIntegratorService.BackgroundServices
{
    public class DocumentsCollectionConsumeService : BackgroundService
    {
        private readonly IRabbitMQ _rabbitMq;
        private readonly IDocumentCollection _documentCollection;
        private readonly GeneralSettings _settings;

        public DocumentsCollectionConsumeService(IRabbitMQ rabbitMq, IDocumentCollection documentCollection, IOptions<GeneralSettings> options)
        {
            _rabbitMq = rabbitMq;
            _documentCollection = documentCollection;
            _settings = options.Value;
            _rabbitMq.OnMessage += OnMessageConsumed;
        }

        private void OnMessageConsumed(object? model, BasicDeliverEventArgs ea)
        {
            try
            {
                var body = ea.Body.ToArray();
                var jsonMessage = Encoding.UTF8.GetString(body);
                var obj = JsonConvert.DeserializeObject<DeletedDocumentCollection>(jsonMessage);
                if (obj != null)
                {
                    _documentCollection.Create(_settings.AppKey, obj);
                }
            }
            catch (Exception)
            {
                throw new InvalidOperationException(ResultCode.InvalidRabbitMqMessageEvent.GetNumericString());
            }
            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _rabbitMq.Consume(stoppingToken, _settings.RabbitMq.DocumentCollectionQueueName);
        }
    }
}
