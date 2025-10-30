using Common.Enums;
using Common.Interfaces;
using Common.Interfaces.FileGateScanner;
using Common.Models;
using Common.Models.FileGateScanner;
using Common.Models.Settings;
using FluentValidation;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using WeSign.Models.Templates;

namespace WeSign.Validators.TemplateValidators
{
    public class MergeTemplatesValidator : AbstractValidator<MergeTemplatesDTO>
    {
        private IFileGateScannerProviderFactory _fileGateScannerProviderHandler;
        private IDataUriScheme _dataUriScheme;
        private const double BASE_64_TO_FILE_SIZE = 0.7;
        //Support 10 MB file size
        //TODO get it from app setting 
        private int MAX_FILE_SIZE = 10485760;

        public MergeTemplatesValidator(IDataUriScheme dataUriScheme, IFileGateScannerProviderFactory fileGateScannerProviderHandler,
             IOptions<GeneralSettings> generalSettings) {
            _fileGateScannerProviderHandler = fileGateScannerProviderHandler;
            _dataUriScheme = dataUriScheme;
            if (generalSettings.Value.MaxUploadFileSize > 0)
            {
                MAX_FILE_SIZE = generalSettings.Value.MaxUploadFileSize;
            }
            RuleFor(x => x.Name)
               .NotEmpty().WithMessage("Please specify a Name");
            RuleFor(x => x.Templates).Must(x => x != null && x.Count() < 0 && x.Count() > 5).WithMessage("Templates list cant be empty and can't be grater then 5 ");
            RuleFor(x => x.Templates).Must(BeValidBase64String).WithMessage("Supported FileType are: PDF, DOCX, PNG, JPG , JPEG. Please specify a valid Base64File in format data:application/FILE_TYPE;base64,....");

            RuleFor(x => x.Templates)
               .Must(BeValidFileSize)
               .WithMessage($"File size limit to {MAX_FILE_SIZE / 1000000} MG");
            RuleFor(x => x.Templates)
                .Must(BeValidAndCleanFile)
                .WithMessage($"Invalid file content");


         
        }

        private bool BeValidAndCleanFile(string[] templetes)
        {
            foreach (var templete in templetes)
            {
                bool isValid = Guid.TryParse(templete, out _);
                if (!isValid)
                {
                    var fileGateScannerProviderHandler = _fileGateScannerProviderHandler.ExecuteCreation(FileGateScannerProviderType.None);
                    FileGateScan fileGateScan = new FileGateScan { Base64 = templete };
                    if (!fileGateScannerProviderHandler.Scan(fileGateScan).IsValid)
                    {
                        return false;
                    }
                }

            }
            return true;
        }

        private bool BeValidFileSize(string[] templetes)
        {
            foreach (var templete in templetes)
            {
                bool isValid = Guid.TryParse(templete, out _);
                if (!isValid)
                {
                    if (MAX_FILE_SIZE >= BASE_64_TO_FILE_SIZE * templete.Length)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool BeValidBase64String(string[] templetes)
        {

            foreach (var templete in templetes)
            {
                bool isValid = Guid.TryParse(templete, out _);
                if (!isValid)
                {
                   // check if id template ID or base64 file.
                    bool isValidWord = _dataUriScheme.IsOctetStreamIsValidWord(templete, out FileType wordType);
                    if ((_dataUriScheme.IsValidFile(templete) && _dataUriScheme.IsValidFileType(templete, out FileType fileType)) ||
                    (_dataUriScheme.IsValidImage(templete) && _dataUriScheme.IsValidImageType(templete, out ImageType imageType) ||
                           (isValidWord && (wordType == FileType.DOCX || wordType == FileType.DOC))))
                    {
                        return false;
                    }
                }
            }
            return true;
            
        }
    }
}
