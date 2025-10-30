using Microsoft.EntityFrameworkCore;
using HistoryIntegratorService.DAL.DAOs.Documents;
using HistoryIntegratorService.DAL.DAOs.Templates;
/*
 * EF code first package manager console commands:
 * 1. Add-migration someMigration 
 * 2. Update-database
 * 3. Script-Migration -Output \\fs01\Production\WeSign\V3\DB_Script\HistoryServiceApi\CreateHistoryServiceApiTables.sql
 * 
 * For update version you can generate script from to migration (not include <PreviousMigration>)
 * 1.Script-Migration -From <PreviousMigration> -To <LastMigration> -Output \\fs01\Production\WeSign\V3\DB_Script\v3.0.7.1\HistoryServiceApi\upgrade-3.0.2to3.0.7.1\updateDb.sql
 * 
 * Script-Migration -From addSigner1AndReCapcheToConfigTable -Output \\fs01\Production\WeSign\V3\DB_Script\v3.0.7.1\HistoryServiceApi\upgrade-3.0.2to3.0.7.1\updateDb.sql
 * Script-Migration -Output \\fs01\Production\WeSign\V3\DB_Script\v3.0.7.1\HistoryServiceApi\CreateHistoryServiceApiTables.sql
 * 
 */
namespace HistoryIntegratorService.Common.Interfaces.Connectors
{
    public interface IMsSqlConnector
    {
        DbSet<DeletedDocumentCollectionDAO> DeletedDocumentCollections { get; set; }
        DbSet<DeletedDocumentDAO> DeletedDocuments { get; set; }
        DbSet<TemplateDAO> Templates { get; set; }
        DbSet<TemplateSignatureFieldDAO> TemplateSignatureFields { get; set; }
        int SaveChanges();
    }
}
