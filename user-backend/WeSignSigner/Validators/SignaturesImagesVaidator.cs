using Common.Interfaces;
using FluentValidation;
using System;
using System.Collections.Generic;
using WeSignSigner.Models.Responses;
using Common.Extensions;
using System.Linq;

namespace WeSignSigner.Validators
{
    public class SignaturesImagesVaidator : AbstractValidator<SignaturesImagesDTO>
    {
        private readonly IDataUriScheme _dataUriScheme;

        public SignaturesImagesVaidator(IDataUriScheme dataUriScheme)
        {
            _dataUriScheme = dataUriScheme;
            RuleFor(x=>x.SignaturesImages)
                .Must(BeVAlidImages)
                .WithMessage("Supported ImageType are: PNG, JPG , JPEG. Please specify a valid Base64File in format   data:image/ImageType;base64,.... ");
        }

        private bool BeVAlidImages(IEnumerable<string> signaturesImages)
        {
            bool result = signaturesImages == null? false: signaturesImages.Any();
            signaturesImages.ForEach(x =>
            {
                if (!_dataUriScheme.IsValidImage(x))
                {
                    result = false;
                }
            });

            return result;            
        }

    }
}
