using Common.Interfaces;
using Common.Interfaces.Reports;

namespace BL.Handlers
{
    public class BLHandler : IBL
    {
        public IUsers Users { get; }
        public IContacts Contacts { get; }
        public IDocumentCollections DocumentCollections { get; }
        public ITemplates Templates { get; }
        public IAdmins Admins { get; }
        public IOneTimeTokens OneTimeTokens { get; }
        public ISelfSign SelfSign { get; }
        public IConfiguration Configuration { get;  }
        public IDistribution Distribution { get; }
        public ILinks Links { get; }
        public IReports Reports { get; set; }

        public BLHandler(IUsers users, IContacts contacts, IDocumentCollections documentCollections, ITemplates templates, 
            IAdmins admins, IOneTimeTokens oneTimeTokens, ISelfSign selfSign, IConfiguration configuration, IDistribution distribution, ILinks links, IReports reports)
        {
            Users = users;
            Contacts = contacts;
            DocumentCollections = documentCollections;
            Templates = templates;
            Admins = admins;
            OneTimeTokens = oneTimeTokens;
            SelfSign = selfSign;
            Configuration = configuration;
            Distribution = distribution;
            Links = links;
            Reports = reports;
        }
    }
}
