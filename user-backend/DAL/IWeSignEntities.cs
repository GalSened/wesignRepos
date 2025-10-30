/*
 * EF code first package manager console commands:
 * 1. Add-migration someMigration 
 * 2. Update-database
 * 3. Script-Migration -Output \\fs01\Production\WeSign\V3\DB_Script\CreateWeSignTables.sql
 * 
 * For update version you can generate script from to migration (not include <PreviousMigration>)
 * 1.Script-Migration -From <PreviousMigration> -To <LastMigration> -Output \\fs01\Production\WeSign\V3\DB_Script\v3.0.7.1\upgrade-3.0.2to3.0.7.1\updateDb.sql
 * 
 * Script-Migration -From addSigner1AndReCapcheToConfigTable -Output \\fs01\Production\WeSign\V3\DB_Script\v3.0.7.1\upgrade-3.0.2to3.0.7.1\updateDb.sql
 * Script-Migration -Output \\fs01\Production\WeSign\V3\DB_Script\v3.0.7.1\CreateWeSignTables.sql
 * 
 */
namespace DAL
{
    using DAL.DAOs.Logs;
    using DAL.DAOs.ActiveDirectory;
    using DAL.DAOs.Companies;
    using DAL.DAOs.Configurations;
    using DAL.DAOs.Contacts;
    using DAL.DAOs.Documents;
    using DAL.DAOs.Documents.Signers;
    using DAL.DAOs.Groups;
    using DAL.DAOs.Programs;
    using DAL.DAOs.Templates;
    using DAL.DAOs.Users;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using System.Threading;
    using System.Threading.Tasks;
    using DAL.DAOs.Management;
    using DAL.DAOs.Reports;
    using Common.Models.Users;

    public interface IWeSignEntities
    {

        //ChangeTracker ChangeTracker { get; }

        DbSet<UserDAO> Users { get; set; }
        DbSet<ContactDAO> Contacts { get; set; }
        DbSet<ContactSealsDAO> ContactSeals { get; set; }
        DbSet<UserPasswordHistoryDAO> UserPasswordHistory { get; set; }
        DbSet<UserConfigurationDAO> UserConfiguration { get; set; }
        DbSet<CompanyConfigurationDAO> CompanyConfiguration { get; set; }
        DbSet<CompanyDAO> Companies { get; set; }
        DbSet<GroupDAO> Groups { get; set; }
        DbSet<ProgramDAO> Programs { get; set; }
        DbSet<ProgramUtilizationDAO> ProgramUtilization { get; set; }
        DbSet<ConfigurationDAO> Configuration { get; set; }
        DbSet<TemplateDAO> Templates { get; set; }
        DbSet<SingleLinkAdditionalResourceDAO> SingleLinkAdditionalResources { get; set; }
        DbSet<TemplateSignatureFieldDAO> TemplatesSignatureFields { get; set; }
        DbSet<TemplateTextFieldDAO> TemplatesTextFields { get; set; }
        DbSet<DocumentCollectionDAO> DocumentCollections { get; set; }
        DbSet<DocumentDAO> Documents { get; set; }
        DbSet<DocumentSignatureFieldDAO> DocumentsSignatureFields { get; set; }
        DbSet<SignerDAO> Signers { get; set; }
        DbSet<SignerTokenMappingDAO> SignerTokensMapping { get; set; }
        DbSet<UserTokensDAO> UserTokens { get; set; }
        DbSet<LogDAO> Logs { get; set; }
        DbSet<SignerLogDAO> SignerLogs { get; set; }
        DbSet<ManagementLogDAO> ManagementLogs { get; set; }
        DbSet<ActiveDirectoryConfigDAO> ActiveDirectoryConfigs { get; set; }
        DbSet<ActiveDirectoryGroupDAO> ActiveDirectoryGroups { get; set; }
        DbSet<AdditionalGroupMapperDAO> AdditionalGroupsMapper { get; set; }
        DbSet<TabletDAO> Tablets { get; set; }
        DbSet<CompanySigner1DetailDAO> CompanySigner1Details { get; set; }
        DbSet<ProgramUtilizationHistoryDAO> ProgramUtilizationHistories { get; set; }
        DbSet<ContactGroupMemberDAO> ContactGroupMembers { get; set; }
        DbSet<ContactsGroupDAO> ContactsGroup { get; set; }
        DbSet<UserPeriodicReportDAO> UserPeriodicReports { get; set; }
        DbSet<ManagementPeriodicReportDAO> ManagementPeriodicReports { get; set; }
        DbSet<ManagementPeriodicReportEmailDAO> ManagementPeriodicReportEmails { get; set; }
        DbSet<PeriodicReportFileDAO> PeriodicReportFiles { get; set;}
        DbSet<UserOtpDetailsDAO> UserOtpDetails { get; set; }
        DatabaseFacade Database { get; }

        //EntityEntry Remove(object entity);
        //EntityEntry Attach(object entity);
        //void AttachRange(IEnumerable<object> entities);

        int SaveChanges();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
