namespace WeSign.Validators.UserValidators
{
    using FluentValidation;
    using System.Text.RegularExpressions;
    using WeSign.Models.Users;

    public class LoginValidator : AbstractValidator<LoginRequestDTO>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Please specify an Email or Username")
                .MinimumLength(6).WithMessage("Minimun length of email/username is 6 characters")
                .Must(BeNotContainsHebrewChars).WithMessage("Username/Email cannot contains Hebrew letters");
                //.EmailAddress().WithMessage("Please specify a valid Email");
        }
        private bool BeNotContainsHebrewChars(string username)
        {
            string HebrewCharsPattern = "[\u0590-\u05FF]+$";
            return !Regex.IsMatch(username, HebrewCharsPattern);
        }
    }
}
