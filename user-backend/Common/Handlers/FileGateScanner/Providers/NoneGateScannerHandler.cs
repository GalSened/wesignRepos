using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.FileGateScanner;
using Common.Models.FileGateScanner;
using Serilog;
using System;

namespace Common.Handlers.FileGateScanner.Providers
{
    public class NoneGateScannerHandler : IFileGateScannerProvider
    {
        
        private readonly IDataUriScheme _dataUriScheme;
        private readonly ILogger _logger;
        
        public NoneGateScannerHandler(ILogger logger, IDataUriScheme dataUriScheme)
        {
            _logger = logger;
            
            _dataUriScheme = dataUriScheme;
        }

        public FileGateScanResult Scan(FileGateScan fileGateScan)
        {
            return new FileGateScanResult { IsValid = true, CleanFile = fileGateScan?.Base64 };
        }
    }
}
