// Ignore Spelling: Doesnt

using Common.Interfaces;
using Common.Interfaces.DB;
using Common.Interfaces.FileGateScanner;
using Common.Interfaces.PDF;
using Common.Models;
using Common.Models.Configurations;
using Common.Models.Documents;
using Common.Models.Documents.Signers;
using Common.Models.FileGateScanner;
using Common.Models.Files.PDF;
using Moq;
using SignerBL.Handlers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SignerBL.Tests
{
    public class ValidatorHandlerTests : IDisposable
    {
        
        private readonly Mock<IJWT> _jwtMock;
        private readonly Mock<IDocumentPdf> _documentPdfMock;
        private readonly Mock<IFileGateScannerProviderFactory> _fileGateScannerProviderHandlerMock;
        private readonly Mock<IFileGateScannerProvider> _fileGateScannerPorivderMock;
        private Common.Interfaces.SignerApp.ISignerValidator _validatorHandler;

        private readonly Mock<ISignerTokenMappingConnector> _signerTokenMappingConnectoMock;
        private readonly Mock<IConfigurationConnector> _configurationConnectorMock;
        public ValidatorHandlerTests()
        {
            _signerTokenMappingConnectoMock = new Mock<ISignerTokenMappingConnector>();
            _configurationConnectorMock = new Mock<IConfigurationConnector>();
            _jwtMock = new Mock<IJWT>();
            _documentPdfMock = new Mock<IDocumentPdf>();
            _fileGateScannerProviderHandlerMock = new Mock<IFileGateScannerProviderFactory>();
            _fileGateScannerPorivderMock = new Mock<IFileGateScannerProvider>();
            _validatorHandler = new SignerValidatorHandler(_signerTokenMappingConnectoMock.Object, _configurationConnectorMock.Object, _jwtMock.Object, _documentPdfMock.Object, _fileGateScannerProviderHandlerMock.Object);
        }

        public void Dispose()
        {
            _signerTokenMappingConnectoMock.Invocations.Clear();
            _configurationConnectorMock.Invocations.Clear();
            _jwtMock.Invocations.Clear();
            _documentPdfMock.Invocations.Clear();
            _fileGateScannerProviderHandlerMock.Invocations.Clear();
            _fileGateScannerPorivderMock.Invocations.Clear();
        }


        #region AreAllFieldsBelongToSigner

        [Fact]
        public void AreAllFieldsBelongToSigner_WithoutSignerFields_ShouldReturnTrue()
        {
            // Arrange
            Signer dbSigner = new Signer();
            Signer Signer = new Signer();
            DocumentCollection documentCollection = new DocumentCollection();

            // Action 
            var action = _validatorHandler.AreAllFieldsBelongToSigner(dbSigner, Signer, documentCollection);

            // Assert
            Assert.True(action);
        }


        [Fact]
        public void AreAllFieldsBelongToSigner_AllFieldsBelongToOriginalSigner_ShouldReturnTrue()
        {
            // Initiate a signer with a list of signer fields
            // Arrange
            SignerField signerField = new SignerField { DocumentId = new Guid(), FieldName = "testname" };
            List<SignerField> signerFields = new List<SignerField>() { signerField };
            Signer signer = new Signer() { SignerFields = signerFields };

            Signer dbsigner = new Signer();
            DocumentCollection documentCollection = new DocumentCollection();

            TextField textField = new TextField { Name = "testname", Description = "Description" };
            List<TextField> textFields = new List<TextField>() { textField };
            _documentPdfMock.Setup(x => x.GetAllFields(It.IsAny<bool>())).Returns(new PDFFields { TextFields = textFields });

            // Action
            var action = _validatorHandler.AreAllFieldsBelongToSigner(dbsigner, signer, documentCollection);

            // Assert
            Assert.True(action);
        }

        [Fact]
        public void AreAllFieldsBelongToSigner_AFieldsBelongToDBSigner_ShouldReturnTrue()
        {

            // Arrange
            SignerField signerField = new SignerField { DocumentId = new Guid(), FieldName = "testName" };
            List<SignerField> signerFields = new List<SignerField>() { signerField };
            Signer signer = new Signer() { SignerFields = signerFields };

            TextField textField = new TextField { Name = "anotherName", Description = "anotherName" };
            List<TextField> textFields = new List<TextField>() { textField };
            _documentPdfMock.Setup(x => x.GetAllFields(It.IsAny<bool>())).Returns(new PDFFields { TextFields = textFields });

            SignerField DbSignerField = new SignerField { FieldName = "testName" };
            List<SignerField> dbSignerFields = new List<SignerField>() { DbSignerField };
            Signer dbsigner = new Signer() { SignerFields = dbSignerFields };

            RadioGroupField radioGroupField = new RadioGroupField() { Name = "radioGroupField" };
            PDFFields pdfFields = new PDFFields() { RadioGroupFields = new List<RadioGroupField>() { radioGroupField } };
            Document doc = new Document() { Fields = pdfFields };
            List<Document> documents = new List<Document>() { doc };
            DocumentCollection documentCollection = new DocumentCollection() { Documents = documents };


            // Action
            var action = _validatorHandler.AreAllFieldsBelongToSigner(dbsigner, signer, documentCollection);

            // Assert
            Assert.True(action);
        }

        [Fact]
        public void AreAllFieldsBelongToSigner_AFieldBelongsToDocumentCollection_ShouldReturnTrue()
        {

            // Arrange
            SignerField signerField = new SignerField { DocumentId = new Guid(), FieldName = "testName" };
            List<SignerField> signerFields = new List<SignerField>() { signerField };
            Signer signer = new Signer() { SignerFields = signerFields };

            TextField textField = new TextField { Name = "anotherName", Description = "anotherName" };
            List<TextField> textFields = new List<TextField>() { textField };
            _documentPdfMock.Setup(x => x.GetAllFields(It.IsAny<bool>())).Returns(new PDFFields { TextFields = textFields });

            SignerField DbSignerField = new SignerField { FieldName = "anotherName" };
            List<SignerField> dbSignerFields = new List<SignerField>() { DbSignerField };
            Signer dbsigner = new Signer() { SignerFields = dbSignerFields };

            RadioGroupField radioGroupField = new RadioGroupField() { Name = "testName" };
            PDFFields pdfFields = new PDFFields() { RadioGroupFields = new List<RadioGroupField>() { radioGroupField } };
            Document doc = new Document() { Fields = pdfFields };
            List<Document> documents = new List<Document>() { doc };
            DocumentCollection documentCollection = new DocumentCollection() { Documents = documents };


            // Action
            var action = _validatorHandler.AreAllFieldsBelongToSigner(dbsigner, signer, documentCollection);

            // Assert
            Assert.True(action);
        }

        //TODO: uncomment test after validator handler fix
        //[Fact]
        //public void AreAllFieldsBelongToSigner_FieldDoesntBelongToSigner_ShouldReturnFalse()
        //{

        //    // Arrange
        //    SignerField signerField = new SignerField { DocumentId = new Guid(), FieldName = "testName" };
        //    List<SignerField> signerFields = new List<SignerField>() { signerField };
        //    Signer signer = new Signer() { SignerFields = signerFields };

        //    TextField textField = new TextField { Name = "anotherName", Description = "anotherName" };
        //    List<TextField> textFields = new List<TextField>() { textField };
        //    _documentPdfMock.Setup(x => x.GetAllFields()).Returns(new PDFFields { TextFields = textFields });

        //    SignerField DbSignerField = new SignerField { FieldName = " dbSignedField" };
        //    List<SignerField> dbSignerFields = new List<SignerField>() { DbSignerField };
        //    Signer dbsigner = new Signer() { SignerFields = dbSignerFields };

        //    RadioGroupField radioGroupField = new RadioGroupField() { Name = "radioGroupField" };
        //    PDFFields pdfFields = new PDFFields() { RadioGroupFields = new List<RadioGroupField>() { radioGroupField } };
        //    Common.Models.Documents.Document doc = new Common.Models.Documents.Document() { Fields = pdfFields };
        //    List<Common.Models.Documents.Document> documents = new List<Common.Models.Documents.Document>() { doc };
        //    DocumentCollection documentCollection = new DocumentCollection() { Documents = documents };


        //    // Action
        //    var action = _validatorHandler.AreAllFieldsBelongToSigner(dbsigner, signer, documentCollection);

        //    // Assert
        //    Assert.False(action);
        //}



        #endregion

        #region AreAllFieldsExistsInDocuments

        [Fact]
        public void AreAllFieldsExistsInDocuments_WhenFieldDoesntExist_ShouldReturnFalse()
        {

            // Arrange
            PDFFields fields = new PDFFields() { TextFields = new List<TextField>() { new TextField() } };
            Document document = new Document() { Fields = fields };
            List<Document> documents = new List<Document>() { document };
            DocumentCollection documentCollection = new DocumentCollection() { Documents = documents };
            _documentPdfMock.Setup(x => x.GetAllFields(It.IsAny<bool>())).Returns(new PDFFields());
            _documentPdfMock.Setup(x => x.Load(It.IsAny<Guid>(), true)).Returns(true);

            _documentPdfMock.Setup(x => x.IsExists(It.IsAny<IBaseField>())).Returns(false);

            // Action 

            var action = _validatorHandler.AreAllFieldsExistsInDocuments(documentCollection);

            // Assert

            Assert.False(action);

        }

        // True Case Not Possible To Test Due to Extended List



        #endregion

        #region AreAllFieldsExistsInDocuments


        // TODO - Create Interface from Pdf abstract class in order to mock an ExtendedList, Cannot be properly tested otherwise 
        // (unless we add a bunch of irrelevant (to the class) mocks.

        #endregion

        #region AreAllMandatortyFieldsFilledIn 
        [Fact]
        public void AreAllMandatoryFieldsFilledIn_AMandatoryFieldIsNotFilledIn_ShouldReturnFalse()
        {
            // Arrange

            SignerField signerField = new SignerField { FieldName = "testname", FieldValue = "" };
            List<SignerField> signerFields = new List<SignerField>() { signerField };

            SignerField DbsignerField = new SignerField { FieldName = "testname", IsMandatory = true };
            List<SignerField> DbsignerFields = new List<SignerField>() { DbsignerField };

            Signer signer = new Signer() { SignerFields = signerFields };
            Signer dbSigner = new Signer() { SignerFields = DbsignerFields };

            // Action
            var action = _validatorHandler.AreAllMandatoryFieldsFilledIn(dbSigner, signer);

            // Assert 
            Assert.False(action);

        }

        [Fact]
        public void AreAllMandatoryFieldsFilledIn_AllMandatoryFieldsAreFilledIn_ShouldReturnTrue()
        {

            // Arrange 

            SignerField signerField = new SignerField { FieldName = "testname", FieldValue = "value" };
            List<SignerField> signerFields = new List<SignerField>() { signerField };

            SignerField DbsignerField = new SignerField { FieldName = "testname", IsMandatory = true };
            List<SignerField> DbsignerFields = new List<SignerField>() { DbsignerField };

            Signer signer = new Signer() { SignerFields = signerFields };
            Signer dbSigner = new Signer() { SignerFields = DbsignerFields };

            // Action
            var action = _validatorHandler.AreAllMandatoryFieldsFilledIn(dbSigner, signer);

            // Assert 
            Assert.True(action);
        }

        [Fact]
        public void AreAllMandatoryFieldsFilledIn_WithoutDbSignerFields_ShouldReturnTrue()
        {
            // Arrange 


            Signer signer = new Signer();
            Signer dbSigner = new Signer();

            // Action
            var action = _validatorHandler.AreAllMandatoryFieldsFilledIn(dbSigner, signer);

            // Assert 
            Assert.True(action);
        }


        #endregion

        #region AreDocumentsBelongToDocumentCollection
        [Fact]
        public void AreDocumentsBelongToDocumentCollection_AllDocumentsBelongToCollection_ShouldReturnTrue()
        {
            Guid sampleID = Guid.Parse("00000000-0000-0000-0000-000000000001");

            // Arrange
            Common.Models.Documents.Document doc = new Common.Models.Documents.Document() { Id = sampleID };
            Common.Models.Documents.Document Dbdoc = new Common.Models.Documents.Document() { Id = sampleID };

            List<Common.Models.Documents.Document> documents = new List<Common.Models.Documents.Document>() { doc };
            List<Common.Models.Documents.Document> Dbdocuments = new List<Common.Models.Documents.Document>() { Dbdoc };

            DocumentCollection documentCollection = new DocumentCollection() { Documents = documents };
            DocumentCollection dbDocumentCollection = new DocumentCollection() { Documents = Dbdocuments };

            // Action
            var action = _validatorHandler.AreDocumentsBelongToDocumentCollection(dbDocumentCollection, documentCollection);

            // Assert
            Assert.True(action);

        }

        [Fact]
        public void AreDocumentsBelongToDocumentCollection_NotAllDocumentsBelongToCollection_ShouldReturnFalse()
        {
            Guid sampleID = Guid.Parse("00000000-0000-0000-0000-000000000001");
            Guid DifferentID = Guid.Parse("00000000-0000-0000-0000-000000000002");


            // Arrange
            Common.Models.Documents.Document doc = new Common.Models.Documents.Document() { Id = sampleID };
            Common.Models.Documents.Document Dbdoc = new Common.Models.Documents.Document() { Id = DifferentID };

            List<Common.Models.Documents.Document> documents = new List<Common.Models.Documents.Document>() { doc };
            List<Common.Models.Documents.Document> Dbdocuments = new List<Common.Models.Documents.Document>() { Dbdoc };

            DocumentCollection documentCollection = new DocumentCollection() { Documents = documents };
            DocumentCollection dbDocumentCollection = new DocumentCollection() { Documents = Dbdocuments };

            // Action
            var action = _validatorHandler.AreDocumentsBelongToDocumentCollection(dbDocumentCollection, documentCollection);

            // Assert
            Assert.False(action);

        }

        [Fact]
        public void AreDocumentsBelongToDocumentCollection_inputCollectionIsEmpty_ShouldReturnTrue()
        {
            // Arrange

            Common.Models.Documents.Document Dbdoc = new Common.Models.Documents.Document() { Id = new Guid() };

            List<Common.Models.Documents.Document> Dbdocuments = new List<Common.Models.Documents.Document>() { Dbdoc };

            DocumentCollection documentCollection = new DocumentCollection();
            DocumentCollection dbDocumentCollection = new DocumentCollection() { Documents = Dbdocuments };

            // Action
            var action = _validatorHandler.AreDocumentsBelongToDocumentCollection(dbDocumentCollection, documentCollection);

            // Assert
            Assert.True(action);
        }
        #endregion

        #region ValidateSignerToken


        [Fact]
        public async Task ValidateSignerToken_WithValidSignerTokenMapping_ShouldChangeDocumentCollectionId()
        {
            // Arrange
            Guid sampleID = Guid.Parse("00000000-0000-0000-0000-000000000001");

            Guid documentCollectionId = Guid.Empty;
            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            Signer signer = new Signer();

            _signerTokenMappingConnectoMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(new SignerTokenMapping() { JwtToken = "token", DocumentCollectionId = sampleID });
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);


            // Action
            (var action, documentCollectionId) = await _validatorHandler.ValidateSignerToken(signerTokenMapping);

            // Assert
            Assert.Equal(sampleID, documentCollectionId);

        }

        [Fact]
        public async Task ValidateSignerToken_WithValidSignerTokenMapping_ShouldReturnSigner()
        {
            // Arrange
            Guid sampleID = Guid.Parse("00000000-0000-0000-0000-000000000001");

            Guid documentCollectionId = Guid.Empty;
            SignerTokenMapping signerTokenMapping = new SignerTokenMapping();
            Signer signer = new Signer();

            _signerTokenMappingConnectoMock.Setup(x => x.Read(It.IsAny<SignerTokenMapping>())).ReturnsAsync(new SignerTokenMapping() { JwtToken = "token", DocumentCollectionId = sampleID });
            _jwtMock.Setup(x => x.GetSigner(It.IsAny<string>())).Returns(signer);


            // Action
            (var action, documentCollectionId) =  await _validatorHandler.ValidateSignerToken(signerTokenMapping);

            // Assert
            Assert.Equal(signer, action);

        }

        #endregion

        #region AreAllSignersSigned 

        [Fact]
        public void AreAllSignersSigned_AllSignersAreSigned_ReturnsTrue()
        {
            //Arrange
            Signer singer = new Signer() { Status = Common.Enums.Contacts.SignerStatus.Signed };
            Signer signer2 = new Signer() { Status = Common.Enums.Contacts.SignerStatus.Signed };

            IEnumerable<Signer> Signers = new List<Signer>() { singer, signer2 };

            //Action
            var action = _validatorHandler.AreAllSignersSigned(Signers);

            // Assert
            Assert.True(action);

        }
        [Fact]
        public void AreAllSignersSigned_NotAllSignersAreSigned_ReturnsFalse()
        {
            //Arrange
            Signer singer = new Signer() { Status = Common.Enums.Contacts.SignerStatus.Signed };
            Signer signer2 = new Signer() { Status = Common.Enums.Contacts.SignerStatus.Rejected };

            IEnumerable<Signer> Signers = new List<Signer>() { singer, signer2 };

            //Action
            var action = _validatorHandler.AreAllSignersSigned(Signers);

            // Assert
            Assert.False(action);
        }
        [Fact]
        public void AreAllSignersSigned_SignersAreEmpty_ReturnsTrue()
        {
            //Arrange
            IEnumerable<Signer> Signers = new List<Signer>() { };

            //Action
            var action = _validatorHandler.AreAllSignersSigned(Signers);

            // Assert
            Assert.True(action);
        }
        #endregion

        #region ValidateIsCleanFile
        [Fact]
        public async Task ValidateIsCleanFile_WhenResultIsValid_ShouldReturnFileGateScan()
        {
            // Arrange

            string base64string = "";
            FileGateScannerConfiguration fileGateScannerConfiguration = new FileGateScannerConfiguration() { Provider = Common.Enums.FileGateScannerProviderType.None };
            _configurationConnectorMock.Setup(x => x.Read()).ReturnsAsync(new Configuration() { FileGateScannerConfiguration = fileGateScannerConfiguration });

            _fileGateScannerProviderHandlerMock.Setup(x => x.ExecuteCreation(It.IsAny<Common.Enums.FileGateScannerProviderType>())).Returns(_fileGateScannerPorivderMock.Object);
            _fileGateScannerPorivderMock.Setup(x => x.Scan(It.IsAny<FileGateScan>())).Returns(new FileGateScanResult() { IsValid = true });

            // Action 
            var action =await _validatorHandler.ValidateIsCleanFile(base64string);

            // Assert
            Assert.IsType<FileGateScanResult>(action);

        }

        #endregion

    }
}
