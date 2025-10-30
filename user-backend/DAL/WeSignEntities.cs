namespace DAL
{
    using Common.Enums.Users;
    using Common.Interfaces;
    using Common.Models.Settings;
    using DAL.DAOs.Logs;
    using DAL.DAOs.ActiveDirectory;
    using DAL.DAOs.Companies;
    using DAL.DAOs.Configurations;
    using DAL.DAOs.Documents;
    using DAL.DAOs.Documents.Signers;
    using DAL.DAOs.Groups;
    using DAL.DAOs.Programs;
    using DAL.DAOs.Templates;
    using DAOs.Contacts;
    using DAOs.Users;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using DAL.DAOs.Management;
    using DAL.DAOs.Reports;

    public class WeSignEntities : DbContext, IWeSignEntities
    {
        private readonly GeneralSettings _generalSettings;
        private readonly IDater _dater;

        public WeSignEntities()
        {
            _generalSettings = new GeneralSettings()
            {
                ConnectionString = "Password=Wesign1!;Persist Security Info=True;User ID=comda;TrustServerCertificate=True;Initial Catalog=WeSign;Data Source=WESIGN3\\SQLEXPRESS"
                //ConnectionString = "Password=Aa123456;Persist Security Info=True;User ID=sa;Initial Catalog=WeSign;Data Source=DEVTEST\\SQLEXPRESS",

                // testing ConnectionString= "Password=Aa123456;Persist Security Info=True;User ID=sa;Initial Catalog=WeSign;Data Source=DevTest\\sqlExpress"
            };
        }

        //HardCoded connectionString  relevant only for migration operation
        public WeSignEntities(IDater dater)
        {
            _generalSettings = new GeneralSettings()
            {
                ConnectionString = "Password=Wesign1!;Persist Security Info=True;User ID=comda;TrustServerCertificate=True;Initial Catalog=WeSign;Data Source=WESIGN3\\SQLEXPRESS"
                //ConnectionString = "Password=Aa123456;Persist Security Info=True;User ID=sa;Initial Catalog=WeSign;Data Source=DEVTEST\\SQLEXPRESS",
                // testing ConnectionString = "Password=Aa123456;Persist Security Info=True;User ID=sa;Initial Catalog=WeSign;Data Source=DevTest\\sqlExpress"
            };
            _dater = dater;
        }

        public WeSignEntities(IOptions<GeneralSettings> generalSettings, IDater dater)
        {
            _generalSettings = generalSettings.Value;
            _dater = dater;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlServer(_generalSettings.ConnectionString,
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null

                        );

                }).
                UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);


        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            InitDbData(modelBuilder);
            OnDocumentCollectionModelCreating(modelBuilder);
            OnDocumentModelCreating(modelBuilder);
            OnUserModelCreating(modelBuilder);
            OnGroupModelCreating(modelBuilder);
            OnCompanyModelCreating(modelBuilder);
            OnSignerModelCreating(modelBuilder);
            OnTemplateModelCreating(modelBuilder);
            OnContactModelCreating(modelBuilder);
            OnActiveDirectoryModelCreating(modelBuilder);
            OnProgramModelCreating(modelBuilder);
        }

        #region Private

        private void InitDbData(ModelBuilder modelBuilder)
        {
            var initConfiguration = new List<ConfigurationDAO>()
            {
                new ConfigurationDAO() { Key = "SmtpServer", Value = "" },
                new ConfigurationDAO() { Key = "SmtpPort", Value = "25" },
                new ConfigurationDAO() { Key = "SmtpUser", Value = "" },
                new ConfigurationDAO() { Key = "SmtpPassword", Value = "" },
                new ConfigurationDAO() { Key = "SmtpFrom", Value = "" },
                new ConfigurationDAO() { Key = "SmtpEnableSsl", Value = "False" },
                new ConfigurationDAO() { Key = "SmtpAttachmentMaxSize", Value = "8388608" },
                new ConfigurationDAO() { Key = "SmsUser", Value = "" },
                new ConfigurationDAO() { Key = "SmsPassword", Value = "" },
                new ConfigurationDAO() { Key = "SmsFrom", Value = "" },
                new ConfigurationDAO() { Key = "SmsProvider", Value = "1" },
                new ConfigurationDAO() { Key = "SmsLanguage", Value = "1" },
                new ConfigurationDAO() { Key = "DeleteSignedDocumentAfterXDays", Value = "14" },
                new ConfigurationDAO() { Key = "DeleteUnsignedDocumentAfterXDays", Value = "30" },
                new ConfigurationDAO() { Key = "MessageBefore", Value = "[DOCUMENT_NAME] : [LINK]" },
                new ConfigurationDAO() { Key = "MessageBeforeHebrew", Value = "[DOCUMENT_NAME] : [LINK]" },
                new ConfigurationDAO() { Key = "MessageAfter", Value = "[DOCUMENT_NAME] signed successfully. [LINK]" },
                new ConfigurationDAO() { Key = "MessageAfterHebrew", Value = "[DOCUMENT_NAME] נחתם בהצלחה. [LINK]" },
                new ConfigurationDAO() { Key = "LogArichveIntervalInDays", Value = "30" },
                new ConfigurationDAO() { Key = "UseManagementOtpAuth", Value = "false" },
                new ConfigurationDAO() { Key = "EnableFreeTrailUsers", Value = "false" },
                new ConfigurationDAO() { Key = "EnableShowSSOOnlyInUserUI", Value = "false" },                
                new ConfigurationDAO() { Key = "EnableTabletsSupport", Value = "false" },
                new ConfigurationDAO() { Key = "EnableSigner1ExtraSigningTypes", Value = "false" },
                new ConfigurationDAO() { Key = "ShouldUseReCaptchaInRegistration", Value = "false" },
                new ConfigurationDAO() { Key = "Signer1Endpoint", Value = "" },
                new ConfigurationDAO() { Key = "Signer1User", Value = "" },
                new ConfigurationDAO() { Key = "Signer1Password", Value = "" },
                new ConfigurationDAO() { Key = "ShouldUseSignerAuth", Value = "false" },
                new ConfigurationDAO() { Key = "ShouldUseSignerAuthDefault", Value = "false" },
                new ConfigurationDAO() { Key = "SendWithOTPByDefault", Value = "false" },
                new ConfigurationDAO() { Key = "EnableVisualIdentityFlow", Value = "false" },
                new ConfigurationDAO() { Key = "EnableRenewalPayingUserLogic", Value = "false" },
                new ConfigurationDAO() { Key = "VisualIdentityURL", Value = "" },
                new ConfigurationDAO() { Key = "VisualIdentityUser", Value = "" },
                new ConfigurationDAO() { Key = "VisualIdentityPassword", Value = "" },
                new ConfigurationDAO() { Key = "ExternalPdfServiceURL", Value = "" },
                new ConfigurationDAO() { Key = "ExternalPdfServiceAPIKey", Value = "" },
                new ConfigurationDAO() { Key = "HistoryIntegratorServiceURL", Value = "" },
                new ConfigurationDAO() { Key = "HistoryIntegratorServiceAPIKey", Value = "" },
                new ConfigurationDAO() { Key = "UseExternalGraphicSignature", Value = "false" },
                new ConfigurationDAO() { Key = "ExternalGraphicSignatureSigner1Url", Value = "" },
                new ConfigurationDAO() { Key = "ExternalGraphicSignatureCert", Value = "" },
                new ConfigurationDAO() { Key = "ExternalGraphicSignaturePin", Value = "" },
                new ConfigurationDAO() { Key = "ShouldReturnActivationLinkInAPIResponse", Value = "false" },
                new ConfigurationDAO() { Key = "RecentPasswordsAmount", Value = "3" },
                new ConfigurationDAO() { Key = "IsTemplatesSyncedMandatoryFields", Value="false" }


            };
            modelBuilder.Entity<ConfigurationDAO>()
                .HasData(initConfiguration);
            var initPrograms = new List<ProgramDAO>() // need adi for that
            {
                new ProgramDAO() {  Id = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1),
                                    Name = "Trial",
                                    Templates = 2,
                                    DocumentsPerMonth = 5,
                                    VisualIdentificationsPerMonth = 0,
                                    VideoConferencePerMonth = 0,
                                    Note = "Upgrade now by calling our Support Center: (+972)3-1111111"
                },
                new ProgramDAO() {  Id = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2),
                                    Name = "Unlimited",
                                    Users = -1,
                                    Templates = -1,
                                    DocumentsPerMonth = -1,
                                    SmsPerMonth = -1,
                                    VisualIdentificationsPerMonth = 0,
                                    VideoConferencePerMonth = 0,
                                    ServerSignature = true,
                                    SmartCard = true
                },
                new ProgramDAO() { Id = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3),
                                    Name = "Basic",
                                    Users = 2,
                                    Templates = 15,
                                    DocumentsPerMonth = 50,
                                    SmsPerMonth = 200,
                                    VisualIdentificationsPerMonth = 0,
                                    VideoConferencePerMonth = 0,
                                    ServerSignature = true,
                                    SmartCard = true
                }
            };
            modelBuilder.Entity<ProgramDAO>()
                .HasData(initPrograms);
            var initCompanies = new List<CompanyDAO>
            {
                new CompanyDAO()
                {
                    Id = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1),
                    Name = "Free Accounts",
                    ProgramId = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1),
                  
                },
                new CompanyDAO()
                {
                    Id = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 5),
                    Name = "Ghost Users",
                    ProgramId = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1),
                 
                }
            };
            modelBuilder.Entity<CompanyDAO>()
                .HasData(initCompanies);
            var initCompaniesConfiguration = new List<CompanyConfigurationDAO>() {
                 new CompanyConfigurationDAO()
                {
              CompanyId = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 5),
              RecentPasswordsAmount = 3,

    }, new CompanyConfigurationDAO() {
         CompanyId = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1),
         RecentPasswordsAmount = 3

    }
    };
            modelBuilder.Entity<CompanyConfigurationDAO>()
            .HasData(initCompaniesConfiguration);

            var systemAdminUsers = new List<UserDAO>()
            {
                new UserDAO
                {
                    Id = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1),
                    CompanyId = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1),
                    Name = "SystemAdmin",
                    CreationTime = _dater?.UtcNow() ?? DateTime.MinValue,
                    Email = "systemadmin@comda.co.il",
                    Type = UserType.SystemAdmin,
                    //Default password Comsign1!
                    Password = "G8s52U1i2VXtX0NM+jq44qlKNrLCP+pFbv7E5OzRL4d7BJ+4",
                    Status = UserStatus.Activated,
                },
                new UserDAO
                {
                    Id = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2),
                    CompanyId = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1),
                    Name = "PaymentAdmin",
                    CreationTime =  _dater?.UtcNow()?? DateTime.MinValue,
                    Email = "paymentAdmin@comda.co.il",
                    Type = UserType.PaymentAdmin,
                    //Default password 123456
                    Password = "aFDgUq3rMdhhvRqzQ+/9v51hevUQyVubl2XdsvpZqQ/Q4dVz",
                    Status = UserStatus.Activated,
                },
                new UserDAO
                {
                    Id = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 5),
                    CompanyId = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 5),
                    Name = "GhostUser",
                    CreationTime =  _dater?.UtcNow()?? DateTime.MinValue,
                    Email = "ghost@comda.co.il",
                    Type = UserType.Ghost,
                    //Default password ghost123
                    Password = "oRxggjg8wbOxTC5DvP4vXzV32vFmnNdhQH8vRpElJ6lziTJk",
                    Status = UserStatus.Activated,
                },
                new UserDAO
                {
                    Id = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 6, 6),
                    CompanyId = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 5),
                    Name = "DevUser",
                    CreationTime =  _dater?.UtcNow()?? DateTime.MinValue,
                    Email = "dev@comda.co.il",
                    Type = UserType.Dev,
                    //Default password dev123
                    Password = "h05j6uZ6S0kHffebPVOUy4Cr1QBfRbsb9oO4/IShSVNyw9sc",
                    Status = UserStatus.Activated,
                }
            };

            modelBuilder.Entity<UserDAO>()
               .HasData(systemAdminUsers);
        }

        private void OnDocumentCollectionModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DocumentCollectionDAO>()
                  .HasMany<SignerDAO>(d => d.Signers)
                  .WithOne(s => s.DocumentCollection)
                  .HasForeignKey(s => s.DocumentCollectionId)
                  .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DocumentCollectionDAO>()
                .HasMany<DocumentDAO>(d => d.Documents)
                .WithOne(s => s.DocumentCollection)
                .HasForeignKey(s => s.DocumentCollectionId);

            modelBuilder.Entity<DocumentCollectionDAO>()
                .HasOne<UserDAO>(d => d.User)
                .WithMany(u => u.DocumentCollections)
                .HasForeignKey(u => u.UserId);

            modelBuilder.Entity<DocumentCollectionDAO>()
               .HasMany<SignerTokenMappingDAO>(x => x.TokensMapping)
               .WithOne(s => s.DocumentCollection)
               .HasForeignKey(s => s.DocumentCollectionId);
        }

        private void OnDocumentModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DocumentDAO>()
               .HasMany<DocumentSignatureFieldDAO>(x => x.SignatureFields)
               .WithOne(s => s.Document)
               .HasForeignKey(s => s.DocumentId);
        }

        private void OnUserModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserDAO>()
                .HasOne<CompanyDAO>(u => u.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(c => c.CompanyId);

            modelBuilder.Entity<UserDAO>()
                .HasOne<UserConfigurationDAO>(s => s.UserConfiguration)
                .WithOne(u => u.User)
                .HasForeignKey<UserConfigurationDAO>(c => c.UserId);

            modelBuilder.Entity<UserDAO>()
                .HasOne<UserTokensDAO>(s => s.UserTokens)
                .WithOne(u => u.User)
                .HasForeignKey<UserTokensDAO>(c => c.UserId);
            
            modelBuilder.Entity<UserDAO>().
                HasMany(x => x.AdditionalGroupsMapper).
                WithOne(x => x.User).
                HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserDAO>()
                .HasMany(x => x.UserPeriodicReports).
                WithOne(x => x.User).
                HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserDAO>()
                .HasMany(x => x.ManagementPeriodicReports).
                WithOne(x => x.User).
                HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        }

        private void OnProgramModelCreating(ModelBuilder modelBuilder)
        {
           modelBuilder.Entity<ProgramDAO>()
                .HasOne<ProgramUIViewDAO>(p => p.ProgramUIView)
                .WithOne(u => u.Program)            
                .HasForeignKey<ProgramUIViewDAO>(x => x.ProgramId);
        }

        private void OnCompanyConfigurationDAO(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompanyDAO>()
            .HasOne<CompanySigner1DetailDAO>(cc => cc.CompanySigner1Details)
            .WithOne(cs1d => cs1d.Company)
            .HasForeignKey<CompanySigner1DetailDAO>(cs1d => cs1d.CompanyId);
        }

        private void OnGroupModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GroupDAO>()
                .HasOne<CompanyDAO>(g => g.Company)
                .WithMany(c => c.Groups)
                .HasForeignKey(g => g.CompanyId);



            modelBuilder.Entity<GroupDAO>()
              .HasOne<ActiveDirectoryGroupDAO>(g => g.ActiveDirectoryGroup)
              .WithOne(c => c.Group)
              .HasForeignKey<ActiveDirectoryGroupDAO>(g => g.GroupId);


            modelBuilder.Entity<AdditionalGroupMapperDAO>().HasOne(x => x.Group).WithMany().HasForeignKey(g => g.GroupId).OnDelete(DeleteBehavior.Restrict);
                
            modelBuilder.Entity<AdditionalGroupMapperDAO>().HasOne(x => x.Company).WithMany().HasForeignKey(c => c.CompanyId).OnDelete(DeleteBehavior.Restrict); 
            


        }

        private void OnCompanyModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompanyDAO>()
                .HasOne<ProgramDAO>(c => c.Program)
                .WithMany(p => p.Companies)
                .HasForeignKey(p => p.ProgramId);

            modelBuilder.Entity<CompanyDAO>()
                .HasOne<CompanyConfigurationDAO>(c => c.CompanyConfiguration)
                .WithOne(cc => cc.Company)
                .HasForeignKey<CompanyConfigurationDAO>(cc => cc.CompanyId);

            modelBuilder.Entity<CompanyDAO>()
                .HasOne<CompanySigner1DetailDAO>(c => c.CompanySigner1Details)
                .WithOne(cc => cc.Company)
                .HasForeignKey<CompanySigner1DetailDAO>(cc => cc.CompanyId);

            modelBuilder.Entity<CompanyConfigurationDAO>()
                .HasMany<MessageProviderDAO>(c => c.MessageProviders)
                .WithOne(m => m.CompanyConfiguration)
                .HasForeignKey(m => m.CompanyId);

            modelBuilder.Entity<CompanyConfigurationDAO>()
                .HasMany<CompanyMessageDAO>(c => c.CompanyMessages)
                .WithOne(m => m.CompanyConfiguration)
                .HasForeignKey(m => m.CompanyId);

            modelBuilder.Entity<CompanyConfigurationDAO>().Property(x => x.DefaultSigningType).HasDefaultValue(Common.Enums.PDF.SignatureFieldType.Graphic);
            modelBuilder.Entity<CompanyConfigurationDAO>().Property(x => x.EnableDisplaySignerNameInSignature).HasDefaultValue(true);


        }

        private void OnSignerModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SignerDAO>()
                  .HasMany<SignerAttachmentDAO>(s => s.SignerAttachments)
                  .WithOne(a => a.Signer)
                  .HasForeignKey(a => a.SignerId);

            modelBuilder.Entity<SignerDAO>()
                .HasOne<NotesDAO>(s => s.Notes)
                .WithOne(n => n.Signer)
                .HasForeignKey<NotesDAO>(n => n.SignerId);

            modelBuilder.Entity<SignerDAO>()
               .HasOne<SignerOtpDetailsDAO>(s => s.OtpDetails)
               .WithOne(o => o.Signer)
               .HasForeignKey<SignerOtpDetailsDAO>(o => o.SignerId);

            modelBuilder.Entity<SignerDAO>()
                .HasMany<SignerFieldDAO>(s => s.SignerFields)
                .WithOne(a => a.Signer)
                .HasForeignKey(a => a.SignerId);

            modelBuilder.Entity<SignerDAO>()
                .HasOne<ContactDAO>(x => x.Contact)
                .WithMany(s => s.Signers)
                .HasForeignKey(x => x.ContactId);
        }

        private void OnTemplateModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TemplateDAO>()
                 .HasMany<DocumentDAO>(t => t.Documents)
                 .WithOne(d => d.Template)
                 .HasForeignKey(d => d.TemplateId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TemplateDAO>()
               .HasMany<TemplateTextFieldDAO>(t => t.TemplateTextFields)
               .WithOne(tf => tf.Template)
               .HasForeignKey(tf => tf.TemplateId);

            modelBuilder.Entity<TemplateDAO>()
               .HasMany<TemplateSignatureFieldDAO>(t => t.TemplateSignatureFields)
               .WithOne(sf => sf.Template)
               .HasForeignKey(sf => sf.TemplateId);

            modelBuilder.Entity<TemplateSignatureFieldDAO>()
                .Property(sf => sf.Mandatory)
                .HasDefaultValue(true);
        }

        private void OnContactModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContactDAO>()
                        .HasMany<ContactSealsDAO>(s => s.Seals)
                        .WithOne(c => c.Contact)
                        .HasForeignKey(c => c.ContactId);

            modelBuilder.Entity<ContactDAO>()
                .HasMany<SignerDAO>(c => c.Signers)
                .WithOne(s => s.Contact)
                .HasForeignKey(s => s.ContactId);

            modelBuilder.Entity<ContactDAO>().HasMany(c => c.ContactGroupsMember)
                .WithOne(s => s.Contact).
                HasForeignKey(s => s.ContactId);

            modelBuilder.Entity<ContactsGroupDAO>().
                HasMany(x => x.ContactGroupMembers).
                WithOne(x => x.ContactsGroup).
                HasForeignKey(x => x.ContactsGroupId);
           
            modelBuilder.Entity<ContactGroupMemberDAO>()
                .HasOne<ContactDAO>(x => x.Contact)
                .WithMany(s => s.ContactGroupsMember)
                .HasForeignKey(x => x.ContactId);

        }

        private void OnActiveDirectoryModelCreating(ModelBuilder modelBuilder)
        {


        }

        #endregion

        public DbSet<UserDAO> Users { get; set; }
        public DbSet<ContactDAO> Contacts { get; set; }
        public DbSet<ContactSealsDAO> ContactSeals { get; set; }
        public DbSet<UserConfigurationDAO> UserConfiguration { get; set; }
        public DbSet<UserPasswordHistoryDAO> UserPasswordHistory { get; set; }
        public DbSet<CompanyConfigurationDAO> CompanyConfiguration { get; set; }
        public DbSet<GroupDAO> Groups { get; set; }
        public DbSet<CompanyDAO> Companies { get; set; }
        public DbSet<ProgramDAO> Programs { get; set; }
        public DbSet<ProgramUtilizationDAO> ProgramUtilization { get; set; }
        public DbSet<ConfigurationDAO> Configuration { get; set; }
        public DbSet<TemplateDAO> Templates { get; set; }
        public DbSet<TemplateSignatureFieldDAO> TemplatesSignatureFields { get; set; }
        public DbSet<TemplateTextFieldDAO> TemplatesTextFields { get; set; }
        public DbSet<DocumentCollectionDAO> DocumentCollections { get; set; }
        public DbSet<DocumentDAO> Documents { get; set; }
        public DbSet<DocumentSignatureFieldDAO> DocumentsSignatureFields { get; set; }
        public DbSet<SignerTokenMappingDAO> SignerTokensMapping { get; set; }
        public DbSet<SignerDAO> Signers { get; set; }
        public DbSet<UserTokensDAO> UserTokens { get; set; }
        public DbSet<LogDAO> Logs { get; set; }
        public DbSet<SignerLogDAO> SignerLogs { get; set; }
        public DbSet<ManagementLogDAO> ManagementLogs { get; set; }
        public DbSet<ActiveDirectoryConfigDAO> ActiveDirectoryConfigs { get; set; }
        public DbSet<ActiveDirectoryGroupDAO> ActiveDirectoryGroups { get; set; }
        public DbSet<TabletDAO> Tablets { get; set; }
        public DbSet<CompanySigner1DetailDAO> CompanySigner1Details { get; set; }
        public DbSet<ProgramUtilizationHistoryDAO> ProgramUtilizationHistories { get; set; }
        public DbSet<ContactGroupMemberDAO> ContactGroupMembers { get; set; }
        public DbSet<ContactsGroupDAO> ContactsGroup { get; set; }
        public DbSet<AdditionalGroupMapperDAO> AdditionalGroupsMapper { get; set; }
        public DbSet<UserPeriodicReportDAO> UserPeriodicReports { get; set; }
        public DbSet<ManagementPeriodicReportDAO> ManagementPeriodicReports { get; set; }
        public DbSet<ManagementPeriodicReportEmailDAO> ManagementPeriodicReportEmails { get; set; }
        public DbSet<PeriodicReportFileDAO> PeriodicReportFiles { get; set; }
        public DbSet<UserOtpDetailsDAO> UserOtpDetails { get; set; }
        public DbSet<SingleLinkAdditionalResourceDAO> SingleLinkAdditionalResources { get; set; }
    }
}
