using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeSign.Models.Documents;
using Common.Extensions;
using Common.Models.Documents.Signers;
using Common.Enums;
using Common.Interfaces;
using WeSign.Models.Distribution.Requests;
using System.Drawing;

namespace WeSign.Validators.Distribution
{
    public class CreateDistributionDocumentsValidator : AbstractValidator<CreateDistributionDocumentsDTO>
    {
        private readonly IDataUriScheme _dataUriScheme;

        public CreateDistributionDocumentsValidator(IDataUriScheme dataUriScheme)
        {
            _dataUriScheme = dataUriScheme;
            RuleForEach(x => x.Signers).ChildRules(x =>
            {
                x.RuleFor(y => y.SignerMeans)
                    .Must(BeValidMeans)
                    .WithMessage(y => $"Please enter valid means - [{y.SignerMeans}]");

                x.RuleFor(y => y.SignerSecondaryMeans).Must(BeValidSecondaryMeans)
                .WithMessage(y => $"Please enter valid secondary means - [{y.SignerSecondaryMeans}]");

                x.RuleFor(y => y.FullName)
                    .NotNull()
                    .NotEmpty()
                    .WithMessage(y => $"Please enter valid name - [{y.FullName}]");
            });

            RuleFor(x => x.Signers)
                .NotEmpty()
                .Must(BeNoDuplication)
                .WithMessage("Please enter signer list without duplications");

            RuleFor(x => x.TemplateId)
                .NotEmpty();
        }

        private bool BeValidSecondaryMeans(string secondaryMeans)
        {
            if(string.IsNullOrWhiteSpace(secondaryMeans))
            {
                return true;
            }
            if (!ContactsExtenstions.IsValidEmail(secondaryMeans) && !ContactsExtenstions.IsValidPhone(secondaryMeans))
            {
                return false;
            }
            return true;
        }

        private bool BeValidMeans(string means)
        {
            if (!ContactsExtenstions.IsValidEmail(means) && !ContactsExtenstions.IsValidPhone(means))
            {
                return false;
            }

            return true;
        }

        private bool BeNoDuplication(IEnumerable<BaseSigner> signers)
        {

           
            var distinctSigners = signers?.Select(s => s.SignerMeans).Distinct();
            if(distinctSigners?.Count() != signers?.Count())
            {
                return false;
            }

            return true;
        }
    }
}
