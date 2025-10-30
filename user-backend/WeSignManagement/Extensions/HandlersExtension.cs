using AspNetCoreRateLimit;
using Certificate.Handlers;
using Certificate.Interfaces;
using Common.Handlers;
using Common.Handlers.FileGateScanner;
using Common.Handlers.Files;
using Common.Handlers.Files.Local;
using Common.Handlers.PDF;
using Common.Handlers.RabbitMQ;
using Common.Handlers.SendingMessages;
using Common.Handlers.SendingMessages.Mail;
using Common.Handlers.SendingMessages.SMS;
using Common.Hubs;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.Emails;
using Common.Interfaces.FileGateScanner;
using Common.Interfaces.Files;
using Common.Interfaces.ManagementApp;
using Common.Interfaces.MessageSending;
using Common.Interfaces.MessageSending.Mail;
using Common.Interfaces.MessageSending.Sms;
using Common.Interfaces.PDF;
using Common.Interfaces.RabbitMQ;
using Common.Interfaces.Reports;
using Common.Models.Settings;
using CTHashSigner;
using CTInterfaces;
using CTPdfSigner;
using DAL;
using DAL.Connectors;
using ManagementBL;
using ManagementBL.CleanDb;
using ManagementBL.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PdfHandler;
using PdfHandler.Interfaces;
using PdfHandler.Signing;
using Serilog;
using Serilog.Exceptions;
using SignatureServiceConnector;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Linq.Expressions;

