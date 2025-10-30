using HistoryIntegratorService.BL.Handlers;
using HistoryIntegratorService.Common.Interfaces;
using HistoryIntegratorService.Common.Interfaces.Connectors;
using HistoryIntegratorService.Common.Models;
using HistoryIntegratorService.DAL.Connectors;
using HistoryIntegratorService.DAL.Connectors.DocumentCollection;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using ILogger = Serilog.ILogger;

namespace HistoryIntegratorService.Extensions
{
    public static class HandlersExtension
    {
        public static void AddHandlers(this IServiceCollection services, IWebHostEnvironment env, GeneralSettings generalSettings)
        {
            var configuration = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                .AddJsonFile($"appsettings.{env.EnvironmentName.ToLower()}.json", optional: true, reloadOnChange: true)
                                .Build();

            // Logger
            services.AddSingleton<ILogger>(new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder())
                .ReadFrom.Configuration(configuration)
                .CreateLogger());

            // BL
            services.AddTransient<IDocumentCollection, DocumentCollectionHandler>();
            services.AddTransient<IManagementReports, ManagementReportsHandler>();
            services.AddTransient<IWeSignReports, WeSignReportsHandler>();
            services.AddTransient<IRabbitMQ, RabbitMqHandler>();
            services.AddTransient<IEncryptor, EncryptorHandler>();

            // DAL
            switch (generalSettings.ConnectorType)
            {
                case Common.Enums.ConnectorType.MsSql:
                    services.AddTransient<IMsSqlConnector, MsSqlConnector>();
                    services.AddTransient<IDocumentCollectionConnector, MsSqlDocumentCollectionConnector>();
                    services.AddTransient<IWeSignDocumentCollectionConnector, MsSqlDocumentCollectionConnector>();
                    services.AddTransient<IManagementDocumentCollectionConnector, MsSqlDocumentCollectionConnector>();
                    break;
                case Common.Enums.ConnectorType.MongoDB:
                    services.AddTransient<IMongoConnector, MongoConnector>();
                    services.AddTransient<IDocumentCollectionConnector, MongoDocumentCollectionConnector>();
                    services.AddTransient<IWeSignDocumentCollectionConnector, MongoDocumentCollectionConnector>();
                    services.AddTransient<IManagementDocumentCollectionConnector, MongoDocumentCollectionConnector>();
                    break;
                default:
                    services.AddTransient<IMsSqlConnector, MsSqlConnector>();
                    services.AddTransient<IDocumentCollectionConnector, MsSqlDocumentCollectionConnector>();
                    services.AddTransient<IWeSignDocumentCollectionConnector, MsSqlDocumentCollectionConnector>();
                    services.AddTransient<IManagementDocumentCollectionConnector, MsSqlDocumentCollectionConnector>();
                    break;
            }
        }
    }
}
