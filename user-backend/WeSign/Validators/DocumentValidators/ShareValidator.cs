using Common.Extensions;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WeSign.Models.Documents;

namespace WeSign.Validators.DocumentValidators
{
    public class ShareValidator : AbstractValidator<ShareDTO>
    {
        private readonly Common.Interfaces.IValidator _validator; 

        public ShareValidator(Common.Interfaces.IValidator validator)
        {
            _validator = validator;
            RuleFor(x => x.SignerMeans)
                .Must(BeValidMeans)
                .WithMessage("Please enter valid means (phone or email)");
            RuleFor(x => x.SignerName)
                .NotNull().NotEmpty()
                .WithMessage("Null or empty name not allowed");
        }

        private bool BeValidMeans(string means)
        {
            return ContactsExtenstions.IsValidEmail(means) || ContactsExtenstions.IsValidPhone(means);
        }
    }
}
