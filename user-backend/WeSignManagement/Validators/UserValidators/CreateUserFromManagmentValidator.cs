using Common.Interfaces.DB;
using Common.Models;
using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Net.Mail;
using System.Text.RegularExpressions;
using WeSignManagement.Models.Users;

namespace WeSignManagement.Validators.UserValidators
{
    public class CreateUserFromManagmentValidator : AbstractValidator<CreateUserFromManagmentDTO>
    {
        private readonly ICompanyConnector _companyConnector;
        private int minPasswordLength = 8;
        public CreateUserFromManagmentValidator(ICompanyConnector companyConnector)
        {
            _companyConnector = companyConnector;
            RuleFor(x => x.UserEmail).NotEmpty().WithMessage("Missing Email").EmailAddress(EmailValidationMode.Net4xRegex).WithMessage("Please specify a valid Email");
            RuleFor(x => x.UserUsername)
            .Must(IsUsernameLengthValid).WithMessage("Username length must be between 6 to 15 digits")
            .Must(BeNotValidEmail).WithMessage("Username cannot be in email format")
            .Must(BeNotContainsHebrewChars).WithMessage("Username cannot contains Hebrew letters")
            .When(x => !string.IsNullOrWhiteSpace(x.UserUsername));
            RuleFor(x => x.CompanyId).NotEmpty().When(x => x.UserType != Common.Enums.Users.UserType.SystemAdmin).WithMessage("Missing Company Id");
            RuleFor(x => x.GroupId).NotEmpty().When(x => x.UserType != Common.Enums.Users.UserType.SystemAdmin).WithMessage("Missing Group Id");
            RuleFor(x => x.UserName).NotEmpty().When(x => x.UserType != Common.Enums.Users.UserType.SystemAdmin).WithMessage("Missing User Name");
            RuleFor(x => x.Password)
               .NotEmpty()
               .When(x => x.UserType == Common.Enums.Users.UserType.SystemAdmin)
               .WithMessage("Please specify a Password")
               .Must(BeValidPassword)
               .When(x => x.UserType == Common.Enums.Users.UserType.SystemAdmin)
               .WithMessage("Password should contain at least one digit, one special character and at least {minPasswordLength} characters long");

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

        private bool BeValidPassword(CreateUserFromManagmentDTO user, string password, ValidationContext<CreateUserFromManagmentDTO> context)
        {
            if (password == null)
                return false;
            if(user.UserType == Common.Enums.Users.UserType.SystemAdmin && user.CompanyId == Guid.Empty)
            {
                return true;
            }

            Company company = _companyConnector.Read(new Company() { Id = user.CompanyId }).GetAwaiter().GetResult();
            if (company != null)
            {
                minPasswordLength = company.CompanyConfiguration.MinimumPasswordLength;
                context.MessageFormatter.AppendArgument("minPasswordLength", minPasswordLength);
                string pattern = $@"^(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{{{company.CompanyConfiguration.MinimumPasswordLength},}}$";
                return Regex.IsMatch(password, pattern);
            }
            return false;
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
