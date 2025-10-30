namespace Common.Interfaces.DB
{
    public interface IDbConnector
    {
        IUserConnector Users { get; }
        IGroupConnector Groups { get; }
        IContactConnector Contacts { get; }
        ICompanyConnector Companies { get; }
        IProgramConnector Programs { get; }
        IProgramUtilizationConnector ProgramUtilizations { get; }
        IDocumentConnector Documents { get; }
        IDocumentCollectionConnector DocumentCollections { get; }
        IConfigurationConnector Configurations { get; }
        ITemplateConnector Templates { get; }
        ISignerTokenMappingConnector SignerTokensMapping { get; }
        IUserTokenConnector UserTokens { get; }
        IUserPasswordHistoryConnector UserPasswordHistory { get; }
        ILogConnector Logs { get; }
        IActiveDirectoryConfigConnector ActiveDirectoryConfiguration { get; }
        IActiveDirectoryGroupsConnector ActiveDirectoryGroups { get; }
        ISignersConnector Signers { get; }
        IProgramUtilizationHistoryConnector ProgramUtilizationHistories { get; }
        IContactsGroupsConnector ContactsGroupsConnector { get; }
        IUserPeriodicReportConnector UserPeriodicReport { get; }
        IManagementPeriodicReportConnector ManagementPeriodicReport { get; }
        IManagementPeriodicReportEmailConnector ManagementPeriodicReportEmail { get; }

    }
}
