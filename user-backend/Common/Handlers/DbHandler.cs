namespace Common.Handlers
{
    using Common.Interfaces.DB;
 

    public class DbHandler : IDbConnector
    {
        public IUserConnector Users { get; }
        public ICompanyConnector Companies { get; }
        public IContactConnector Contacts { get; }
        public IProgramConnector Programs { get; }
        public IProgramUtilizationConnector ProgramUtilizations { get; }
        public IConfigurationConnector Configurations { get; }
        public IDocumentCollectionConnector DocumentCollections { get; }
        public IDocumentConnector Documents { get; }
        public ITemplateConnector Templates { get; }
        public ISignerTokenMappingConnector SignerTokensMapping { get; }
        public IGroupConnector Groups { get; }
        public IUserTokenConnector UserTokens { get; }
        public IUserPasswordHistoryConnector UserPasswordHistory { get; }
        public ILogConnector Logs { get; }
        public IActiveDirectoryConfigConnector ActiveDirectoryConfiguration { get; }

        public IActiveDirectoryGroupsConnector ActiveDirectoryGroups { get; }

        public ISignersConnector Signers { get; }
        public IProgramUtilizationHistoryConnector ProgramUtilizationHistories { get; }

        public IContactsGroupsConnector ContactsGroupsConnector { get; }
        public IUserPeriodicReportConnector UserPeriodicReport { get; }
        public IManagementPeriodicReportConnector ManagementPeriodicReport { get; set; }
        public IManagementPeriodicReportEmailConnector ManagementPeriodicReportEmail { get; set; }


        public DbHandler(IUserConnector users, IContactConnector contacts, IProgramConnector programs, IConfigurationConnector configurations, IDocumentCollectionConnector documentCollections, IDocumentConnector documents, ITemplateConnector templates, IProgramUtilizationConnector programUtilizations, ISignerTokenMappingConnector signerTokensMapping, IGroupConnector groups, IUserTokenConnector userTokens,
            IUserPasswordHistoryConnector userPasswordHistory, ICompanyConnector companies, ILogConnector logs, IActiveDirectoryConfigConnector activeDirectory, IContactsGroupsConnector contactsGroupsConnector,
            IActiveDirectoryGroupsConnector activeDirectoryGroups, ISignersConnector  signers, IProgramUtilizationHistoryConnector programUtilizationHistories, IUserPeriodicReportConnector userPeriodicReport, IManagementPeriodicReportConnector managementPeriodicReport, IManagementPeriodicReportEmailConnector managementPeriodicReportEmail)
        {
            Users = users;
            Contacts = contacts;
            Programs = programs;
            Configurations = configurations;
            DocumentCollections = documentCollections;
            Documents = documents;
            Templates = templates;
            ProgramUtilizations = programUtilizations;
            SignerTokensMapping = signerTokensMapping;
            Groups = groups;
            UserTokens = userTokens;
            UserPasswordHistory = userPasswordHistory;
            Companies = companies;
            Logs = logs;
            ActiveDirectoryConfiguration = activeDirectory;
            ActiveDirectoryGroups = activeDirectoryGroups;
            Signers = signers;
            ProgramUtilizationHistories = programUtilizationHistories;
            ContactsGroupsConnector = contactsGroupsConnector;
            UserPeriodicReport = userPeriodicReport;
            ManagementPeriodicReport = managementPeriodicReport;
            ManagementPeriodicReportEmail = managementPeriodicReportEmail;
        }
    }
}
