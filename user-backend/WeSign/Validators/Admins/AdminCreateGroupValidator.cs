
namespace WeSign.Validators.Admins
{
    using FluentValidation;
    using WeSign.Models.Admins;

    public class AdminCreateGroupValidator : AbstractValidator<AdminCreateGroupDTO>
    {
        public AdminCreateGroupValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .NotNull()
                .WithMessage("Please specify valid Name");
        }
    }
}
