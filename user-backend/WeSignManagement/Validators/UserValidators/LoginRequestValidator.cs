using FluentValidation;
using FluentValidation.Validators;
using WeSignManagement.Models.Users;

namespace WeSignManagement.Validators.UserValidators
{
    public class LoginRequestValidator : AbstractValidator<LoginManagementRequestDTO>
    {
        public LoginRequestValidator()
        {
            RuleFor(x=>x.Email)
                .NotEmpty().WithMessage("Please specify an Email")
                .EmailAddress(EmailValidationMode.Net4xRegex).WithMessage("Please specify a valid Email");
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Please specify a Password");                
        }
    }
}
