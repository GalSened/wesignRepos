using Common.Enums;
using Common.Interfaces.DB;
using Common.Interfaces.FileGateScanner;
using Common.Interfaces;
using Common.Models.FileGateScanner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common.Models;
using Common.Enums.Users;
using Common.Enums.Results;
using Common.Extensions;

namespace Common.Handlers
{
    public  class ValidatorHandler : IValidator
    {
        private readonly IProgramConnector _programConnector;
        private readonly IConfigurationConnector _configurationConnector;
        private readonly IFileGateScannerProviderFactory _fileGateScannerProviderHandler;

        public ValidatorHandler(IProgramConnector programConnector, IConfigurationConnector configurationConnector, IFileGateScannerProviderFactory fileGateScannerProviderHandler)
        {
            _programConnector = programConnector;
            _configurationConnector = configurationConnector;
            _fileGateScannerProviderHandler = fileGateScannerProviderHandler;
        }

        public bool HasDuplication(IEnumerable<string> collection)
        {
            try
            {
                return collection.Distinct().Count() != collection.Count();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool IsValidEmail(string email)
        {
            try
            {
                var rgx = new Regex("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$");
                return rgx.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        public async Task ValidateEditorUserPermissions(User user)
        {
            if (user.Type != UserType.Editor && user.Type != UserType.CompanyAdmin && user.Type != UserType.SystemAdmin)
            {
                throw new InvalidOperationException(ResultCode.UserIsNotEditorOrCompanyAdmin.GetNumericString());
            }
            if (await _programConnector.IsProgramExpired(user))
            {
                throw new InvalidOperationException(ResultCode.UserProgramExpired.GetNumericString());
            }
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
