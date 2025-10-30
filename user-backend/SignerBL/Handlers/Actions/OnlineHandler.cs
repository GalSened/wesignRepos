using Common.Enums;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.SignerApp;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents.Signers;
using Serilog;
using System.Threading.Tasks;

namespace SignerBL.Handlers.Actions
{
    public class OnlineHandler : IDocumentModeAction
    {
        private readonly ICompanyConnector _companyConnector;
        private readonly IConfiguration _configuration;
        private readonly Common.Interfaces.SignerApp.ISignerValidator _validator;
        private readonly ISender _sender;
        private readonly ILogger _logger;
        private readonly IDoneActionsHelper _actionsHelper;


        public OnlineHandler(ICompanyConnector companyConnector, IConfiguration configuration, Common.Interfaces.SignerApp.ISignerValidator validator,
            ISender sender, ILogger logger, IDoneActionsHelper actionsHelper)
        {
            _companyConnector = companyConnector;
            _configuration = configuration;
            _validator = validator;
            _sender = sender;
            _logger = logger;
            _actionsHelper = actionsHelper;
        }

        public async Task<string> DoAction(DocumentCollection dbDocumentCollection, Signer dbSigner)
        {
            Configuration appConfiguration = await _configuration.ReadAppConfiguration();
            CompanyConfiguration companyConfiguration = await _companyConnector.ReadConfiguration(new Company() { Id = dbDocumentCollection.User.CompanyId });
            string downloadLink = "";

            if (_configuration.ShouldNotifyWhileSignerSigned(dbDocumentCollection.User, companyConfiguration, dbDocumentCollection.Notifications))
            {
                await _sender.SendEmailNotification(MessageType.SingleSignerSignedNotification, dbDocumentCollection, appConfiguration, dbSigner, companyConfiguration); 
            }
            if (_validator.AreAllSignersSigned(dbDocumentCollection?.Signers))
            {
                await _actionsHelper.HandlerSigningUsingSigner1AfterDocumentSigningFlow(dbDocumentCollection);
                await _sender.SendEmailNotification(MessageType.AllSignersSignedNotification, dbDocumentCollection, appConfiguration, dbSigner, companyConfiguration);

                foreach (var signer in dbDocumentCollection?.Signers)
                {
                    downloadLink = await _sender.SendSignedDocument(dbDocumentCollection, appConfiguration, signer, companyConfiguration);
                }
            }

            return downloadLink;
        }
    }
}
