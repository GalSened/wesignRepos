using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Files;
using Common.Interfaces.PDF;
using Common.Interfaces.RabbitMQ;
using Common.Models.Settings;
using ManagementBL.CleanDb.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Net.Http;

namespace ManagementBL.CleanDb
{
    public class CleanDBFactory : ICleanDBFactory
    {
        private IOptions<GeneralSettings> _generalSettings;
        private IOptions<RabbitMQSettings> _rabbitSettings;
        
        private readonly ILogger _logger;
        private readonly IDater _dater;
        private readonly IDocumentPdf _documentPdf;
        private readonly IConfiguration _configuration;        
        private readonly ICertificate _certificate;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ITemplatePdf _templatePdf;
        private readonly IFilesWrapper _filesWrapper;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IRabbitConnector _rabbitConnector;
        private readonly IEncryptor _encryptor;
        private readonly IDocumentCollectionOperationsNotifier _documentCollectionOperationsNotifier;

        public CleanDBFactory(IOptions<GeneralSettings> generalSettings, IOptions<RabbitMQSettings> rabbitSettings, ILogger logger, IDater dater, IDocumentPdf documentPdf,
             IConfiguration configuration, ICertificate certificate,
             IServiceScopeFactory scopeFactory, ITemplatePdf templatePdf, IFilesWrapper filesWrapper, IHttpClientFactory httpClientFactory, IRabbitConnector rabbitConnector,
             IEncryptor encryptor, IDocumentCollectionOperationsNotifier documentCollectionOperationsNotifier)
        {
            _generalSettings = generalSettings;
            _rabbitSettings = rabbitSettings;
            _logger = logger;
            _dater = dater;
            _documentPdf = documentPdf;
            _configuration = configuration;         
            _certificate = certificate;
            _scopeFactory = scopeFactory;
            _templatePdf = templatePdf;
            _filesWrapper = filesWrapper;
            _httpClientFactory = httpClientFactory;
            _rabbitConnector = rabbitConnector;
            _encryptor = encryptor;
            _documentCollectionOperationsNotifier = documentCollectionOperationsNotifier;
        }
        public IDeleter GetDeleter(Type type)
        {
            switch (type)
            {
                case Type docType when docType == typeof(DocumentsDeleter) :
                    {
                        return new DocumentsDeleter(_generalSettings, _rabbitSettings, _logger, _dater, _documentPdf, _configuration, _scopeFactory, _filesWrapper, _httpClientFactory, _rabbitConnector,
                            _encryptor, _documentCollectionOperationsNotifier);
                    }
                case Type contactType when contactType == typeof(ContactsDeleter):
                    {
                        return new ContactsDeleter( _logger, _certificate, _scopeFactory, _filesWrapper);
                    }

                case Type templateType when templateType == typeof(TemplatesDeleter):
                    {
                        return new TemplatesDeleter( _logger,  _templatePdf, _scopeFactory);
                    }              
                case Type logType when logType == typeof(UsersDeleter):
                    {
                        return new UsersDeleter( _logger, _certificate, _scopeFactory);
                    }
                case Type logType when logType == typeof(GroupsDeleter):
                    {
                        return new GroupsDeleter( _logger, _scopeFactory);
                    }
                case Type logType when logType == typeof(CompaniesDeleter):
                    {
                        return new CompaniesDeleter( _logger, _scopeFactory, _filesWrapper);
                    }
                default:
                    {
                        throw new Exception("type not exist");
                    }
            }
        }
    }
}
