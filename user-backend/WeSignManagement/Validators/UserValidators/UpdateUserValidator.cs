using FluentValidation;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System;
using WeSignManagement.Models.Users;
using FluentValidation.Validators;

namespace WeSignManagement.Validators.UserValidators
{
    public class UpdateUserValidator : AbstractValidator<UpdateUserManagementDTO>
    {
        public UpdateUserValidator()
        {
            RuleFor(x => x.Email).NotEmpty().WithMessage("Missing Email").EmailAddress(EmailValidationMode.Net4xRegex).WithMessage("Please specify a valid Email");
            RuleFor(x => x.Username)
            .Length(6, 70).WithMessage("Username length must be between 6 to 70 characters.")
            .Must(BeNotValidEmail).WithMessage("Username cannot be in email format")
            .Must(BeNotContainsHebrewChars).WithMessage("Username cannot contains Hebrew letters")
            .When(x => !string.IsNullOrWhiteSpace(x.Username));
            RuleFor(x => x.Name).NotEmpty().When(x => x.UserType != Common.Enums.Users.UserType.SystemAdmin).WithMessage("Missing User Name");
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
