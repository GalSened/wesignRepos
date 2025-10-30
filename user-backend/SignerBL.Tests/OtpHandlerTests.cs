using Common.Interfaces.DB;
using Common.Interfaces.SignerApp;
using Common.Interfaces;
using Common.Models.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using Microsoft.Extensions.Options;
using Common.Models.Documents.Signers;
using SignerBL.Handlers;
using Xunit;
using Common.Enums.Results;
using Common.Extensions;
using Common.Models;
using Common.Models.Configurations;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Common.Enums;

namespace SignerBL.Tests
{
    public class OtpHandlerTests : IDisposable
    {
        private const string GUID = "C14E4B8B-5970-4B50-A1F1-090B8F99D3B2";

        
        private readonly Mock<IJWT> _jwtMock;
        private readonly Mock<IDater> _daterMock;
        private readonly Mock<ISender> _senderMock;
        private readonly Mock<IEncryptor> _encriptor;
        private readonly IOptions<GeneralSettings> _generalOptions;
        private readonly Mock<ISignerTokenMappingConnector> _signerTokenMappingConnectorMock;
        private readonly Mock<IDocumentCollectionConnector> _documentCollectionConnectorMock;
        
        private readonly Mock<ISignersConnector> _signersConnectorMock;
        private readonly Mock<IConfigurationConnector> _configurationConnectorMock;
        private readonly Mock<ICompanyConnector> _companyConnectorMock;
        private readonly IOTP _otpHandler;

        public void Dispose()
        {
            _signerTokenMappingConnectorMock.Invocations.Clear();
            _documentCollectionConnectorMock.Invocations.Clear();
            _companyConnectorMock.Invocations.Clear();
            _configurationConnectorMock.Invocations.Clear();
            _signersConnectorMock.Invocations.Clear();
            _jwtMock.Invocations.Clear();
            _daterMock.Invocations.Clear();
            _senderMock.Invocations.Clear();
            _encriptor.Invocations.Clear();
        }

        public OtpHandlerTests()
        {
            _signerTokenMappingConnectorMock = new Mock<ISignerTokenMappingConnector>();
            _documentCollectionConnectorMock = new Mock<IDocumentCollectionConnector>();
            _signersConnectorMock = new Mock<ISignersConnector>();
            _configurationConnectorMock = new Mock<IConfigurationConnector>();  
            _companyConnectorMock = new Mock<ICompanyConnector>();
            _jwtMock = new Mock<IJWT>();
            _daterMock = new Mock<IDater>();
            _senderMock = new Mock<ISender>();
            _encriptor = new Mock<IEncryptor>();
            _generalOptions = Options.Create(new GeneralSettings()
            {
            });

            _otpHandler = new OtpHandler(_signerTokenMappingConnectorMock.Object, _documentCollectionConnectorMock.Object,
                _signersConnectorMock.Object,_configurationConnectorMock.Object, _companyConnectorMock.Object,
                _jwtMock.Object, _daterMock.Object, _senderMock.Object, _generalOptions, _encriptor.Object);
        }

        #region ValidatePassword

        [Fact]
        public async Task ValidatePassword_SignerIsNull_ShouldThrowException()
        {
            // Arrange
            

            SignerTokenMapping signerToken = new SignerTokenMapping();
            Signer signer = null;

            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerToken);
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);

