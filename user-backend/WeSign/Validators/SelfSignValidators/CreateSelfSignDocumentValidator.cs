using Common.Enums;
using Common.Interfaces;
using Common.Models.Settings;
using FluentValidation;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeSign.Models.SelfSign;

namespace WeSign.Validators.SelfSignValidators
{
    public class CreateSelfSignDocumentValidator : AbstractValidator<CreateSelfSignDocumentDTO>
    {
        private const double BASE_64_TO_FILE_SIZE = 0.7;
        private int MAX_FILE_SIZE = 10485760;
        private readonly IDataUriScheme _dataUriScheme;

        public CreateSelfSignDocumentValidator(IDataUriScheme dataUriScheme, IOptions<GeneralSettings> generalSettings)
        {
            _dataUriScheme = dataUriScheme;
            if (generalSettings.Value.MaxUploadFileSize > 0)
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
