namespace WeSign.Validators.TemplateValidators
{
    using Common.Enums;
    using Common.Interfaces;
    using Common.Interfaces.FileGateScanner;
    using Common.Models.FileGateScanner;
    using Common.Models.Settings;
    using Common.Models.XMLModels;
    using FluentValidation;
    using Microsoft.Extensions.Options;
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using WeSign.Models.Templates;

    public class CreateTemplateValidator : AbstractValidator<CreateTemplateDTO>
    {
        private const double BASE_64_TO_FILE_SIZE = 0.7;
        //Support 10 MB file size
        //TODO get it from app setting 
        private  int MAX_FILE_SIZE = 10485760;

        private readonly IDataUriScheme _dataUriScheme;
        private readonly IXmlHandler<PDFMetaData> _xmlHandler;
        private readonly IFileGateScannerProviderFactory _fileGateScannerProviderHandler;

        public CreateTemplateValidator(IDataUriScheme dataUriScheme, IXmlHandler<PDFMetaData> xmlHandler,
                                       IOptions<GeneralSettings> generalSettings, IFileGateScannerProviderFactory fileGateScannerProviderHandler )
        {
            _dataUriScheme = dataUriScheme;
            _xmlHandler = xmlHandler;
            _fileGateScannerProviderHandler = fileGateScannerProviderHandler;

            if (generalSettings.Value.MaxUploadFileSize > 0 )
            {
                MAX_FILE_SIZE = generalSettings.Value.MaxUploadFileSize;
            }
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Please specify a Name");
            RuleFor(x => x.Base64File)
                .NotEmpty().WithMessage("Please specify a Base64File");
            RuleFor(x => x.Base64File)
                .Must(BeValidBase64String)
                .WithMessage("Supported FileType are: PDF, DOCX, PNG, JPG , JPEG. Please specify a valid Base64File in format data:application/FILE_TYPE;base64,.... ");
            RuleFor(x => x.Base64File)
                .Must(BeValidFileSize)
                .WithMessage($"File size limit to {MAX_FILE_SIZE / 1000000} MG");
            RuleFor(x => x.Base64File)
                .Must(BeValidAndCleanFile)
                .WithMessage($"Invalid file content");
            //Metadata validator
            RuleFor(x => x.MetaData).Must(BeValidXML).WithMessage("Invalid xml");
        }

        private bool BeValidAndCleanFile(string base64string)
        {
            var fileGateScannerProviderHandler = _fileGateScannerProviderHandler.ExecuteCreation(FileGateScannerProviderType.None);
            FileGateScan fileGateScan = new FileGateScan { Base64 = base64string };
            bool isValid = fileGateScannerProviderHandler.Scan(fileGateScan).IsValid;

            return isValid;
        }

        private bool BeValidXML(string xmlBase64)
        {
            try
            {
                if (string.IsNullOrEmpty(xmlBase64))
                {
                    return true;
                }
                byte[] data = _dataUriScheme.GetBytes(xmlBase64);
                using (var stream = new MemoryStream(data))
                {
                    return XDocument.Load(stream) != null && _xmlHandler.ConvertBase64ToModel(xmlBase64) != null;
                }
            }
            catch 
            {
                return false;
            }

        }

        private bool IsValidFileType(string base64string)
        {
            return _dataUriScheme.IsValidFileType(base64string, out FileType fileType);
        }

        private bool BeValidBase64String(string base64string)
        {           
            bool isValidWord = _dataUriScheme.IsOctetStreamIsValidWord(base64string, out FileType wordType);
            return (_dataUriScheme.IsValidFile(base64string) && _dataUriScheme.IsValidFileType(base64string, out FileType fileType)) ||
                   (_dataUriScheme.IsValidImage(base64string) && _dataUriScheme.IsValidImageType(base64string, out ImageType imageType) ||
                   (isValidWord && (wordType == FileType.DOCX || wordType == FileType.DOC)));
        }

        private bool BeValidFileSize(string base64string)
        {
            return MAX_FILE_SIZE >= BASE_64_TO_FILE_SIZE * base64string.Length;
        }
    }
}
