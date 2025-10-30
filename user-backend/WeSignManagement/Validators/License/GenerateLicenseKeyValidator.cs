using FluentValidation;
using FluentValidation.Validators;
using System;
using WeSignManagement.Models.License;

namespace WeSignManagement.Validators.License
{
    public class GenerateLicenseKeyValidator :  AbstractValidator<UserInfoDTO>
    {
        public GenerateLicenseKeyValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Please specify an Id")
                .Must(BeValidID).WithMessage("Please specify valid id number");
            RuleFor(x => x.Company).NotEmpty().WithMessage("Please specify an Company");
            RuleFor(x => x.Name).NotEmpty().WithMessage("Please specify an Name");
            RuleFor(x => x.Phone).NotEmpty().WithMessage("Please specify an Phone");
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Please specify an Email")
                .EmailAddress(EmailValidationMode.Net4xRegex).WithMessage("Please specify a valid Email");
        }

        private bool BeValidID(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Length != 9)
            {
                return false;
            }

            int counter = 0;
            for (int i = 0; i < 9; i++)
            {
                int incNum = int.Parse(id[i].ToString());
                incNum *= (i % 2) + 1;
                if (incNum > 9)
                    incNum -= 9;
                counter += incNum;
            }
            return (counter % 10 == 0);
        }
    }
}

