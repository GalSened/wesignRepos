using Comda.License.Interfaces;
using Common.Handlers;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Tests.Handlers
{
    public class BaseLicenseHandlerTests
    {

        private readonly Mock<ILicenseManager> _licenseManager;
        private readonly BaseLicenseHandler _baseLicenseHandler;

        public BaseLicenseHandlerTests()
        {

            _licenseManager = new Mock<ILicenseManager>();
            
        }

    }
}
