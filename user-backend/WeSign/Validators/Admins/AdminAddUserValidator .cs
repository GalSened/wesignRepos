
namespace WeSign.Validators.Admins
{
    using Common.Enums.Users;
    using FluentValidation;
    using System.Net.Mail;
    using System;
    using WeSign.Models.Admins;
    using System.Text.RegularExpressions;
    using FluentValidation.Validators;

    public class AdminAddUserValidator : AbstractValidator<AdminCreateUserDTO>
    {
        public AdminAddUserValidator()
        {
            RuleFor(x => x.Name)
                  .NotEmpty()
                  .NotNull()
                  .WithMessage("Please specify valid Name")
                  .Length(3, 50)
                  .WithMessage("Name length limit to 50");
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Please specify an Email")
                .EmailAddress(EmailValidationMode.Net4xRegex).WithMessage("Please specify a valid Email");
            RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Please specify a Language")
             .Must(x => x == UserType.Basic || x == UserType.Editor || x == UserType.CompanyAdmin || x == UserType.SystemAdmin)
             .WithMessage("Valid UserType: 1 (Basic) or 2 (Editor) or 3 (CompanyAdmin)");
            RuleFor(x => x.Username)
               .Must(IsUsernameLengthValid).WithMessage("Username length must be between 6 to 15 digits")
               .Must(BeNotValidEmail).WithMessage("Username cannot be in email format")
               .Must(BeNotContainsHebrewChars).WithMessage("Username cannot contains Hebrew letters")
               .When(x => !string.IsNullOrWhiteSpace(x.Username));
        }

        private bool BeNotValidEmail(string username)
        {
            try
            {
                MailAddress m = new MailAddress(username);

                return false;
            }
            catch (FormatException)
            {
                return true;
            }
        }

        private bool IsUsernameLengthValid(string username)
        {
            return (username.Length >= 6 && username.Length <= 15) || username.Length == 64;
        }

        private bool BeNotContainsHebrewChars(string username)
        {
            string HebrewCharsPattern = "[\u0590-\u05FF]+$";
            return !Regex.IsMatch(username, HebrewCharsPattern);
        }
    }
}
