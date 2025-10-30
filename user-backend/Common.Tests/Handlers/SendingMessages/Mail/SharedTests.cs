using Common.Handlers.Files;
using Common.Handlers.SendingMessages.Mail;
using Common.Interfaces;
using Common.Interfaces.Files;
using Common.Interfaces.MessageSending.Mail;
using Common.Models;
using Common.Models.Settings;

using Microsoft.Extensions.Options;
using Moq;
using System;

using System.IO.Abstractions;

using Xunit;

namespace Common.Tests.Handlers.SendingMessages.Mail
{
    public class SharedTests : IDisposable
    {
        private readonly IShared _sharedHandler;

        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IFileSystem> _fileSystemMock;

        private readonly IOptions<FolderSettings> _folderSettings;

        private readonly Mock<IFilesWrapper> _filesWrapper;
        public void Dispose()
        {
            _configurationMock.Invocations.Clear();
            _fileSystemMock.Invocations.Clear();
        }

        public SharedTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            _fileSystemMock = new Mock<IFileSystem>();
            _folderSettings = Options.Create(new FolderSettings() { });
            _filesWrapper = new Mock<IFilesWrapper>();
            _sharedHandler = new Shared(_configurationMock.Object, _filesWrapper.Object);
        }

        #region GetContactNameFormat

        [Fact]
        public void GetContactNameFormat_EmptyEmail_ShouldReturnStringWithoutSlash()
        {
            Contact contact = new Contact()
            {
                Name= "a",
                Email = "",
                Phone = "0501234567"
            };

            var result = _sharedHandler.GetContactNameFormat(contact);
            var expected = "a (0501234567)";

            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetContactNameFormat_EmailNotEmpty_ShouldReturnStringWithSlash()
        {
            Contact contact = new Contact()
            {
                Name = "a",
                Email = "a@comsign.co.il",
                Phone = "0501234567"
            };

            var result = _sharedHandler.GetContactNameFormat(contact);
            var expected = "a (0501234567 / a@comsign.co.il)";

            Assert.Equal(expected, result);
        }
        #endregion
    }
}
