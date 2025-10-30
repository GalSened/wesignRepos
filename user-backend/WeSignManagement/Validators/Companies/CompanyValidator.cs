using Common.Enums;
using Common.Enums.Users;
using Common.Interfaces;
using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using WeSignManagement.Models.Companies;

namespace WeSignManagement.Validators.Companies
{
    public class CompanyValidator : AbstractValidator<CompanyDTO>
    {
        private readonly IDataUriScheme _dataUri;
        private readonly IDater _dater;

        //TODO add validation for emailTemplate PlaceHolders , means they exist in file 
        public CompanyValidator(IDataUriScheme dataUri, IDater dater)
        {
            _dataUri = dataUri;
            _dater = dater;
            RuleFor(x => x.CompanyName)
                 .NotEmpty().WithMessage("Please specify a CompanyName");
            RuleFor(x => x.Language)
                             .NotEmpty().WithMessage("Please specify a Language")
                             .Must(x => x == Language.en || x == Language.he)
                             .WithMessage("Valid Language: 1 (English) or 2 (Hebrew)"); 
            RuleFor(x => x.User)
                             .NotNull().WithMessage("Please specify a User");
            RuleFor(x => x.User.Email)
                             .NotEmpty().WithMessage("Please specify User Email")
                             .EmailAddress(EmailValidationMode.Net4xRegex).WithMessage("Please specify a valid Email");
            RuleFor(x => x.User.UserUsername)
                .Must(IsUsernameLengthValid).WithMessage("Username length must be between 6 to 15 digits")
                .Must(BeNotValidEmail).WithMessage("Username cannot be in email format")
                .Must(BeNotContainsHebrewChars).WithMessage("Username cannot contains Hebrew letters")
                .When(x => !string.IsNullOrWhiteSpace(x.User.UserUsername));
            RuleFor(x => x.User.UserName)
                              .NotEmpty().WithMessage("Please specify User Name");
            RuleFor(x => x.User.GroupName)
                             .NotEmpty().WithMessage("Please specify User group name");
            RuleFor(x => x.ProgramId)
                             .NotEmpty().WithMessage("Please specify ProgramId");
       
            RuleFor(x => x.LogoBase64String)
                .Must(IsValidImage)
                .When(x => !string.IsNullOrWhiteSpace(x.LogoBase64String))
                .WithMessage("Invalid base 64 image");
            RuleFor(x => x.SmtpConfiguration.SmtpFrom)
                .EmailAddress(EmailValidationMode.Net4xRegex).WithMessage("Please specify a valid Email")
                .When(x => x.SmtpConfiguration != null && !string.IsNullOrWhiteSpace(x.SmtpConfiguration.SmtpFrom));
            RuleFor(x => x.SmtpConfiguration.BeforeSigningHtmlTemplateBase64String)
                .Must(BeValidHtmlFile).WithMessage("Please specify a valid base64String")
                .When(x => x.SmtpConfiguration != null && !string.IsNullOrWhiteSpace(x.SmtpConfiguration.BeforeSigningHtmlTemplateBase64String));
            RuleFor(x => x.SmtpConfiguration.BeforeSigningHtmlTemplateBase64String)
                .Must(ContainPlaceHolders).WithMessage("Template Before Signing must contain following place holders: [DOCUMENT_LINK_URL]")
                .When(x => x.SmtpConfiguration != null && !string.IsNullOrWhiteSpace(x.SmtpConfiguration.BeforeSigningHtmlTemplateBase64String));
            RuleFor(x => x.SmtpConfiguration.AfterSigningHtmlTemplateBase64String)
                     .Must(BeValidHtmlFile).WithMessage("Please specify a valid base64String")
                     .When(x => x.SmtpConfiguration != null && !string.IsNullOrWhiteSpace(x.SmtpConfiguration.AfterSigningHtmlTemplateBase64String));
            RuleFor(x => x.SmtpConfiguration.SmtpPort)
                .Must(IsValidPort).WithMessage("Please specify positive port number")
                .When(x => x.SmtpConfiguration != null && !string.IsNullOrWhiteSpace(x.SmtpConfiguration.SmtpPort));
           RuleFor(x => x.DefaultSigningType).NotEmpty().WithMessage("Please specify a signing default")
                             .Must(x => x == Common.Enums.PDF.SignatureFieldType.Graphic || x == Common.Enums.PDF.SignatureFieldType.Server
                             || x == Common.Enums.PDF.SignatureFieldType.SmartCard)
                             .WithMessage("Valid default signing type: 1 (Graphic) ,  2 (Smart Card) or  3 (Server)");
            When(x => x.Notifications.CanUserControlReminderSettings || !x.Notifications.ShouldEnableSignReminders, () =>
            {
                RuleFor(x => x.Notifications.SignReminderFrequencyInDays).Equal(0).WithMessage("Choosing Company frequency is only allowed when user cannot control reminder settings");
            }).Otherwise(() =>
            {
                RuleFor(x => x.Notifications.SignReminderFrequencyInDays).ExclusiveBetween(0, 26).WithMessage("Please specificy a number between 1 and 25");
            });
            When(x => x.Notifications.ShouldSendDocumentNotifications, () =>
            {
                RuleFor(x => x.Notifications.DocumentNotificationsEndpoint)
                .NotEmpty().WithMessage("Please specify an endpoint to post notifications to or disable the option for posting notifications")
                .Must(BeValidUrl).WithMessage("Please provide a valid URL");
            });
            RuleFor(x => x.ExpirationTime).NotEmpty().NotNull().Must(ValidateDate).WithMessage("Please provide a valid Expiration date ");
            RuleFor(x => x.RecentPasswordsAmount).InclusiveBetween(0,15).WithMessage("Please specificy a number between 0 and 15");
            RuleFor(x => x.PasswordExpirationInDays).InclusiveBetween(0,180).WithMessage("Please specificy a number between 0 and 180");
            RuleFor(x => x.MinimumPasswordLength).InclusiveBetween(8,16).WithMessage("Please specificy a number between 8 and 16");
        }

        private bool ValidateDate(DateTime time)
        {
            return time != DateTime.MinValue;
        }

        private bool ContainPlaceHolders(string base64string)
        {
            var bytes = _dataUri.GetBytes(base64string);
            string text = Encoding.Default.GetString(bytes);

            return text.Contains("[DOCUMENT_LINK_URL]");
        }
        private bool IsUsernameLengthValid(string username)
        {
            return (username.Length >= 6 && username.Length <= 15) || username.Length == 64;
        }

        private bool IsValidPort(string smtpPort)
        {
            return int.TryParse(smtpPort, out int port) && port >= 0;
        }

        private bool IsValidImage(string base64string)
        {
            return _dataUri.IsValidImage(base64string) && _dataUri.IsValidImageType(base64string, out ImageType imageType);
        }

        private bool BeValidHtmlFile(string base64string)
        {
            _dataUri.IsValidFileType(base64string, out FileType fileType);
            return _dataUri.IsValidFile(base64string) && fileType == FileType.HTML;
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

        private bool BeValidUrl(string url)
        {
             
            return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private bool BeNotContainsHebrewChars(string username)
        {
            string HebrewCharsPattern = "[\u0590-\u05FF]+$";
            return !Regex.IsMatch(username, HebrewCharsPattern);
        }
    }
}
