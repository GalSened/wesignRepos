using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeSignManagement.Models.License;

namespace WeSignManagement.Validators.License
{
    public class ActivateLicenseValidator : AbstractValidator<ActivateLicenseDTO>
    {
        public ActivateLicenseValidator()
        {
            RuleFor(x => x.License)
                     .NotEmpty().WithMessage("Please specify a License Activation Key ");
        }
    }
}
