using Common.Models.License;
using Common.Models.Settings;
using Serilog;
using System;
using Common.Extensions;
using Microsoft.Extensions.Options;
using Comda.License.Enums;
using Comda.License.Interfaces;
using Common.Interfaces;
using Common.Enums.Results;
using System.IO.Abstractions;
using Common.Interfaces.License;

namespace Common.Handlers
{
    public abstract class BaseLicenseHandler
    {
        protected GeneralSettings _generalSettings;
        protected FolderSettings _folderSettings;
        protected ILicenseManager _licenseManager;
        protected IFileSystem _fileSystem;
        protected ILogger _logger;
        private static object locker = new object();

        //TODO change the new operation to dependency injection solution
        public BaseLicenseHandler(IOptions<GeneralSettings> generalSettings, IOptions<FolderSettings> folderSettings, ILogger logger,
            ILicenseWrapper licenseWrapper, IFileSystem fileSystem )
        {
            _generalSettings = generalSettings.Value;
            _folderSettings = folderSettings.Value;
            _logger = logger;
            _licenseManager = licenseWrapper.GetLicenseManager();
            _fileSystem = fileSystem;
            _fileSystem.Directory.CreateDirectory(@_folderSettings.License);
        }

        /// <summary>
        /// Try to get license , id success license valid, else license is not valid and exception will throw
        /// </summary>
        /// <returns></returns>
        public IWeSignLicense ReadLicenseInformation()
        {
            var licenseExpiration = _licenseManager.ValidateExpirationTime();
            if (licenseExpiration?.Status != LicenseStatus.Activated)
            {
                throw new InvalidOperationException(ResultCode.InvalidLicense.GetNumericString());
            }

            var licenseProperties = _licenseManager.GetLicenseProperties();

            return new WeSignLicense(licenseProperties, _fileSystem)
            {
                ExpirationTime = licenseExpiration.ExpirationDate
            };
        }
    }
}
