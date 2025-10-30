using MongoDB.Driver;

namespace HistoryIntegratorService.Common.Interfaces.Connectors
{
    public interface IMongoConnector
    {
        IMongoCollection<T>? GetCollection<T>(string collectionName);
    }
}
