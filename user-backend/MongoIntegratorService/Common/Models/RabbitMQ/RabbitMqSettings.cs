namespace HistoryIntegratorService.Common.Models.RabbitMQ
{
    public class RabbitMqSettings
    {
        public string HostName { get; set; }
        public int? Port { get; set; }
        public string Uri { get; set; }
        public bool SslEnabled { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DocumentCollectionQueueName { get; set; }
        public int ConnectionRetries { get; set; }
        public int DelayBetweenConnectionAttemptsMS { get; set; }
    }
}