namespace WeSignManagement.Extensions
{
    public static class HandlersExtension
    {
        public static void AddHandlers(this IServiceCollection services, GeneralSettings generalSettings)
        {
            // User
            services.AddTransient(s => s.GetService<IHttpContextAccessor>()?.HttpContext?.User);

            var configuration = new ConfigurationBuilder()
                                .AddJsonFile("appsettings.json")
                                .Build();
            // Logger
            services.AddSingleton<ILogger>(
                new LoggerConfiguration()
                .Enrich.WithExceptionDetails()
                .ReadFrom.Configuration(configuration)
                .CreateLogger());
            
            services.AddHttpClient<ISmsProviderHandler, SmsProviderHandler>();

            services.AddTransient<IManagementBL, ManagementBLHandler>();
            services.AddTransient<IPrograms, ProgramsHandler>();
            services.AddTransient<IAppConfigurations, AppConfigurationsHandler>();
            services.AddTransient<ILogConnector, LogsConnector>();
            services.AddTransient<ILogs, LogsHandler>();
            services.AddTransient<ICompanies, CompaniesHandler>();
            services.AddTransient<IJson, JsonHandler>();
            services.AddTransient<ISendingMessageHandler, SendingMessageHandler>();
            services.AddTransient<IJWT, JWTHandler>();
            services.AddTransient<IOneTimeTokens, OneTimeTokensHandler>();
            services.AddTransient<IEncryptor, EncryptorHandler>();
            services.AddTransient<ISymmetricEncryptor, SymmetricEncryptorHandler>();
            services.AddTransient<Common.Interfaces.IConfiguration, ConfigurationHandler>();
            services.AddTransient<ISmsProviderHandler, SmsProviderHandler>();
            // files

            services.AddTransient<IFileSystem, FileSystem>();
            services.AddTransient<IDocumentFileWrapper, LocalDocumentFileWrapperHandler>();
            services.AddTransient<IContactFileWrapper, LocalContactFileWrapperHandler>();
            services.AddTransient<IUserFileWrapper, LocalUserFileWrapperHandler>();
            services.AddTransient<ISignerFileWrapper, LocalSignerFileWrapperHandler>();
            services.AddTransient<IConfigurationFileWrapper, LocalConfigurationFileWrapperHandler>();
            services.AddTransient<IFilesWrapper, FilesWrapper>();
            

            services.AddTransient<IEmail, EmailMessageHandler>();
            services.AddTransient<IPBKDF2, PBKDF2Handler>();
            if (!generalSettings.UseMailKit)
            {
                services.AddTransient<IEmailProvider, SmtpProviderHandler>();
            }
            else
            {
                services.AddTransient<IEmailProvider, SmtpMailkitProviderHandler>();
            }
            services.AddTransient<IDataUriScheme, DataUriSchemeHandler>();
            services.AddTransient<IPdfPackage, PdfPackage>();
            services.AddTransient<IValidator, ValidatorHandler>();
            services.AddTransient<ITableFormatter, TableFormatterHandler>();
            services.AddTransient<IDebenuPdfLibrary, DebenuPDFLibrary>();
            services.AddTransient<IDocumentPdf, DocumentPdfHandler>();
            services.AddTransient<IShared, Shared>();
            services.AddTransient<IEmailTypeHandler, EmailTypeHandler>();
            services.AddTransient<IJobs, JobsHandler>();
            services.AddTransient<IMessageSender, EmailHandler>();
            services.AddTransient<IUserPeriodicReports, UserPeriodicReportsHandler>();
            services.AddTransient<IManagementPeriodicReports, ManagementPeriodicReportsHandler>();
            services.AddTransient<IUserPeriodicReportConnector, UserPeriodicReportConnector>();
            services.AddTransient<IManagementPeriodicReportConnector, ManagementPeriodicReportConnector>();
            services.AddTransient<IManagementPeriodicReportEmailConnector,  ManagementPeriodicReportEmailConnector>();
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
            services.AddTransient<ISignerTokenMappingConnector, SignerTokenMappingConnector>();
            services.AddTransient<IUserTokenConnector, UserTokenConnector>();
            services.AddTransient<IUserPasswordHistoryConnector, UserPasswordHistoryConnector>();
            services.AddTransient<IContactsGroupsConnector, ContactsGroupsConnector>();
            services.AddTransient<IActiveDirectoryConfigConnector, ActiveDirectoryConfigConnector>();
            services.AddTransient<IDater, DaterHandler>();
            services.AddTransient<IAppendices, AppendicesHandler>();
            services.AddTransient<ICertificate, CertificatesHandler>();
            services.AddTransient<IOTP, OtpHandler>();
            services.AddTransient<ILicense, LicenseHandler>();
            services.AddTransient<ILicenseDMZ, LicenseDMZHandler>();
            services.AddTransient<ILicenseWrapper, LicenseWrapperHandler>();
            services.AddTransient<IActiveDirectory, ActiveDirectoryHandler>();
            services.AddTransient<ISigningTypeHandler, SigningTypeHandler>();
            services.AddTransient<IGenerateLinkHandler, GenerateLinkHandler>();
            services.AddTransient<IDocumentCollectionOperations, DocumentCollectionOperantionsHandler>();
            services.AddTransient<IPeriodicReportFileConnector, PeriodicReportFileConnector>();
            services.AddTransient<IContactSignatures, ContactSignaturesHandler>();
            services.AddTransient<IDocumentCollectionOperationsNotifier, HttpDocumentCollectionOperationsNotifier>();

            if (generalSettings.Signer1RestConnector)
            {
                services.AddTransient<ISignConnector, RestSignConnector>();
            }
            else
            {
                services.AddTransient<ISignConnector, SignConnector>();
            }

            services.AddTransient<IActiveDirectoryGroupsConnector, ActiveDirectoryGroupsConnector>();
            services.AddTransient<ITemplatePdf, TemplatePdfHandler>();
            services.AddTransient<IPdfPackage, PdfPackage>();
            services.AddTransient<IPdfConverter, PdfConverter>();
            services.AddTransient<IExternalPDFConverter, ExternalCloudmersivePDFConverterHandler>();
            
            services.AddTransient<IPDFSign, PDFSigner>();
            services.AddTransient<IImage, PDFImage>();
            services.AddTransient<ISign, CSign>();
            services.AddTransient<ISignersConnector, SignersConnector>();
            services.AddTransient<IFileGateScannerProviderFactory, FileGateScannerProviderFactoryHandler>();
            services.AddTransient<ICertificateCreator, CertificateCreatorHandler>();

            services.AddTransient<ICleanDBManager, CleanDBManager>();
            services.AddTransient<ICleanDBFactory, CleanDBFactory>();

            services.AddTransient<IDocumentCollection, DocumentCollectionHandler>();

            services.AddTransient<IRabbitConnector, RabbitConnectorHandler>();
            services.AddTransient<IPayment, PaymentHandler>();
            services.AddTransient<IReport, ReportsHandler>();
            services.AddTransient<IUserReports, UserReportHandler>();
            services.AddTransient<IWeSignHistoryReports, WeSignHistoryReportsHandler>();
            services.AddTransient<IManagementHistoryReports, ManagementHistoryReportsHandler>();
            services.AddTransient<IHistoryDocumentCollection, HistoryDocumentCollectionHandler>();
            services.AddTransient<Common.Interfaces.ManagementApp.IUsers, UsersHandler>();

            // IpRateLimiting
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

            // caching
            //services.AddScoped<IAppCache, CachingService>();
            //// CoR pattern 
            //services.Chain<IDeleter>()
            //    .Add<DocumentsDeleter>()
            //    .Add<ContactsDeleter>()
            //    .Add<LogsDeleter>()
            //    .Add<GroupsDeleter>()
            //    .Add<UsersDeleter>()
            //    .Add<TokensDeleter>()
            //    .Add<CompaniesDeleter>()
            //    .Configure();
        }
    }
    public static class ChainConfigurator
    {
        public static IChainConfigurator<T> Chain<T>(this IServiceCollection services) where T : class
        {
            return new ChainConfiguratorImpl<T>(services);
        }
        public interface IChainConfigurator<T>
        {
            IChainConfigurator<T> Add<TImplementation>() where TImplementation : T;
            void Configure();
        }

