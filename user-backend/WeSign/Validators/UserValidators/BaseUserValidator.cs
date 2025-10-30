using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeSign.Models.Users;

namespace WeSign.Validators.UserValidators
{
    public class BaseUserValidator : AbstractValidator<BaseUserDTO>
    {
        public BaseUserValidator()
        {
            RuleFor(x => x.Email)
                .NotNull()
                .NotEmpty()
                .EmailAddress(EmailValidationMode.Net4xRegex)
                .WithMessage("Please insert valid email");
        }
    }
}
