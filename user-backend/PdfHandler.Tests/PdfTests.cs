namespace PdfHandler.Tests
{
    using Common.Enums.PDF;
    using Common.Interfaces;
    using Common.Interfaces.Files;
    using Common.Interfaces.PDF;
    using Common.Models.Settings;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Options;
    using Moq;
    using PdfHandler.Enums;
    using PdfHandler.Interfaces;
    using System;
    using System.IO;
    using Xunit;

    public class PdfTests : IDisposable
    {
        private const string NAME = "name";
        private const string DESCRIPTION = "description";
        private const double DOUBLE = 0.1;
        private const string VALUE = "value";
        private const string CHECKBOX_CHECKED = "Yes";
        private PdfStub _pdf;
        private IOptions<GeneralSettings> _generalSettingsMock;
        private Mock<IDebenuPdfLibrary> _debenuMock;
        private readonly Mock<IMemoryCache> _memoryCacheMock;
        private readonly Mock<IFilesWrapper> _fileWrapperMock;

        public PdfTests()
        {
            _generalSettingsMock = Options.Create(new GeneralSettings() { DPI = 1 });
            _debenuMock = new Mock<IDebenuPdfLibrary>();
            _memoryCacheMock = new Mock<IMemoryCache>();
            _fileWrapperMock = new Mock<IFilesWrapper>();
            _pdf = new PdfStub(_generalSettingsMock, _debenuMock.Object,
                _memoryCacheMock.Object,
                _fileWrapperMock.Object) ;
        }

        public void Dispose()
        {
            _debenuMock.Invocations.Clear();
        }

        [Fact]
        public void Load_ValidFile_Success()
        {
            LoadPdf(true);
        }

        [Fact]
        public void Load_InvalidFile_ReturnFalse()
        {
            LoadPdf(false);
        }

        [Fact]
        public void GetAllFields_TextField_ReturnTextResult()
        {
            LoadPdf(true);
            GetAllFieldsSetup(DebenuFieldType.TextField);

            var result = _pdf.GetAllFields();

            Assert.Single( result.TextFields);
            Assert.Equal(DESCRIPTION, result.TextFields[0].Description);
            Assert.Equal(DOUBLE, result.TextFields[0].Height);
            Assert.Equal(DOUBLE, result.TextFields[0].Width);
            Assert.Equal(DOUBLE, result.TextFields[0].X);
            Assert.Equal(DOUBLE, result.TextFields[0].Y);
            Assert.True(result.TextFields[0].Mandatory);
            Assert.Equal(NAME, result.TextFields[0].Name);
            Assert.Equal(1, result.TextFields[0].Page);
            Assert.Equal(TextFieldType.Text, result.TextFields[0].TextFieldType);
        }

        [Fact]
        public void GetAllFields_Choice_ReturnChoiceResult()
        {
            LoadPdf(true);
            GetAllFieldsSetup(DebenuFieldType.ChoiceField);

            var result = _pdf.GetAllFields();

            Assert.Single(result.ChoiceFields);
            Assert.Equal(DESCRIPTION, result.ChoiceFields[0].Description);
            Assert.Equal(DOUBLE, result.ChoiceFields[0].Height);
            Assert.Equal(DOUBLE, result.ChoiceFields[0].Width);
            Assert.Equal(DOUBLE, result.ChoiceFields[0].X);
            Assert.Equal(DOUBLE, result.ChoiceFields[0].Y);
            Assert.True(result.ChoiceFields[0].Mandatory);
            Assert.Equal(NAME, result.ChoiceFields[0].Name);
            Assert.Equal(1, result.ChoiceFields[0].Page);
            Assert.Equal(NAME, result.ChoiceFields[0].Options[0]);
            Assert.Equal(VALUE, result.ChoiceFields[0].SelectedOption);
        }

        [Fact]
        public void GetAllFields_CheckBox_ReturnCheckBoxResult()
        {
            LoadPdf(true);
            GetAllFieldsSetup(DebenuFieldType.CheckBoxField);
            _debenuMock.Setup(x => x.GetFormFieldValue(It.IsAny<int>())).Returns(CHECKBOX_CHECKED);

            var result = _pdf.GetAllFields();

            Assert.Single(result.CheckBoxFields);
            Assert.Equal(DESCRIPTION, result.CheckBoxFields[0].Description);
            Assert.Equal(DOUBLE, result.CheckBoxFields[0].Height);
            Assert.Equal(DOUBLE, result.CheckBoxFields[0].Width);
            Assert.Equal(DOUBLE, result.CheckBoxFields[0].X);
            Assert.Equal(DOUBLE, result.CheckBoxFields[0].Y);
            Assert.True(result.CheckBoxFields[0].Mandatory);
            Assert.Equal(NAME, result.CheckBoxFields[0].Name);
            Assert.Equal(1, result.CheckBoxFields[0].Page);
        }

        [Fact]
        public void GetAllFields_RadioGroup_ReturnRadioGroupResult()
        {
            LoadPdf(true);
            GetAllFieldsSetup(DebenuFieldType.RadioGroupField);

            var result = _pdf.GetAllFields();

            Assert.Single(result.RadioGroupFields);
            Assert.Equal(VALUE, result.RadioGroupFields[0].SelectedRadioName);
            Assert.Equal(NAME, result.RadioGroupFields[0].Name);
            Assert.Equal(DESCRIPTION, result.RadioGroupFields[0].RadioFields[0].Description);
            Assert.Equal(DOUBLE, result.RadioGroupFields[0].RadioFields[0].Height);
            Assert.Equal(DOUBLE, result.RadioGroupFields[0].RadioFields[0].Width);
            Assert.Equal(DOUBLE, result.RadioGroupFields[0].RadioFields[0].X);
            Assert.Equal(DOUBLE, result.RadioGroupFields[0].RadioFields[0].Y);
            Assert.True(result.RadioGroupFields[0].RadioFields[0].Mandatory);
            Assert.Equal(NAME, result.RadioGroupFields[0].RadioFields[0].Name);
            Assert.Equal(1, result.RadioGroupFields[0].RadioFields[0].Page);
            Assert.Equal(NAME, result.RadioGroupFields[0].RadioFields[0].Value);
        }

        [Fact]
        public void GetAllFields_SignatureField_ReturnSignatureFieldResult()
        {
            LoadPdf(true);
            GetAllFieldsSetup(DebenuFieldType.SignatureField);

            var result = _pdf.GetAllFields();

            Assert.Single(result.SignatureFields);
            Assert.Equal(DESCRIPTION, result.SignatureFields[0].Description);
            Assert.Equal(DOUBLE, result.SignatureFields[0].Height);
            Assert.Equal(DOUBLE, result.SignatureFields[0].Width);
            Assert.Equal(DOUBLE, result.SignatureFields[0].X);
            Assert.Equal(DOUBLE, result.SignatureFields[0].Y);
            Assert.True(result.SignatureFields[0].Mandatory);
            Assert.Equal(NAME, result.SignatureFields[0].Name);
            Assert.Equal(1, result.SignatureFields[0].Page);
            Assert.Equal(SignatureFieldType.Graphic, result.SignatureFields[0].SigningType);
        }

        [Fact]
        public void GetAllFields_NoLoad_ThrowException()
        {
            LoadPdf(false);

            var ex = Assert.Throws<FileLoadException>(() => _pdf.GetAllFields());
        }

        // FIX 
        //[Fact]
        //public void Images_ValidImages_ReturnImages()
        //{
        //    string smallImage = "R0lGODlhAQABAIAAAP///////yH5BAEKAAEALAAAAAABAAEAAAICTAEAOw==";
        //    LoadPdf(true);
        //    _debenuMock.Setup(x => x.PageCount()).Returns(1);
        //    _debenuMock.Setup(x => x.RenderPageToString(It.IsAny<double>(), 
        //        It.IsAny<int>(), It.IsAny<int>())).Returns(Convert.FromBase64String(smallImage));
            
        //    var images = _pdf.Images;

        //    Assert.Equal(1, images.Count);
        //}

      

        [Fact]
        public void Signatures_ValidSignature_ReturnSignaturesResult()
        {
            LoadPdf(true);
            GetAllFieldsSetup(DebenuFieldType.SignatureField);

            var signatures = _pdf.SignatureFields;

            Assert.Single( signatures);
        }

        [Fact]
        public void Signatures_ValidSignatureWithName_ReturnSignaturesResult()
        {
            LoadPdf(true);
            GetAllFieldsSetup(DebenuFieldType.SignatureField);
            _debenuMock.Setup(x => x.GetFormFieldTitle(It.IsAny<int>())).Returns(string.Empty);

            var signatures = _pdf.SignatureFields;

            Assert.Single(signatures);
            Assert.Equal("Signature_1", signatures[0].Name);
        }

        private void GetAllFieldsSetup(DebenuFieldType fieldType, int fieldsCount = 1)
        {
            _debenuMock.Setup(x => x.SetOrigin(It.IsAny<int>())).Returns(0);
            _debenuMock.Setup(x => x.FormFieldCount()).Returns(fieldsCount);
            _debenuMock.Setup(x => x.GetFormFieldType(It.IsAny<int>())).Returns((int)fieldType);
            _debenuMock.Setup(x => x.GetFormFieldPage(It.IsAny<int>())).Returns(1);
            _debenuMock.Setup(x => x.SelectPage(It.IsAny<int>())).Returns(0);
            _debenuMock.Setup(x => x.GetFormFieldTitle(It.IsAny<int>())).Returns(NAME);
            _debenuMock.Setup(x => x.GetFormFieldRequired(It.IsAny<int>())).Returns(1);
            _debenuMock.Setup(x => x.GetFormFieldDescription(It.IsAny<int>())).Returns(DESCRIPTION);
            _debenuMock.Setup(x => x.GetFormFieldBound(It.IsAny<int>(), It.IsAny<int>())).Returns(DOUBLE);
            _debenuMock.Setup(x => x.PageWidth()).Returns(1);
            _debenuMock.Setup(x => x.PageHeight()).Returns(1);
            _debenuMock.Setup(x => x.GetFormFieldValue(It.IsAny<int>())).Returns(VALUE);

            _debenuMock.Setup(x => x.GetFormFieldSubCount(It.IsAny<int>())).Returns(2);
            _debenuMock.Setup(x => x.GetFormFieldSubName(It.IsAny<int>(), It.IsAny<int>())).Returns(NAME);
        }

        private void LoadPdf(bool valid)
        {
            _debenuMock.Setup(x => x.LoadFromString(It.IsAny<byte[]>(), It.IsAny<string>())).Returns(valid ? 1 : 0);

            bool loadResult = _pdf.Load(default);

            Assert.True(valid ? loadResult : !loadResult);
        }
    }
}
