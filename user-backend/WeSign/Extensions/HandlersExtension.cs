using AspNetCoreRateLimit;
using BL.Handlers;
using BL.Handlers.FilesHandler;
using Common.Handlers;
using Common.Handlers.FileGateScanner;
using Common.Handlers.SendingMessages;
using Common.Handlers.SendingMessages.Mail;
using Common.Handlers.SendingMessages.SMS;
using Common.Hubs;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Emails;
using Common.Interfaces.FileGateScanner;
using Common.Interfaces.MessageSending;
using Common.Interfaces.MessageSending.Mail;
using Common.Interfaces.MessageSending.Sms;
using Common.Interfaces.PDF;
using Common.Interfaces.UserApp;
using CsvHelper;
using CTHashSigner;
using CTInterfaces;
using CTPdfSigner;
using DAL;
using DAL.Connectors;
using Ganss.Xss;
using LazyCache;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PdfHandler;
using PdfHandler.Interfaces;
using PdfHandler.Signing;
using Serilog;
using SignatureServiceConnector;
using System.IO.Abstractions;
using WeSign.Validators;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.MsSqlServer.Destructurers;
using Certificate.Interfaces;
using Certificate.Handlers;
using Common.Models.Settings;
using Common.Handlers.PDF;
using Common.Handlers.RabbitMQ;
using Common.Interfaces.RabbitMQ;
using Common.Handlers.RabbitMQ.SmatCard;
using Common.Handlers.Files;
using Common.Interfaces.Files;
using Common.Handlers.Files.Local;
using Common.Interfaces.Oauth;
using Common.Interfaces.Reports;
using Common.Interfaces.Dashboard;
using Common.Extensions;

namespace WeSign.Extensions
{
    public static class HandlersExtension
    {
        public static void AddHandlers(this IServiceCollection services, IWebHostEnvironment env, GeneralSettings generalSettings)
        {
            // User
            services.AddTransient(s => s.GetService<IHttpContextAccessor>().HttpContext.User);

            var configuration = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                .AddJsonFile($"appsettings.{env.EnvironmentName.ToLower()}.json", optional: true, reloadOnChange: true)
                                .Build();
            // Logger
            services.AddSingleton<ILogger>(
                new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
                    .WithDefaultDestructurers()
                    .WithDestructurers(new[] { new SqlExceptionDestructurer() }))
                .ReadFrom.Configuration(configuration)
                .CreateLogger());

            services.AddHttpClient<ISmsProviderHandler, SmsProviderHandler>();

            // BL
            services.AddTransient<IBL, BLHandler>();
            services.AddTransient<IUsers, UsersHandler>();
            services.AddTransient<IAdmins, AdminsHandlers>();
            services.AddTransient<IContacts, ContactsHandler>();
            services.AddTransient<IDocumentCollections, DocumentCollectionsHandler>();
            services.AddTransient<ISigners, SignersHandler>();
            services.AddTransient<IEmail, EmailMessageHandler>();
            services.AddTransient<IDashboard, DashboardHandler>();
            services.AddTransient<IOcrService, GoogleOcrHandler>();
            if (!generalSettings.UseMailKit)
            {
                services.AddTransient<IEmailProvider, SmtpProviderHandler>();
            }
            else
            {
                services.AddTransient<IEmailProvider, SmtpMailkitProviderHandler>();
            }

            services.AddTransient<ISendingMessageHandler, SendingMessageHandler>();
            services.AddTransient<ISmsProviderHandler, SmsProviderHandler>();
            services.AddTransient<IFileGateScannerProviderFactory, FileGateScannerProviderFactoryHandler>();
            services.AddTransient<IEmailTypeHandlerFactory, UserPeriodicReportHandlerFactory>();
            services.AddTransient<IPBKDF2, PBKDF2Handler>();
            services.AddTransient<IEncryptor, EncryptorHandler>();
            services.AddTransient<ISymmetricEncryptor, SymmetricEncryptorHandler>();
            services.AddTransient<IJWT, JWTHandler>();
            services.AddTransient<IOneTimeTokens, OneTimeTokensHandler>();
            services.AddTransient<ISelfSign, SelfSignHandler>();
            services.AddTransient<ICertificate, CertificatesHandler>();
            services.AddTransient<IValidator, ValidatorHandler>();            
            services.AddTransient<IJson, JsonHandler>();
            services.AddTransient<ITemplates, TemplatesHandler>();
            services.AddTransient<Common.Interfaces.IConfiguration, ConfigurationHandler>();
            services.AddTransient<IDataUriScheme, DataUriSchemeHandler>();
            services.AddTransient<IEmailTypeHandler, EmailTypeHandler>();
            services.AddTransient<IShared, Shared>();
            services.AddTransient<IGenerateLinkHandler, GenerateLinkHandler>();
            services.AddSingleton<ILicense, LicenseHandler>();
            services.AddSingleton<ILicenseWrapper, LicenseWrapperHandler>();
            services.AddTransient(typeof(IXmlHandler<>), typeof(XMLHandler<>));
            services.AddTransient(typeof(ICsvHandler<>), typeof(CsvHandler<>));
            services.AddTransient<IActiveDirectoryConfigConnector, ActiveDirectoryConfigConnector>();
            services.AddTransient<IDocumentPdf, DocumentPdfHandler>();
            services.AddTransient<ILMSWrapperConnectorService, LMSWrapperConnectorServiceHandler>();

