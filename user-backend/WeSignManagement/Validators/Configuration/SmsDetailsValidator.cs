using FluentValidation;
using System.Text.RegularExpressions;
using WeSignManagement.Models.Configurations;

namespace WeSignManagement.Validators.Configuration
{
    public class SmsDetailsValidator : AbstractValidator<SmsDetailsDTO>
    {       

        public SmsDetailsValidator()
        {
            RuleFor(x => x.From)
                       .NotEmpty();
            RuleFor(x => x.User)
                .NotEmpty();
            RuleFor(x => x.Password)
                .NotEmpty();
            RuleFor(x => x.Message)
                .NotEmpty()
                .NotNull()
                .WithMessage("Please enter a message for tester");
            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .Must(BeValidPhone)
                .WithMessage("Please enter valid phone");
        }

        private bool BeValidPhone(string phone)
        {
            try
            {
                var rgx = new Regex("^[0-9\\-\\+]{9,15}$");
                return rgx.IsMatch(phone);
            }
            catch
            {
                return false;
            }
        }
    }
}
