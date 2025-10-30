using Common.Interfaces;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeSignManagement.Models.Users;

namespace WeSignManagement.Validators.UserValidators
{
    public class CreateHtmlTemplateValidator : AbstractValidator<CreateHtmlTemplateDTO>
    {
        private readonly IDataUriScheme _dataUriScheme;

        public CreateHtmlTemplateValidator(IDataUriScheme dataUriScheme)
        {
            _dataUriScheme = dataUriScheme;
            RuleFor(x => x.TemplateId).NotEmpty().WithMessage("Please specify template id");
            RuleFor(x => x.UserId).NotEmpty().WithMessage("Please specify user id");
            RuleFor(x => x.HtmlBase64File)
                .NotEmpty().WithMessage("Please specify HtmlBase64File")
                .Must(ValidBase64).WithMessage("HTML Data is not Valid")
                .Must(BeValidHtmlType).WithMessage("Invalid file type, expected HTML file"); 
            RuleFor(x => x.JSBase64File)
                .NotEmpty().WithMessage("Please specify JSBase64File")
                .Must(ValidBase64).WithMessage("JS Data is not Valid")
                .Must(BeValidJSType).WithMessage("Invalid file type, expected JS file"); 
        }

        private bool BeValidHtmlType(string base64file)
        {
            string first = base64file.Split(",").FirstOrDefault();
            return first.Contains("text/html");
        }

        private bool BeValidJSType(string base64file)
        {
            string first = base64file.Split(",").FirstOrDefault();
            return first.Contains("text/javascript");
        }

        private bool ValidBase64(string base64file)
        {
            try
            {                
                string content = _dataUriScheme.Getbase64Content(base64file);
                Convert.FromBase64String(content);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
