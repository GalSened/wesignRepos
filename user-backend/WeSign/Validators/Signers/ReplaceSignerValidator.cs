using Common.Enums;
using Common.Extensions;
using Common.Interfaces;
using Common.Models.Documents.Signers;
using FluentValidation;
using WeSign.Models.Signers;

namespace WeSign.Validators.Signers
{
    public class ReplaceSignerValidator: AbstractValidator<ReplaceSignerWithDetailsDTO>
    {
        private readonly Common.Interfaces.IValidator _validator;

        public ReplaceSignerValidator(Common.Interfaces.IValidator validator)
        {
            _validator = validator;
            RuleFor(_ => _.NewSignerName)
                .NotEmpty()
                .WithMessage("Please specify a name");
            RuleFor(_ => _.NewSignerMeans)
                .NotEmpty()
                .WithMessage("Please specify means");
            RuleFor(_ => _)
                .Must(_ => _.NewAuthenticationMode == AuthMode.None || _.NewOtpMode == OtpMode.None)
                .WithMessage("Signer shouldn't contain both otp and face recognition");
        }
    }
}