namespace WeSign.Validators.UserValidators
{
    using Common.Enums.Users;
    using FluentValidation;
    using WeSign.Models.Users;

    public class UpdateUserInGroupValidator : AbstractValidator<UpdateUserInGroupDTO>
    {
        public UpdateUserInGroupValidator()
        {
            RuleFor(x => x.UserType)
             .NotEmpty().WithMessage("Please specify a UserType")
             .Must(x => x == UserType.Basic || x == UserType.Editor)
             .WithMessage("Valid UserType: 1 (Basic) or 2 (Editor)");
        }
    }
}