            services.AddTransient<ICertificateCreator, CertificateCreatorHandler>();
            services.AddTransient<IDocumentCollectionOperations, DocumentCollectionOperantionsHandler>();
            services.AddTransient<IDocumentCollectionOperationsNotifier, HttpDocumentCollectionOperationsNotifier>();

            services.AddTransient<IContactSignatures, ContactSignaturesHandler>();
            // Files
            services.AddTransient<IFileSystem, FileSystem>();
            services.AddTransient<IDocumentFileWrapper, LocalDocumentFileWrapperHandler>();
            services.AddTransient<IContactFileWrapper, LocalContactFileWrapperHandler>();
            services.AddTransient<IUserFileWrapper, LocalUserFileWrapperHandler>();
            services.AddTransient<ISignerFileWrapper, LocalSignerFileWrapperHandler>();
            services.AddTransient<IConfigurationFileWrapper, LocalConfigurationFileWrapperHandler>();
            services.AddTransient<IFilesWrapper, FilesWrapper>();

            // DAL
            services.AddTransient<IWeSignEntities, WeSignEntities>();
            services.AddTransient<IDbConnector, DbHandler>();
            services.AddTransient<IContactConnector, ContactsConnector>();
            services.AddTransient<ICompanyConnector, CompanyConnector>();
            services.AddTransient<IUserConnector, UserConnector>();
            services.AddTransient<IGroupConnector, GroupConnector>();
            services.AddTransient<ILogConnector, LogsConnector>();
            services.AddTransient<IProgramConnector, ProgramConnector>();
            services.AddTransient<IProgramUtilizationConnector, ProgramUtilizationConnector>();
            services.AddTransient<IProgramUtilizationHistoryConnector, ProgramUtilizationHistoryConnector>();
            services.AddTransient<IConfigurationConnector, ConfigurationConnector>();
            services.AddTransient<IDocumentCollectionConnector, DocumentCollectionConnector>();
            services.AddTransient<IDocumentConnector, DocumentConnector>();
            services.AddTransient<ITemplateConnector, TemplateConnector>();
            services.AddTransient<ISignerTokenMappingConnector, SignerTokenMappingConnector>();
            services.AddTransient<IActiveDirectoryConfigConnector, ActiveDirectoryConfigConnector>();
            services.AddTransient<IActiveDirectoryGroupsConnector, ActiveDirectoryGroupsConnector>();
            services.AddTransient<IUserTokenConnector, UserTokenConnector>();
            services.AddTransient<IUserPasswordHistoryConnector, UserPasswordHistoryConnector>();
            services.AddTransient<IContactsGroupsConnector, ContactsGroupsConnector>();
            services.AddTransient<IUserPeriodicReportConnector, UserPeriodicReportConnector>();
            services.AddTransient<IManagementPeriodicReportConnector, ManagementPeriodicReportConnector>();
            services.AddTransient<ISignersConnector, SignersConnector>();
            services.AddTransient<IExternalPDFService, ExternalPDFServiceHandler>();
            services.AddTransient<IDater, DaterHandler>();
            services.AddTransient<IAppendices, AppendicesHandler>();
            services.AddTransient<IDoneDocuments, DoneDocumentsHandler>();
            services.AddTransient<ILinks, LinksHandler>();
            services.AddTransient<IVideoConfrence, VideoConfrenceHandler > ();
            services.AddTransient<IReports, ReportsHandler>();
            services.AddTransient<IWeSignHistoryReports, WeSignHistoryReportsHandler>();
            services.AddTransient<IManagementPeriodicReportEmailConnector, ManagementPeriodicReportEmailConnector>();
            services.AddTransient<IPeriodicReportFileConnector, PeriodicReportFileConnector>();
            services.AddTransient<IDashboardConnector, DashboardConnector>();

            // PDF
            services.AddTransient<ITemplatePdf, TemplatePdfHandler>();
            services.AddScoped<IDocumentPdf, DocumentPdfHandler>();
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
            services.AddTransient<IDistribution, DistributionHandler>();
            services.AddTransient<IDoneActionsHelper, DoneActionsHelper>();
            services.AddTransient<IOauth, ComsignOauthHandler>();
            // IpRateLimiting
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();


            //Hangfire
            services.AddTransient<IUserHangfire, UserHangfireHandler>();

            //sanitetion 
            services.AddTransient<IHtmlSanitizer, HtmlSanitizer>();

            services.AddTransient<IRabbitConnector, RabbitConnectorHandler>();
            services.AddSingleton<IMessageQSmartCardConnector, MessageQSmartCardConnectorHandler>();
            services.AddTransient<ISmartCardSigningProcess, SmartCardSigningProcessHandler>();
            services.AddTransient<ISmartCardConsumedProcessFactory,SmartCardConsumedProcessFactory>();
            //HttpClient
            services.AddHttpClient();

            // caching
            //services.AddLazyCache();
            services.AddMemoryCache();
           


        



        }
    }
}
