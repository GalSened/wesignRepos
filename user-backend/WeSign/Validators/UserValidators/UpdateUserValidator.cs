namespace WeSign.Validators.UserValidators
{
    using Common.Enums.Users;
    using FluentValidation;
    using FluentValidation.Validators;
    using System;
    using System.Net.Mail;
    using System.Text.RegularExpressions;
    using WeSign.Models.Users;

    public class UpdateUserValidator : AbstractValidator<UpdateUserDTO>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Please specify a Name")
                .Length(2, 50).WithMessage("Name length limit to 50");
            RuleFor(x => x.Email)
               .NotEmpty().WithMessage("Please specify an Email")
               .EmailAddress(EmailValidationMode.Net4xRegex).WithMessage("Please specify a valid Email");
            RuleFor(x => x.UserConfiguration.Language)
                 .Must(BeValidLanguage)
                 .When(x => x.UserConfiguration != null)
                 .WithMessage("Valid Language: 1 (English) or 2 (Hebrew)");
            RuleFor(x => x.Username)
               .Must(IsUsernameLengthValid).WithMessage("Username length must be between 6 to 15 digits")
               .Must(BeNotValidEmail).WithMessage("Username cannot be in email format")
               .Must(BeNotContainsHebrewChars).WithMessage("Username cannot contains Hebrew letters")

               .When(x => !string.IsNullOrWhiteSpace(x.Username));
            When(x => x.UserConfiguration.ShouldNotifySignReminder, () =>
       {
           RuleFor(x => x.UserConfiguration.SignReminderFrequencyInDays).ExclusiveBetween(0, 26).WithMessage("Please specificy a number between 1 and 25");
       }).Otherwise(() =>
       {
           RuleFor(x => x.UserConfiguration.SignReminderFrequencyInDays).Equal(0).WithMessage("Your company has disabled the option to specify sign reminder frequency, please re-fresh this page to continue");
       });
            //TODO add validation for   UserConfigurationDTO.Color @"^#[0-9a-fA-F]{3,6}$"
        }

        private bool BeValidLanguage(Language language)
        {
            return language == Language.en || language == Language.he;
        }

        private bool IsUsernameLengthValid(string username)
        {
            return (username.Length >= 6 && username.Length <= 15) || username.Length == 64;
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
        private bool BeNotContainsHebrewChars(string username)
        {
            string HebrewCharsPattern = "[\u0590-\u05FF]+$";
            return !Regex.IsMatch(username, HebrewCharsPattern);
        }
    }
}
