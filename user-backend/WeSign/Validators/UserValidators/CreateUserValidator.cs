namespace WeSign.Validators.UserValidators
{
    using Common.Enums.Users;
    using FluentValidation;
    using Models.Users;
    using System.Net.Mail;
    using System;
    using System.Text.RegularExpressions;
    using FluentValidation.Validators;

    public class CreateUserValidator : AbstractValidator<CreateUserDTO>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Please specify a Name")
                .Length(3, 50).WithMessage("Name length limit to 50");
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Please specify an Email")
                .EmailAddress(EmailValidationMode.Net4xRegex).WithMessage("Please specify a valid Email");
            RuleFor(x => x.Password).Must(ValidatePassword).
               
                WithMessage("Password should contain at least one digit, one special character and at least 8 characters long");
            RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Please specify a Language")
             .Must(x => x == Language.en || x == Language.he)
             .WithMessage("Valid Language: 1 (English) or 2 (Hebrew)");
            RuleFor(x => x.Username)
           .Must(IsUsernameLengthValid).WithMessage("Username length must be between 6 to 15 digits")
           .Must(BeNotValidEmail).WithMessage("Username cannot be in email format")
           .Must(BeNotContainsHebrewChars).WithMessage("Username cannot contains Hebrew letters")
           .When(x => !string.IsNullOrWhiteSpace(x.Username));
        }

        private bool ValidatePassword(string password)
        {
            if(string.IsNullOrWhiteSpace(password))
            {
                return true;
            }
            string pattern = @"^(?=.*[0-9])(?=.*[!@#$%^&*])(?=.{8,})";
            return Regex.IsMatch(password, pattern);


        }

        private bool BeNotValidEmail(string emailAddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailAddress);

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
