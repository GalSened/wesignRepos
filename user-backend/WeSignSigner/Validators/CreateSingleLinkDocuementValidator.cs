using Common.Extensions;
using FluentValidation;
using WeSignSigner.Models.Requests;
namespace WeSignSigner.Validators
{
    public class CreateSingleLinkDocuementValidator : AbstractValidator<CreateDocumentDTO>
    {

        public CreateSingleLinkDocuementValidator()
        {
            RuleFor(x => x.SignerMeans.IsValidPhone() || x.SignerMeans.IsValidEmail()).NotEmpty()
                .WithMessage("Please provide valid phone or email");
            RuleFor(x => x.Fullname)
                .NotEmpty().WithMessage("Please specify a Name")
                .Length(1, 50).WithMessage("FirstName length limit to 50");
        }
    }
}