        private class ChainConfiguratorImpl<T> : IChainConfigurator<T> where T : class
        {
            private readonly IServiceCollection _services;
            private List<Type> _types;
            private Type _interfaceType;

            public ChainConfiguratorImpl(IServiceCollection services)
            {
                _services = services;
                _types = new List<Type>();
                _interfaceType = typeof(T);
            }

            public IChainConfigurator<T> Add<TImplementation>() where TImplementation : T
            {
                var type = typeof(TImplementation);

                if (!_interfaceType.IsAssignableFrom(type))
                    throw new ArgumentException($"{type.Name} type is not an implementation of {_interfaceType.Name}", nameof(type));
                _types.Add(type);

                return this;
            }

            public void Configure()
            {
                if (_types.Count == 0)
                    throw new InvalidOperationException($"No implementation defined for {_interfaceType.Name}");

                bool first = true;
                foreach (var type in _types)
                {
                    ConfigureType(type, first);
                    first = false;
                }
            }

            private void ConfigureType(Type currentType, bool first)
            {
                var nextType = _types.SkipWhile(x => x != currentType).SkipWhile(x => x == currentType).FirstOrDefault();

                var ctor = currentType.GetConstructors().OrderByDescending(x => x.GetParameters().Count()).First();

                var parameter = Expression.Parameter(typeof(IServiceProvider), "x");

                var ctorParameters = ctor.GetParameters().Select(x =>
                {
                    if (_interfaceType.IsAssignableFrom(x.ParameterType))
                    {
                        if (nextType == null)
                            return Expression.Constant(null, _interfaceType);
                        else
                            return Expression.Call(typeof(ServiceProviderServiceExtensions), "GetRequiredService", new Type[] { nextType }, parameter);
                    }

                    return (Expression)Expression.Call(typeof(ServiceProviderServiceExtensions), "GetRequiredService", new Type[] { x.ParameterType }, parameter);
                });

                var body = Expression.New(ctor, ctorParameters.ToArray());

                var resolveType = first ? _interfaceType : currentType;

                var expressionType = Expression.GetFuncType(typeof(IServiceProvider), resolveType);
                var expression = Expression.Lambda(expressionType, body, parameter);
                var compiledExpr = (Func<IServiceProvider, object>)expression.Compile();
                _services.AddTransient(resolveType, compiledExpr);
            }
        }
    }
}
