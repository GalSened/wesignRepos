namespace HistoryIntegratorService.Common.Models.Mongo
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string DocumentCollectionName { get; set; }
        public int ConnectionRetries { get; set; }
        public int DelayBetweenConnectionAttemptsMS { get; set; }
    }
}
