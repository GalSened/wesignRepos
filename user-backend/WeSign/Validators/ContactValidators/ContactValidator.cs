namespace WeSign.Validators.ContactValidators
{
    using Common.Enums.Documents;
    using Common.Extensions;
    using Common.Interfaces;
    using FluentValidation;
    using FluentValidation.Validators;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using WeSign.Models.Contacts;

    public class ContactValidator : AbstractValidator<ContactDTO>
    {

        private readonly IDataUriScheme _dataUriScheme;

        public ContactValidator(IDataUriScheme dataUriScheme)
        {
            _dataUriScheme = dataUriScheme;
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Please specify a Name")
                .Length(1, 50).WithMessage("FirstName length limit to 50");
            RuleFor(x => x.DefaultSendingMethod)
                .NotEmpty().WithMessage("Please specify a DefaultSendingMethod")
                .Must(x => x == SendingMethod.SMS || x == SendingMethod.Email)
                .WithMessage("Please specify valid DefaultSendingMethod: 1 (SMS) or 2 (Email)")
                .Must(IsValidMeans)
                .WithMessage("Please specify valid Phone while DefaultSendingMethod=1 (SMS), or valid Email while DefaultSendingMethod=2 (Email)");
            RuleFor(x => x.Email)
                .EmailAddress(EmailValidationMode.Net4xRegex)
                .When(x => !string.IsNullOrWhiteSpace(x.Email))
                .WithMessage("Please specify valid Email ");
            RuleFor(x => x.Phone)
                //.Matches(@"^[0-9\\-\\+]{9,15}$")
                .Matches(@"^[0-9\\-]{9,15}$")
                .When(x => !string.IsNullOrWhiteSpace(x.Phone))
                .WithMessage("Please specify valid Phone");
            RuleFor(x => x.PhoneExtension)
                .Matches(@"\+(9[976]\d|8[987530]\d|6[987]\d|5[90]\d|42\d|3[875]\d|2[98654321]\d|9[8543210]|8[6421]|6[6543210]|5[87654321]|4[987654310]|3[9643210]|2[70]|7|1)")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneExtension))
                .WithMessage("Please specify valid PhoneExtention");
            RuleFor(x => x.Seals)
                    .Must(seals => AreValidImages(seals))
                    .WithMessage("Please specify a valid base64 image in format data:image/IMAGE_TYPE;base64,iV.... ")
                    .When(x => x.Seals != null);
            RuleFor(x => x.Seals)
                    .Must(seals => AreValidImagesName(seals))
                    .WithMessage("Please specify an image Name")
                    //.When(x => x.Seals != null && x.Seals.Any());            
                    .When(x => x.Seals != null );
            RuleFor(x => x.SearchTag)
                .Length(2, 20)
                .WithMessage("Tags length must be between 2 to 50")
                .When(x => !string.IsNullOrWhiteSpace(x.SearchTag));
        }
        
        private bool IsValidMeans(ContactDTO instance, SendingMethod sendingMethod)
        {
            if (sendingMethod == SendingMethod.Email)
            {
                return !string.IsNullOrWhiteSpace(instance.Email) && ContactsExtenstions.IsValidEmail(instance.Email);
            }
            if (sendingMethod == SendingMethod.SMS)
            {
                return !string.IsNullOrWhiteSpace(instance.Phone) && ContactsExtenstions.IsValidPhone(instance.Phone);
            }
            return true;
        }

        private bool AreValidImagesName(IEnumerable<SealDTO> seals)
        {
            foreach (var image in seals)
            {
                if (string.IsNullOrEmpty(image.Name))
                {
                    return false;
                }
            }
            return true;
        }

        private bool AreValidImages(IEnumerable<SealDTO> seals)
        {
            try
            {
                foreach (var image in seals)
                {
                    if (!_dataUriScheme.IsValidImage(image.Base64Image))
                    {
                        return false;
                    }                    
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        
    }
}
