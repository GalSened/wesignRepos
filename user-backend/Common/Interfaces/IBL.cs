using Common.Interfaces.Reports;

namespace Common.Interfaces
{
    public interface IBL
    {
        IUsers Users { get; }
        IContacts Contacts { get; }
        IAdmins Admins { get; }
        IDocumentCollections DocumentCollections { get; }
        ITemplates Templates { get; }
        IOneTimeTokens OneTimeTokens { get; }
        ISelfSign SelfSign { get; }
        IConfiguration Configuration { get; }
        IDistribution Distribution { get; }
        ILinks Links { get; }
        IReports Reports { get; }
    }
}