            // Act
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _otpHandler.ValidatePassword());

            // Assert
            Assert.Equal(ResultCode.InvalidToken.GetNumericString(), actual.Message);
        }


        [Fact]
        public async Task ValidatePassword_DocumentCollectionIsNull_ShouldThrowException()
        {
            // Arrange
            

            SignerTokenMapping signerToken = new SignerTokenMapping();
            Signer signer = new Signer();
            DocumentCollection documentCollection = null;

            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerToken);
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            // Act
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _otpHandler.
            ValidatePassword());

            // Assert
            Assert.Equal(ResultCode.InvalidDocumentCollectionId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task ValidatePassword_InvalidCredential_ShouldThrowException()
        {
            // Arrange

            SignerTokenMapping signerToken = new SignerTokenMapping();
            Signer signer = new Signer()
            {
                Id = Guid.Parse(GUID),
                SignerAuthentication = new SignerAuthentication()
                {
                    OtpDetails = new OtpDetails()
                    {
                        Mode = OtpMode.CodeAndPasswordRequired,
                    }
                }
            };

            DocumentCollection documentCollection = new DocumentCollection()
            {

                Signers = new List<Signer>()
                {
                    signer
                },
            };

            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerToken);
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            // Act
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _otpHandler.ValidatePassword(default, null, true));

            // Assert
            Assert.Equal(ResultCode.InvalidCredential.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task ValidatePassword_OtpModeIsPasswordRequired_ShouldReturnEmptyString()
        {
            // Arrange
            

            SignerTokenMapping signerToken = new SignerTokenMapping();
            Signer signer = new Signer()
            {
                Id = Guid.Parse(GUID),
                SignerAuthentication = new SignerAuthentication()
                {
                    OtpDetails = new OtpDetails()
                    {
                        Mode = OtpMode.PasswordRequired,
                        Identification = "a"
                    }
                }
            };
            DocumentCollection documentCollection = new DocumentCollection()
            {

                Signers = new List<Signer>()
                {
                    signer
                }
            };

            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerToken);
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _encriptor.Setup(x => x.Decrypt(It.IsAny<string>())).Returns("a");
            // Act
            var actual = await _otpHandler.ValidatePassword(default, "a");

            // Assert
            Assert.Empty(actual.Item1);
        }


        [Fact]
        public async Task ValidatePassword_ShouldReturnSignerMeans_Success()
        {
            // Arrange
            

            SignerTokenMapping signerToken = new SignerTokenMapping();
            Signer signer = new Signer()
            {
                Id = Guid.Parse(GUID),
                SignerAuthentication = new SignerAuthentication()
                {
                    OtpDetails = new OtpDetails()
                    {
                        Mode = OtpMode.CodeAndPasswordRequired,
                        Identification = "a"
                    }
                }
            };
            DocumentCollection documentCollection = new DocumentCollection()
            {

                Signers = new List<Signer>()
                {
                    signer
                },
                User = new User()
            };
            string signerMeans = "signerMeans";

            Configuration configuration = new Configuration();
            Company company = new Company();

            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerToken);
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            // GenerateInternalCode
            _daterMock.Setup(x => x.UtcNow()).Callback(() => { });
            //_daterMock.Setup(x => x.UtcNow().AddMinutes(It.IsAny<double>())).Callback(() => { });
            _documentCollectionConnectorMock.Setup(x => x.Update(It.IsAny<DocumentCollection>())).Callback(() => { });

            _configurationConnectorMock.Setup(x => x.Read()).ReturnsAsync(configuration);
            _companyConnectorMock.Setup(x => x.Read(It.IsAny<Company>())).ReturnsAsync(company);
            _senderMock.Setup(x => x.SendOtpCode(configuration, signer, documentCollection.User, company)).ReturnsAsync(signerMeans);

            // Act
            var actual = await _otpHandler.ValidatePassword(default, "a");

            // Assert
            Assert.Equal(signerMeans, actual.Item1);
        }

        #endregion

        #region IsValidCode

        [Fact]
        public async Task IsValidCode_SignerIsNull_ShouldThrowException()
        {
            // Arrange
            

            SignerTokenMapping signerToken = new SignerTokenMapping();
            Signer signer = null;

            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerToken);
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);

            // Act
            var actual =await Assert.ThrowsAsync<InvalidOperationException>(() => _otpHandler.IsValidCode(""));

            // Assert
            Assert.Equal(ResultCode.InvalidToken.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task IsValidCode_DocumentCollectionIsNull_ShouldThrowException()
        {
            // Arrange
            

            SignerTokenMapping signerToken = new SignerTokenMapping();
            Signer signer = new Signer();
            DocumentCollection documentCollection = null;

            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerToken);
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);

            // Act
            var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => _otpHandler.IsValidCode(""));

            // Assert
            Assert.Equal(ResultCode.InvalidDocumentCollectionId.GetNumericString(), actual.Message);
        }

        [Fact]
        public async Task IsValidCode_ShouldReturnFalse()
        {
            // Arrange
                

            SignerTokenMapping signerToken = new SignerTokenMapping();
            Signer signer = new Signer()
            {
                SignerAuthentication = new SignerAuthentication()
                {
                    OtpDetails = new OtpDetails()
                    {
                        Code = "code",
                        ExpirationTime = new DateTime(2023,10,2),
                    }
                }
            };
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Signers = new List<Signer>()
                {
                    signer
                }
            };

            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerToken);
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _daterMock.Setup(x => x.UtcNow()).Returns(DateTime.Today);

            // Act
            var actual = await _otpHandler.IsValidCode("");

            // Assert
            Assert.False(actual.Item1);
        }

        [Fact]
        public async Task IsValidCode_ShouldReturnTrue_Success()
        {
            // Arrange
            

            SignerTokenMapping signerToken = new SignerTokenMapping();
            Signer signer = new Signer()
            {
                SignerAuthentication = new SignerAuthentication()
                {
                    OtpDetails = new OtpDetails()
                    {
                        Code = "code",
                        ExpirationTime =  DateTime.UtcNow.AddDays(1),
                    }
                }
            };
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Signers = new List<Signer>()
                {
                    signer
                }
            };

            _signerTokenMappingConnectorMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(signerToken);
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);
            _documentCollectionConnectorMock.Setup(x => x.Read(It.IsAny<DocumentCollection>())).ReturnsAsync(documentCollection);
            _daterMock.Setup(x => x.UtcNow()).Returns(DateTime.Today);

            // Act
            var actual =await _otpHandler.IsValidCode("code");

            // Assert
            Assert.True(actual.Item1);
        }
        #endregion

    }
}
