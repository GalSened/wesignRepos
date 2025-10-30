using Common.Enums;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.FileGateScanner;
using Common.Models;
using Common.Models.FileGateScanner;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ManagementBL.Handlers
{
    public class ManagementValidatorHandler : IValidator
    {
        private readonly IConfigurationConnector _configurationConnector;
        private readonly IFileGateScannerProviderFactory _fileGateScannerProviderHandler;

        public ManagementValidatorHandler(IConfigurationConnector  configurationConnector, IFileGateScannerProviderFactory fileGateScannerProviderHandler)
        {
            _configurationConnector = configurationConnector;
            _fileGateScannerProviderHandler = fileGateScannerProviderHandler;
        }

        public bool HasDuplication(IEnumerable<string> collection)
        {
            throw new NotImplementedException();
        }       

        public Task ValidateEditorUserPermissions(User user)
        {
            throw new NotImplementedException();
        }

        public async Task<FileGateScanResult> ValidateIsCleanFile(string base64string)
        {
            var fileGateScannerConfiguration = (await _configurationConnector.Read())?.FileGateScannerConfiguration;
            var providerType = fileGateScannerConfiguration?.Provider ?? FileGateScannerProviderType.None;
            var fileGateScannerProviderHandler = _fileGateScannerProviderHandler.ExecuteCreation(providerType);
            var fileGateScan = new FileGateScan { Base64 = base64string };
            var result = fileGateScannerProviderHandler.Scan(fileGateScan);
            if (!result?.IsValid ?? false)
            {
                throw new InvalidOperationException(ResultCode.InvalidFileContent.GetNumericString());
            }

            return result;
        }
    }
}
