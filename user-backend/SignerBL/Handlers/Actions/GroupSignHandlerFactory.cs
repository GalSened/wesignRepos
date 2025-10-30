using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.SignerApp;
using Serilog;
using ISignerValidator = Common.Interfaces.SignerApp.ISignerValidator;

namespace SignerBL.Handlers.Actions
{
    public class GroupSignHandlerFactory : IDocumentModeHandlerFactory
    {
        private readonly ICompanyConnector _companyConnector;
        private readonly IConfiguration _configuration;
        private readonly ISender _sender;
        private readonly ISignerValidator _validator;
        private readonly ILogger _logger;
        private readonly IDater _dater;
        private readonly IDoneActionsHelper _actionsHelper;

        public GroupSignHandlerFactory(ILogger logger, ICompanyConnector companyConnector, IDocumentCollectionConnector documentCollectionConnector, IConfiguration configuration, ISignerValidator validator,
            ISender sender, IDater dater, IDoneActionsHelper actionsHelper)
        {
            _companyConnector = companyConnector;
            _configuration = configuration;
            _validator = validator;
            _sender = sender;
            _logger = logger;
            _dater = dater;
            _actionsHelper = actionsHelper;

        }

        public IDocumentModeAction Create() => new GroupSignHandler(_companyConnector, _configuration, _validator, _sender, _logger, _actionsHelper);
    }
}
