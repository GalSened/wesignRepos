using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.SignerApp;
using Serilog;
using ISignerValidator = Common.Interfaces.SignerApp.ISignerValidator;

namespace SignerBL.Handlers.Actions
{
    public class OrderedGroupSignHandlerFactory : IDocumentModeHandlerFactory
    {
        private readonly ICompanyConnector _companyConnector;
        private readonly IDocumentCollectionConnector _documentCollectionConnector;
        private readonly IConfiguration _configuration;
        private readonly ISender _sender;
        private readonly ISignerValidator _validator;
        private readonly ILogger _logger;
        private readonly IDater _dater;
        private readonly IDoneActionsHelper _actionsHelper;

        public OrderedGroupSignHandlerFactory(ILogger logger, ICompanyConnector companyConnector,IDocumentCollectionConnector documentCollectionConnector, IConfiguration configuration, ISignerValidator validator, ISender sender, IDater dater, IDoneActionsHelper actionsHelper)
        {
            _companyConnector = companyConnector;
            _documentCollectionConnector = documentCollectionConnector;
            _configuration = configuration;
            _validator = validator;
            _sender = sender;
            _logger = logger;
            _dater = dater;
            _actionsHelper = actionsHelper;
        }

        public IDocumentModeAction Create() =>  new OrderedGroupSignHandler(_companyConnector, _documentCollectionConnector,_configuration, _sender, _logger, _dater, _actionsHelper);
    }
}
