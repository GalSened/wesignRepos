using Common.Enums;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.SignerApp;
using Common.Models;
using Common.Models.Documents.Signers;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SignerBL.Handlers.Actions
{
    public class OrderedGroupSignHandler : IDocumentModeAction
    {

        private readonly ICompanyConnector _companyConnector;
        private readonly IDocumentCollectionConnector _documentCollectionConnector;
        private readonly IConfiguration _configuration;
        private readonly ISender _sender;
        private readonly ILogger _logger;
        private readonly IDater _dater;
        private readonly IDoneActionsHelper _actionsHelper;

        public OrderedGroupSignHandler(ICompanyConnector companyConnector,IDocumentCollectionConnector documentCollectionConnector, IConfiguration configuration, ISender sender, ILogger logger, IDater dater, IDoneActionsHelper actionsHelper)
        {
            _companyConnector = companyConnector;
            _documentCollectionConnector = documentCollectionConnector;
            _configuration = configuration;
            _sender = sender;
            _logger = logger;
            _dater = dater;
            _actionsHelper = actionsHelper;
        }
        public async Task<string> DoAction(DocumentCollection dbDocumentCollection, Signer dbSigner)
        {
            var appConfiguration =await _configuration.ReadAppConfiguration();
            var companyConfiguration =await _companyConnector.ReadConfiguration(new Company() { Id = dbDocumentCollection.User.CompanyId });           

            var currentIndex = dbDocumentCollection.Signers.ToList().IndexOf(dbSigner);
            var nextSigner = dbDocumentCollection.Signers.ElementAtOrDefault(currentIndex + 1);
            bool isCurrentSignerIsLastSignerInSigningOrder = nextSigner == null;

            if (isCurrentSignerIsLastSignerInSigningOrder)
            {
                await _actionsHelper.HandlerSigningUsingSigner1AfterDocumentSigningFlow(dbDocumentCollection);                
                await _sender.SendEmailNotification(MessageType.AllSignersSignedNotification, dbDocumentCollection, appConfiguration, dbSigner, companyConfiguration);

                string downloadLink = "";
                foreach (var signer in dbDocumentCollection.Signers)
                {
                    downloadLink = await _sender.SendSignedDocument(dbDocumentCollection, appConfiguration, signer, companyConfiguration);
                }
                
                return downloadLink;
            }
            if (dbDocumentCollection?.Notifications?.ShouldSendDocumentForSigning ?? true)
            {
                await _sender.SendSigningLinkToNextSigner(dbDocumentCollection, appConfiguration, companyConfiguration, nextSigner);
            }
            nextSigner.TimeSent = _dater.UtcNow();
            nextSigner.TimeLastSent = nextSigner.TimeSent.Value;
            await _documentCollectionConnector.Update(dbDocumentCollection);
            if (_configuration.ShouldNotifyWhileSignerSigned(dbDocumentCollection.User, companyConfiguration, dbDocumentCollection.Notifications))
            {
                await _sender.SendEmailNotification(MessageType.SingleSignerSignedNotification, dbDocumentCollection, appConfiguration, dbSigner, companyConfiguration); 
            }
            return "";
        }
    }
}
