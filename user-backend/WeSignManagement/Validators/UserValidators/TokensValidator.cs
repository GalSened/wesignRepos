using FluentValidation;
using WeSignManagement.Models.Users;

namespace WeSignManagement.Validators.UserValidators
{
    public class TokensValidator : AbstractValidator<TokensManagementDTO>
    {
        public TokensValidator()
        {
            RuleFor(x => x.JwtToken)
                .NotEmpty().WithMessage("Please specify a JwtToken");
            RuleFor(x => x.RefreshToken)
              .NotEmpty().WithMessage("Please specify a RefreshToken")
              ;
        }
    }
}
