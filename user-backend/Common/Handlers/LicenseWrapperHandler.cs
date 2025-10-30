using Comda.License;
using Comda.License.Interfaces;
using Common.Interfaces;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using Serilog;

namespace Common.Handlers
{
    public class LicenseWrapperHandler : ILicenseWrapper
    {
        private GeneralSettings _generalSettings;
        private FolderSettings _folderSettings;
        private ILogger _logger;

        public LicenseWrapperHandler(IOptions<GeneralSettings> generalSettings, IOptions<FolderSettings> folderSettings, ILogger logger)
        {
            _generalSettings = generalSettings.Value;
            _folderSettings = folderSettings.Value;
            _logger = logger;
        }
        public ILicenseManager GetLicenseManager()
        {
            return new Manager(_generalSettings.ProductId, _logger.Debug, _logger.Error, _generalSettings?.LicenseDMZEndpoint, _folderSettings.License);
        }
    }
}
