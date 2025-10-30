using HistoryIntegratorService.Common.Enums;
using HistoryIntegratorService.Common.Models.Mongo;
using HistoryIntegratorService.Common.Models.MsSql;
using HistoryIntegratorService.Common.Models.RabbitMQ;

namespace HistoryIntegratorService.Common.Models
{
    public class GeneralSettings
    {
        public ConnectorType ConnectorType { get; set; }
        public RabbitMqSettings RabbitMq { get; set; }
        public MongoDbSettings MongoDb { get; set; }
        public MsSqlSettings MsSql { get; set; }
        public string UserBackendRoute { get; set; }
        public string ManagementBackendRoute { get; set; }
        public bool UseRabbitMQ { get; set; }
        public string AppKey { get; set; }
    }
}
