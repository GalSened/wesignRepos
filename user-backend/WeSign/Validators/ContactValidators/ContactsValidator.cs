using Common.Enums;
using Common.Enums.Documents;
using Common.Extensions;
using Common.Interfaces;
using Common.Models.CSV_Mapper;
using FluentValidation;
using Spire.Xls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WeSign.Models.Contacts;

namespace WeSign.Validators.ContactValidators
{
    public class ContactsValidator : AbstractValidator<ContactsDTO>
    {
        private readonly IDataUriScheme _dataUriScheme;
        private readonly ICsvHandler<ContactMapper> _csvHandler;
        private readonly Common.Interfaces.IValidator _validator;

        public ContactsValidator(IDataUriScheme dataUriScheme, ICsvHandler<ContactMapper> csvHandler, Common.Interfaces.IValidator validator)
        {
            _dataUriScheme = dataUriScheme;
            _csvHandler = csvHandler;
            _validator = validator;
            RuleFor(input => input.Base64File).NotEmpty().WithMessage("Please supply base64");
            RuleFor(input => input.Base64File).Must(BeValidBase64).WithMessage("Supported FileType are: XLSX && XLS. Please specify a valid Base64File in format data:application/FILE_TYPE;base64,.... ");
        }

 

        private bool BeValidBase64(string base64)
        {
            try
            {
                string content = _dataUriScheme.Getbase64Content(base64);
                bool isValidFileType = _dataUriScheme.IsValidFileType(base64, out FileType fileType);
                return !string.IsNullOrEmpty(content) && isValidFileType && (fileType == FileType.XLSX);

            }
            catch
            {
                return false;
            }
        }
    }
}
