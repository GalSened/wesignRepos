using Common.Models.Settings;
using ManagementBL.Handlers;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ManagementBL.Tests.Handlers
{
    public class OtpHandlerTests
    {
        private readonly IOptions<GeneralSettings> _generalSettings;

        private readonly OtpHandler _otpHandler;

        public OtpHandlerTests()
        {
            _generalSettings = Options.Create(new GeneralSettings { });

            _otpHandler = new OtpHandler(_generalSettings);
        }


        [Fact]
        public void GenerateCode_()
        {
            var actual = _otpHandler.GenerateCode();
        }


        [Fact]
        public async Task IsValidCode_EmptyCode()
        {
            var actual = await Assert.ThrowsAsync<Exception>(() => _otpHandler.IsValidCode(""));

            Assert.Equal("Null input - code is null or empty",actual.Message);
        }
    }
}
