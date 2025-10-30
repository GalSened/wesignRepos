using Common.Enums;
using Common.Enums.Results;
using Common.Extensions;
using Common.Handlers;
using Common.Handlers.Files;
using Common.Interfaces;
using Common.Interfaces.Files;
using Common.Models;
using Common.Models.FileGateScanner;
using Common.Models.Settings;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using Xunit;

namespace Common.Tests.Handlers
{
    public class ContactSignaturesHandlerTests : IDisposable
    {
        private readonly List<string> INVALID_SIGNATURE_IMAGES = new List<string>() { ".invalid-signature" };
        private readonly Mock<IFileSystem> _fileSystem;
        private readonly Mock<IDirectory> _directory;
        private readonly Mock<IPath> _path;
        private readonly Mock<IValidator> _validator;
        private readonly Mock<IDataUriScheme> _dataUriScheme;
        private readonly Mock<IOptions<FolderSettings>> _options;
        private readonly Mock<FolderSettings> _folderSettings;
        private readonly Mock<Contact> _contact;
        private readonly IContactSignatures _contactSignatures;
        private readonly Mock<IFilesWrapper> _filesWrapper;

        public ContactSignaturesHandlerTests()
        {
            _fileSystem = new Mock<IFileSystem>();
            _directory = new Mock<IDirectory>();
            _path = new Mock<IPath>();
            _validator = new Mock<IValidator>();
            _dataUriScheme = new Mock<IDataUriScheme>();
            _options = new Mock<IOptions<FolderSettings>>();
            _folderSettings = new Mock<FolderSettings>();
            _filesWrapper = new Mock<IFilesWrapper>();
            _contact = new Mock<Contact>();
            _fileSystem.SetupGet(_ => _.Directory).Returns(_directory.Object);
            _fileSystem.SetupGet(_ => _.Path).Returns(_path.Object);
            _options.SetupGet(_ => _.Value).Returns(_folderSettings.Object);
            _contactSignatures = new ContactSignaturesHandler(_filesWrapper.Object, _validator.Object);
        }

        public void Dispose()
        {
            _fileSystem.Invocations.Clear();
            _directory.Invocations.Clear();
            _path.Invocations.Clear();
            _validator.Invocations.Clear();
            _dataUriScheme.Invocations.Clear();
            _options.Invocations.Clear();
            _folderSettings.Invocations.Clear();
            _contact.Invocations.Clear();
        }

        #region UpdateSignatureImages

      

        #endregion
    }
}
