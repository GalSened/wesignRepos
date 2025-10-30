using Common.Extensions;
using FluentValidation;
using System;
using System.Collections.Generic;
using Common.Models.Links;
using WeSign.Models.Links;

namespace WeSign.Validators.Links
{
    public class CreateVideoConferencetValidator : AbstractValidator<CreateVideoConferencesDTO>
    {
        public CreateVideoConferencetValidator() {
            
            RuleFor(x => x.DocumentCollectionName).NotEmpty().WithMessage("Please specify document collection name");
            RuleFor(x =>x.VideoConferenceUsers).NotEmpty().WithMessage("Please specify users for the conference call").Must(AreValidContactsMeans).
                WithMessage("Please specify valid conference user (user should contain contactId with valid sendingMethod (sms-1,email-2 ) , means with fullName)");
            
        }

        private bool AreValidContactsMeans(List<VideoConferenceUser> list)
        {
            foreach (var signer in list)
            {
                if( !IsValidContactWithoutId(signer))                
                {
                    return false;
                }             
            }
            return true;
        }

        private bool IsValidContactWithoutId(VideoConferenceUser signer)
        {
            return !string.IsNullOrWhiteSpace(signer.Means) && !string.IsNullOrWhiteSpace(signer.FullName) &&
                        ((signer.SendingMethod == Common.Enums.Documents.SendingMethod.Email && ContactsExtenstions.IsValidEmail(signer.Means)) || (signer.SendingMethod == Common.Enums.Documents.SendingMethod.SMS && ContactsExtenstions.IsValidPhone(signer.Means)));
        }
    }
}
