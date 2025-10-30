using Common.Handlers;
using Common.Interfaces;
using Common.Interfaces.DB;
using DAL;
using DAL.Connectors;
using LazyCache;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using WSE_ADAuth.AD;
using WSE_ADAuth.Handler;
using WSE_ADAuth.SAML;

namespace WSE_ADAuth.Extensions
{
    public static class HandlersExtension
    {
        public static void AddHandlers(this IServiceCollection services, IWebHostEnvironment env)
        {

            services.AddTransient<IUserTokenConnector, UserTokenConnector>();
            services.AddTransient<IUserPasswordHistoryConnector, UserPasswordHistoryConnector>();
            services.AddTransient<IContactsGroupsConnector, ContactsGroupsConnector>();
            services.AddTransient<IUserConnector, UserConnector>();
            services.AddTransient<IDbConnector, DbHandler>();
            services.AddTransient<IJWT, JWTHandler>();
            services.AddTransient<IOneTimeTokens, OneTimeTokensHandler>();
            services.AddTransient<IAD, ADHandler>();
            services.AddTransient<IWeSignEntities, WeSignEntities>();
            services.AddTransient<IContactConnector, ContactsConnector>();
            services.AddTransient<IProgramConnector, ProgramConnector>();
            services.AddTransient<IDater, DaterHandler>();
            services.AddTransient<IConfigurationConnector, ConfigurationConnector>();
            services.AddTransient<IDocumentCollectionConnector, DocumentCollectionConnector>();
            services.AddTransient<IProgramUtilizationHistoryConnector, ProgramUtilizationHistoryConnector>();
            services.AddTransient<IDocumentConnector, DocumentConnector>();
            services.AddTransient<ITemplateConnector, TemplateConnector>();
            services.AddTransient<IProgramUtilizationConnector, ProgramUtilizationConnector>();
            services.AddTransient<ISignerTokenMappingConnector, SignerTokenMappingConnector>();
            services.AddTransient<IGroupConnector, GroupConnector>();
            services.AddTransient<ICompanyConnector, CompanyConnector>();
            services.AddTransient<ILogConnector, LogsConnector>();
            services.AddTransient<IActiveDirectoryConfigConnector, ActiveDirectoryConfigConnector>();
            services.AddTransient<IActiveDirectoryGroupsConnector, ActiveDirectoryGroupsConnector>();
            services.AddTransient<ISignersConnector, SignersConnector>();
            services.AddTransient<IEncryptor, EncryptorHandler>();
            services.AddTransient<ISymmetricEncryptor, SymmetricEncryptorHandler>();
            services.AddTransient<ISAMLResponse, SAMLResponseHandler>();
            services.AddTransient<ISAMLRequest, SAMLRequestHandler>();
            services.AddTransient<IUserPeriodicReportConnector, UserPeriodicReportConnector>();
            services.AddTransient<ILoginHandler, LoginHandler>();
            services.AddTransient<INewUserGenerator, NewUserGeneratorHandler>();
            services.AddTransient<IManagementPeriodicReportConnector, ManagementPeriodicReportConnector>();
            services.AddTransient<IManagementPeriodicReportEmailConnector, ManagementPeriodicReportEmailConnector>();

            var configuration = new ConfigurationBuilder()
                              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                              .AddJsonFile($"appsettings.{env.EnvironmentName.ToLower()}.json", optional: true, reloadOnChange: true)
                              .Build();

            services.AddSingleton<ILogger>(
              new LoggerConfiguration()
              .ReadFrom.Configuration(configuration)
              .CreateLogger());


            services.AddHttpClient();

            services.AddLazyCache();

        }
    }
}
