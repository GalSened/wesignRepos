using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using HistoryIntegratorService.Common.Models.MsSql;
using HistoryIntegratorService.DAL.DAOs.Documents;
using HistoryIntegratorService.DAL.DAOs.Templates;
using HistoryIntegratorService.Common.Interfaces.Connectors;
using HistoryIntegratorService.Common.Models;

namespace HistoryIntegratorService.DAL.Connectors
{
    public class MsSqlConnector : DbContext , IMsSqlConnector
    {
        private readonly GeneralSettings _settings;

        //HardCoded ConnectionString  relevant only for migration operation
        public MsSqlConnector()
        {
            _settings = new GeneralSettings()
            {
                MsSql = new MsSqlSettings()
                {
                    ConnectionString = "Password=Wesign1!;Persist Security Info=True;User ID=comda;TrustServerCertificate=True;Initial Catalog=HistoryIntegratorService;Data Source=WESIGN3\\SQLEXPRESS"
                }
            };
        }

        public MsSqlConnector(IOptions<GeneralSettings> options)
        {
            _settings = options.Value;
        }

        public DbSet<DeletedDocumentCollectionDAO> DeletedDocumentCollections { get; set; }
        public DbSet<DeletedDocumentDAO> DeletedDocuments { get; set; }
        public DbSet<TemplateDAO> Templates { get; set; }
        public DbSet<TemplateSignatureFieldDAO> TemplateSignatureFields { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlServer(_settings.MsSql.ConnectionString,
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                }).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }
}
