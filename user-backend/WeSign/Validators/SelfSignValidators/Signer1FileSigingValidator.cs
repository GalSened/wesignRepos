using Common.Enums;
using Common.Interfaces;
using Common.Models.Documents.Signers;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using WeSign.Models.SelfSign;

namespace WeSign.Validators.SelfSignValidators
{
    public class Signer1FileSigingValidator : AbstractValidator<Signer1FileSigingDTO>
    {
        private IDataUriScheme _dataUriScheme;
        private IFileSystem _fileSystem;

        public Signer1FileSigingValidator(IDataUriScheme dataUriScheme, IFileSystem fileSystem)
        {
            _dataUriScheme = dataUriScheme;
            _fileSystem = fileSystem;
            RuleFor(x => x.FileName).NotEmpty().NotNull().WithMessage("Please specify a File Name").
                Must(ValidFileName).WithMessage("File name is not valid or not supported");            
            RuleFor(x => x.Base64File).NotEmpty().WithMessage("Please specify a Base64File").
                Must(ValidBase64).WithMessage("File Data is not Valid");
            RuleFor(x => x.SigingFileType).IsInEnum().WithMessage("File Type Is not supported");
            RuleFor(x => x.Signer1Credential).Must(ValidSignerAuthentication).WithMessage("Missing Signer1 Authentication");
        }

        private bool ValidSignerAuthentication(Signer1Credential signerAuthentication)
        {
            if(signerAuthentication == null  )
            {
                return false;
            }
            if ((string.IsNullOrWhiteSpace( signerAuthentication.CertificateId) || string.IsNullOrWhiteSpace(signerAuthentication.Password)) &&
                string.IsNullOrWhiteSpace(signerAuthentication.SignerToken))
            {
                return false;
            }

            return true;
        }
        
        private bool ValidBase64(string base64file)
        {
            try
            {
                Convert.FromBase64String(base64file);
            }
            catch
            {
                return false;
            }
            return true;
        }

        private bool ValidFileName(string fileName)
        {
            if(string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }
            var ext = _fileSystem.Path.GetExtension(fileName).ToLower() ;

            return ext == ".docx" || ext == ".xml" || ext == ".xlsx" || ext == ".pdf";

        }
        
    }
}
