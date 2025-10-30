namespace WeSign.Validators.UserValidators
{
    using FluentValidation;
    using System;
    using WeSign.Models.Users;

    public class ActivationValidator : AbstractValidator<ActivationDTO>
    {
        public ActivationValidator()
        {
            RuleFor(x => x.Token)
             .NotEmpty().WithMessage("Please specify a Token")
             .Must(token => Guid.TryParse(token, out Guid newGuid)).WithMessage("Invalid Token format");
        }
    }
}
