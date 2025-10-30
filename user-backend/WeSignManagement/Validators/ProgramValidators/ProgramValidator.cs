using FluentValidation;
using WeSignManagement.Models.Programs;

namespace WeSignManagement.Validators.ProgramValidators
{
    public class ProgramValidator : AbstractValidator<ProgramDTO>
    {
        public ProgramValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Please specify a Name");
            RuleFor(x => x.DocumentsPerMonth)
               .NotNull().WithMessage("Please specify a DocumentsPerMonth")
               .Must(x=> x >= -1).WithMessage("Please specify positive number");
            RuleFor(x => x.ServerSignature)
               .NotNull().WithMessage("Please specify a ServerSignature");
            RuleFor(x => x.SmartCard)
               .NotNull().WithMessage("Please specify a SmartCard");
            RuleFor(x => x.SmsPerMonth)
               .NotNull().WithMessage("Please specify a SmsPerMonth")
               .Must(x => x >= -1).WithMessage("Please specify positive number");
            RuleFor(x => x.VisualIdentificationsPerMonth)
             .NotNull().WithMessage("Please specify a visual identifications per month")
             .Must(x => x >= -1).WithMessage("Please specify positive number");
            RuleFor(x => x.VideoConferencePerMonth)
            .NotNull().WithMessage("Please specify a video conference per month")
            .Must(x => x >= -1).WithMessage("Please specify positive number");
            RuleFor(x => x.Templates)
               .NotNull().WithMessage("Please specify a TemplatesPerMonth")
               .Must(x => x >= -1).WithMessage("Please specify positive number");
            RuleFor(x => x.Users)
               .NotNull().WithMessage("Please specify an Users")
               .Must(x => x >= -1).WithMessage("Please specify positive number");
        }
    }
}
