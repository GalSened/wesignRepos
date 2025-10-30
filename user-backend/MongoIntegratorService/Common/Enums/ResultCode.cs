using System.ComponentModel;

namespace HistoryIntegratorService.Common.Enums
{
    public enum ResultCode
    {
        [Description("Invalid RabbitMQ message event")]
        InvalidRabbitMqMessageEvent = 1,
        [Description("Invalid RabbitMQ hostname")]
        InvalidRabbitMqHostname = 2,
        [Description("Invalid RabbitMQ username or password")]
        InvalidRabbitMqUsernamePassword = 3,
        [Description("RabbitMQ connection close")]
        RabbitMqConnectionClose = 4,
        [Description("MongoDB connection close")]
        MongoDbConnectionClose = 5,
        [Description("Invalid MongoDB connection string")]
        InvalidMongoDbConnectionString = 6,
        [Description("Invalid database name")]
        InvalidDatabaseName = 7,
        [Description("Maximum MongoDB connection retries attempted")]
        MaximumMongoDbConnectionRetriesAttempted = 8,
        [Description("Maximum RabbitMQ connection retries attempted")]
        MaximumRabbitMqConnectionRetriesAttempted = 9,
        [Description("Invalid RabbitMQ port")]
        InvalidRabbitMqPort = 10,
        [Description("Invalid positive offfset number")]
        InvalidPositiveOffsetNumber = 11,
        [Description("Invalid limit number")]
        InvalidLimitNumber = 12,
        [Description("Invalid rabbit consumed message")]
        InvalidRabbitConsumedMesssge = 13,
        [Description("Invalid collection name")]
        InvalidCollectionName = 14,
        [Description("Invalid refresh token")]
        InvalidRefreshToken = 15,
        [Description("Invalid credential")]
        InvalidCredential = 16
    }
}
