using Common.Enums.Users;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeSignManagement.Models.Users;

namespace WeSignManagement.Validators.UserValidators
{
    public class UpdateUserTypeValidator : AbstractValidator<UpdateUserTypeDTO>
    {
        public UpdateUserTypeValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("Missing User Id");
            RuleFor(x => x.UserType).NotEmpty().WithMessage("Missing User Type").Must(x => x == UserType.Basic || x == UserType.Editor || x== UserType.CompanyAdmin || x == UserType.SystemAdmin)
                             .WithMessage("Valid User Types: 1 (Basic), 2 (Editor), 3 (CompanyAdmin) or 4 (SystemAdmin)"); 
        }
    }
    
}
