using Common.Handlers;
using Common.Handlers.FileGateScanner;
using Common.Handlers.SendingMessages;
using Common.Handlers.SendingMessages.Mail;
using Common.Handlers.SendingMessages.SMS;
using Common.Hubs;
using Common.Interfaces;
using AspNetCoreRateLimit;
using Common.Interfaces.DB;
using Common.Interfaces.Emails;
using Common.Interfaces.FileGateScanner;
using Common.Interfaces.MessageSending;
using Common.Interfaces.MessageSending.Mail;
using Common.Interfaces.MessageSending.Sms;
using Common.Interfaces.PDF;
using Common.Interfaces.SignerApp;
using CTHashSigner;
using CTInterfaces;
using CTPdfSigner;
using DAL;
using DAL.Connectors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PdfHandler;
using PdfHandler.Interfaces;
using PdfHandler.Signing;
using Serilog;
using SignatureServiceConnector;
using SignerBL.Handlers;
using SignerBL.Handlers.Actions;
using SignerBL.HandlersdocumentCollection;
using System.IO.Abstractions;
using WeSignSigner.ActionFilters;
using Serilog.Exceptions;
using Common.Interfaces.Oauth;
using Certificate.Interfaces;
using Certificate.Handlers;
using Common.Models.Settings;
using Common.Handlers.PDF;
using Common.Interfaces.RabbitMQ;
using SignerBL.Handlers.RabbitMQ;

using SignerBL.Hubs;
using Common.Handlers.RabbitMQ;
using Common.Handlers.RabbitMQ.SmatCard;
using Common.Handlers.Files;
using Common.Interfaces.Files;
using Common.Handlers.Files.Local;
using Ganss.Xss;
using Common.Extensions;

