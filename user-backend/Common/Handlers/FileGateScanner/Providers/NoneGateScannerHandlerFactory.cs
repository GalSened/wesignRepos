using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.FileGateScanner;
using Serilog;

namespace Common.Handlers.FileGateScanner.Providers
{
    public class NoneGateScannerHandlerFactory : IFileGateScannerProviderHandlerFactory
    {

        private readonly ILogger _logger;
        private readonly IDataUriScheme _dataUriScheme;

        public NoneGateScannerHandlerFactory(ILogger logger, IDataUriScheme dataUriScheme)
        {
            _logger = logger;      
            _dataUriScheme = dataUriScheme;
        }

        public IFileGateScannerProvider Create() => new NoneGateScannerHandler(_logger, _dataUriScheme);

    }
}
