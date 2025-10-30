using Common.Handlers;
using Common.Interfaces;
using Common.Interfaces.License;
using Common.Interfaces.UserApp;
using Common.Models.License;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.IO.Abstractions;
using System.Threading;

namespace BL.Handlers
{
    public class LicenseHandler : BaseLicenseHandler, ILicense
    {
        private ReaderWriterLockSlim cacheLock = new ReaderWriterLockSlim();
        private static LicenseResurce _licenseResurce = null;
        private readonly IDater _dater;

        public LicenseHandler(ILogger logger, IOptions<GeneralSettings> generalSettings, IOptions<FolderSettings> folderSettings, ILicenseWrapper licenseWrapper, IFileSystem fileSystem,
          IDater dater)
            : base(generalSettings, folderSettings, logger, licenseWrapper, fileSystem) {
            _dater = dater;
        }

        public IWeSignLicense GetLicenseInformation()
        {
            if (_licenseResurce == null || _dater.UtcNow() > _licenseResurce.NextRefrash)
            {
                cacheLock.EnterWriteLock();
                try
                {
                    RefrashResurce();
                }
                finally
                {
                    cacheLock.ExitWriteLock();
                }
            }

            cacheLock.EnterReadLock();
            try
            {
                return _licenseResurce.WeSignLicense;
            }
            finally
            {
                cacheLock.ExitReadLock();
            }
        }

        private void RefrashResurce()
        {

            if (_licenseResurce == null || _dater.UtcNow() > _licenseResurce.NextRefrash)
            {
                _licenseResurce = new LicenseResurce
                {
                    WeSignLicense = ReadLicenseInformation(),
                    NextRefrash = _dater.UtcNow().AddMinutes(2)
                };
            }
            
        }

        
    }
    internal class LicenseResurce
    {
        public IWeSignLicense WeSignLicense { get; set; }
        public DateTime NextRefrash { get; set; }

    }

}
