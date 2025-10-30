using FluentValidation;
using System;
using WeSign.Models.Users;

namespace WeSign.Validators.UserValidators
{
    public class OtpValidator : AbstractValidator<OtpDTO>
    {
        public OtpValidator() {
            RuleFor(x => x.Code.Trim()).NotEmpty().WithMessage("Please specify a code").Length(6).WithMessage("Wrong code input");            
            RuleFor(x => x.OtpToken).NotEmpty().WithMessage("Please specify a Token")
             .Must(token => Guid.TryParse(token, out Guid newGuid)).WithMessage("Invalid Token format");
        }
    }

    public class OtpResendValidator : AbstractValidator<OtpResendDTO>
    {
        public OtpResendValidator()
        {          
            RuleFor(x => x.OtpToken).NotEmpty().WithMessage("Please specify a Token")
             .Must(token => Guid.TryParse(token, out Guid newGuid)).WithMessage("Invalid Token format");
        }
    }
}
