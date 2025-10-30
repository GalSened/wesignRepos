using Common.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Options;
using PdfExternalService.Models;
using PdfExternalService.Models.DTO;

namespace PdfExternalService.Validators
{
    public class FileMergeValidator : AbstractValidator<FileMergeDTO>
    {
        private readonly PDFExternalGeneralSettings _generalSettings;
        private readonly IDataUriScheme _dataUriScheme;
        private const double BASE_64_TO_FILE_SIZE = 0.7;

        public FileMergeValidator(IOptions<PDFExternalGeneralSettings> generalSettings, IDataUriScheme dataUriScheme)
        {
            _generalSettings = generalSettings.Value;
            _dataUriScheme = dataUriScheme;

            RuleForEach(x => x.Base64Files)
            .NotEmpty().WithMessage("Please specify a Base64File");
            RuleForEach(x => x.Base64Files)
                .Must(BeValidBase64String)
                .WithMessage("Supported FileType is PDF valid Base64File");
            RuleForEach(x => x.Base64Files)
                .Must(BeValidFileSize)
                .WithMessage($"File size limit to {_generalSettings.MaxFileSize / 1000000} MG");
            RuleFor(x => x.Base64Files).Must(BeUnderMaxFileAmount)
                .WithMessage($"Amount of allowed files is up to {_generalSettings.MaxMergeFiles}");

            RuleFor(x => x.Base64Files).Must(BeUnderMinFileAmount)
              .WithMessage($"Missing Files to merge");


        }

        private bool BeUnderMinFileAmount(List<string> pdfs)
        {
            if( pdfs.Count() < 2)
            {
                return false;
            } 
            return true;
                
        }

        private bool BeValidBase64String(string pdfFile)
        {
            try
            {
                var bytes = Convert.FromBase64String(pdfFile);

                return pdfFile.StartsWith("JVBER");
                
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool BeValidFileSize(string base64string)
        {
            return _generalSettings.MaxFileSize >= BASE_64_TO_FILE_SIZE * base64string.Length;
        }

        private bool BeUnderMaxFileAmount(List<string> base64Strings)
        {
            return base64Strings.Count <= _generalSettings.MaxMergeFiles;
        }

    }
}
