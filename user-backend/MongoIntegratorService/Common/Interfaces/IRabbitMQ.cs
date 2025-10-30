using RabbitMQ.Client.Events;

namespace HistoryIntegratorService.Common.Interfaces
{
    public interface IRabbitMQ
    {
        public EventHandler<BasicDeliverEventArgs> OnMessage { get; set; }
        void Consume(CancellationToken cancellationToken, string channelName);
    }
}
