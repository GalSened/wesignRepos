using Common.Enums;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.FileGateScanner;
using Serilog;
using System;
using System.Collections.Generic;

namespace Common.Handlers.FileGateScanner
{
    public class FileGateScannerProviderFactoryHandler : IFileGateScannerProviderFactory
    {
        private readonly IDictionary<FileGateScannerProviderType, IFileGateScannerProviderHandlerFactory> _factories;
        
        public FileGateScannerProviderFactoryHandler(ILogger logger,  IDataUriScheme dataUriScheme)
        {
            _factories = new Dictionary<FileGateScannerProviderType, IFileGateScannerProviderHandlerFactory>();
            foreach (FileGateScannerProviderType providerType in Enum.GetValues(typeof(FileGateScannerProviderType)))
            {

                var parameters = new object[2];
                parameters[0] = logger;                
                parameters[1] = dataUriScheme;
                var factory = (IFileGateScannerProviderHandlerFactory)Activator.CreateInstance(Type.GetType("Common.Handlers.FileGateScanner.Providers." + Enum.GetName(typeof(FileGateScannerProviderType), providerType) + "GateScannerHandlerFactory"), parameters);
                _factories.Add(providerType, factory);
            }
        }

        public IFileGateScannerProvider ExecuteCreation(FileGateScannerProviderType providerType) => _factories[providerType]?.Create();        
    }
}