namespace WeSignSigner.Extensions
{
    public static class HandlersExtension
    {
        public static void AddHandlers(this IServiceCollection services, GeneralSettings generalSettings)
        {
            var configuration = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json")
                                .Build();
            // Logger
            services.AddSingleton<ILogger>(
                new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .ReadFrom.Configuration(configuration)
                .CreateLogger());

            services.AddHttpClient<ISmsProviderHandler, SmsProviderHandler>();

            services.AddTransient<IJson, JsonHandler>();
            services.AddTransient<IDocuments, DocumentsHandler>();
            services.AddTransient<IDoneDocuments, DocumentsHandler>();
            services.AddTransient<IJWT, JWTHandler>();
            services.AddTransient<IPdfPackage, PdfPackage>();
            services.AddTransient<ISendingMessageHandler, SendingMessageHandler>();
            services.AddTransient<ISmsProviderHandler, SmsProviderHandler>();
            services.AddTransient<IDocumentModeHandler, DocumentModeHandler>();
            services.AddTransient<IShared, Shared>();
            services.AddTransient<IEmailTypeHandler, EmailTypeHandler>();
            services.AddTransient<ISender, SenderHandler>();
            services.AddTransient<Common.Interfaces.SignerApp.ISignerValidator, SignerValidatorHandler>();
            services.AddTransient<ICertificate, CertificatesHandler>();
            services.AddTransient<Common.Interfaces.IConfiguration, ConfigurationHandler>();
            services.AddTransient<IDataUriScheme, DataUriSchemeHandler>();
            services.AddTransient<ISignerIdentity, SignerIdentityHandler>();
            services.AddTransient<IUserPeriodicReportConnector, UserPeriodicReportConnector>();
            services.AddTransient<IManagementPeriodicReportConnector, ManagementPeriodicReportConnector>();
            services.AddTransient<IManagementPeriodicReportEmailConnector, ManagementPeriodicReportEmailConnector>();

            services.AddTransient<IDocumentCollectionOperationsNotifier, HttpDocumentCollectionOperationsNotifier>();
            if (!generalSettings.UseMailKit)
            {
                services.AddTransient<IEmailProvider, SmtpProviderHandler>();
            }
            else
            {
                services.AddTransient<IEmailProvider, SmtpMailkitProviderHandler>();
            }
            services.AddTransient<IGenerateLinkHandler, GenerateLinkHandler>();
            services.AddTransient<IEncryptor, EncryptorHandler>();
            services.AddTransient<ISymmetricEncryptor, SymmetricEncryptorHandler>();
            services.AddTransient<ISingleLink, SingleLinkHandler>();
            services.AddTransient<ILogs, LogsHandler>();
            services.AddTransient<Common.Interfaces.SignerApp.IContacts, ContactsHandler>();
            services.AddTransient<IFileGateScannerProviderFactory, FileGateScannerProviderFactoryHandler> ();
            services.AddTransient<IOcrService, GoogleOcrHandler>();

            // Files
            services.AddTransient<IFileSystem, FileSystem>();
            services.AddTransient<IDocumentFileWrapper, LocalDocumentFileWrapperHandler>();
            services.AddTransient<IContactFileWrapper, LocalContactFileWrapperHandler>();
            services.AddTransient<IUserFileWrapper, LocalUserFileWrapperHandler>();
            services.AddTransient<ISignerFileWrapper, LocalSignerFileWrapperHandler>();
            services.AddTransient<IConfigurationFileWrapper, LocalConfigurationFileWrapperHandler>();
            services.AddTransient<IFilesWrapper, FilesWrapper>();

            services.AddTransient<ICertificateCreator, CertificateCreatorHandler>();
            services.AddTransient<IValidator, ValidatorHandler>();
            // DAL
            services.AddTransient<IWeSignEntities, WeSignEntities>();
            services.AddTransient<IDbConnector, DbHandler>();
            services.AddTransient<IContactConnector, ContactsConnector>();
            services.AddTransient<ICompanyConnector, CompanyConnector>();
            services.AddTransient<IUserConnector, UserConnector>();
            services.AddTransient<IGroupConnector, GroupConnector>();
            services.AddTransient<IProgramConnector, ProgramConnector>();
            services.AddTransient<IProgramUtilizationConnector, ProgramUtilizationConnector>();
            services.AddTransient<IProgramUtilizationHistoryConnector, ProgramUtilizationHistoryConnector>();
            services.AddTransient<IConfigurationConnector, ConfigurationConnector>();
            services.AddTransient<IDocumentCollectionConnector, DocumentCollectionConnector>();
            services.AddTransient<IDocumentConnector, DocumentConnector>();
            services.AddTransient<ITemplateConnector, TemplateConnector>();
            services.AddTransient<IActiveDirectoryConfigConnector, ActiveDirectoryConfigConnector>();
            services.AddTransient<IUserPeriodicReportConnector, UserPeriodicReportConnector>();
            

            services.AddTransient<IActiveDirectoryGroupsConnector, ActiveDirectoryGroupsConnector>();
            services.AddTransient<ISignersConnector, SignersConnector>();            


            services.AddTransient<ISignerTokenMappingConnector, SignerTokenMappingConnector>();
            services.AddTransient<IUserTokenConnector, UserTokenConnector>();
            services.AddTransient<IUserPasswordHistoryConnector, UserPasswordHistoryConnector>();
            services.AddTransient<IContactsGroupsConnector, ContactsGroupsConnector>();
            services.AddTransient<ILogConnector, LogsConnector>();
            services.AddTransient<IDater, DaterHandler>();
            services.AddTransient<IAppendices, AppendicesHandler>();
            services.AddTransient<IOTP, OtpHandler>();
            services.AddTransient<IContactSignatures, ContactSignaturesHandler>();

            // PDF
            services.AddTransient<ITemplatePdf, TemplatePdfHandler>();
            services.AddTransient<IDocumentPdf, DocumentPdfHandler>();
            services.AddTransient<IDebenuPdfLibrary, DebenuPDFLibrary>();
            
            services.AddTransient<ISigningTypeHandler, SigningTypeHandler>();
            if (generalSettings.Signer1RestConnector)
            {
                services.AddTransient<ISignConnector, RestSignConnector>();
            }
            else
            {
                services.AddTransient<ISignConnector, SignConnector>();
            }

            services.AddTransient<IPdfPackage, PdfPackage>();
            services.AddTransient<IPdfConverter, PdfConverter>();
            services.AddTransient<IExternalPDFConverter, ExternalCloudmersivePDFConverterHandler>();
            services.AddTransient<IPDFSign, PDFSigner>();
            services.AddTransient<IImage, PDFImage>();
            services.AddTransient<ISign, CSign>();
            services.AddTransient<IDoneActionsHelper, DoneActionsHelper>();


            services.AddTransient<IVisualIdentity, VisualIdentityHandler>();
            services.AddTransient<IHtmlSanitizer, HtmlSanitizer>();

            services.AddScoped<SingleLinkValidation>();
            services.AddTransient<IOauth, ComsignOauthHandler>(); 
            // caching
            services.AddMemoryCache();

            // IpRateLimiting
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();


            

            services.AddSingleton<IMessageMQLiveConnector, MessageQLiveConnectorHandler>();
            services.AddTransient<IRabbitConnector, RabbitConnectorHandler>();
            services.AddSingleton<IMessageQSmartCardConnector, MessageQSmartCardConnectorHandler>();
            services.AddTransient<ISmartCardSigningProcess, SmartCardSigningProcessHandler>();
            services.AddTransient<ISmartCardConsumedProcessFactory, SmartCardConsumedProcessFactory>();
            services.AddSingleton<IMessageMQAgentConnector, MessageAgentConnectorHandler>();
            services.AddSingleton<IMessageMQSignerIdentityConnector, MessageMQSignerIdentityConnectorHandler>();
        
            //HttpClient
            services.AddHttpClient();

        }
    }
}
