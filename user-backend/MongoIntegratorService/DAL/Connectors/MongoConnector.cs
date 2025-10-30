using Microsoft.Extensions.Options;
using MongoDB.Driver;
using HistoryIntegratorService.Common.Enums;
using HistoryIntegratorService.Common.Extensions;
using HistoryIntegratorService.Common.Interfaces.Connectors;
using HistoryIntegratorService.Common.Models.Mongo;
using ILogger = Serilog.ILogger;
using HistoryIntegratorService.Common.Models;

namespace HistoryIntegratorService.DAL.Connectors
{
    public class MongoConnector : IMongoConnector
    {
        private MongoClient? _client;
        private ILogger _logger;
        private GeneralSettings _settings;
        private IMongoDatabase? _database;
        private readonly int _defaultConnectionRetries;

        public MongoConnector(ILogger logger, IOptions<GeneralSettings> options)
        {
            _logger = logger;
            _settings = options.Value;
            _defaultConnectionRetries = _settings.MongoDb.ConnectionRetries;
            InitConnection();
        }

        public IMongoCollection<T>? GetCollection<T>(string collectionName)
        {
            return _database?.GetCollection<T>(collectionName);
        }

        #region PRIVATE
        private void InitConnection()
        {
            if (string.IsNullOrWhiteSpace(_settings.MongoDb.ConnectionString))
            {
                throw new InvalidOperationException(ResultCode.InvalidMongoDbConnectionString.GetNumericString());
            }
            if (string.IsNullOrWhiteSpace(_settings.MongoDb.DatabaseName))
            {
                throw new InvalidOperationException(ResultCode.InvalidDatabaseName.GetNumericString());
            }
            try
            {
                while (_client == null && _settings.MongoDb.ConnectionRetries > 0)
                {
                    _client = TryInitMongoClient();
                    Thread.Sleep(_settings.MongoDb.DelayBetweenConnectionAttemptsMS);
                }
                if (_client == null)
                {
                    _logger.Error("Max attempts to connect to mongo reached");
                    throw new InvalidOperationException(ResultCode.MaximumMongoDbConnectionRetriesAttempted.GetNumericString());
                }
                _database = _client.GetDatabase(_settings.MongoDb.DatabaseName);
                _settings.MongoDb.ConnectionRetries = _defaultConnectionRetries;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while trying to connect MongoDB");
                _settings.MongoDb.ConnectionRetries--;
            }
        }

        private MongoClient? TryInitMongoClient()
        {
            try
            {
                var client = new MongoClient(_settings.MongoDb.ConnectionString);
                return client;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "MongoDB attempt to connect failed");
                return null;
            }
        }

        #endregion
    }
}
