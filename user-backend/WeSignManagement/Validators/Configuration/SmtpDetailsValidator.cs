using FluentValidation;
using FluentValidation.Validators;
using WeSignManagement.Models.Configurations;

namespace WeSignManagement.Validators.Configuration
{
    public class SmtpDetailsValidator : AbstractValidator<SmtpDetailsDTO>
    {
        public SmtpDetailsValidator()
        {
            RuleFor(x => x.From)
                .NotEmpty();
            RuleFor(x => x.Server)
                .NotEmpty();
            RuleFor(x => x.Port)
                .NotEmpty();
            RuleFor(x => x.Message)
                .NotEmpty()
                .NotNull()
                .WithMessage("Please enter a message for tester");
            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress(EmailValidationMode.Net4xRegex)
                .WithMessage("Please enter valid email");
        }
    }
}
