using Comda.Authentication.Models;
using Common.Interfaces.DB;
using Common.Models;
using DAL.Connectors;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WeSign.Models.Users;

namespace WeSign.Validators.UserValidators
{
    public class ChangePasswordValidator : AbstractValidator<ChangePasswordDTO>
    {

        int minPasswordLength = 8;
        private readonly ClaimsPrincipal _userClaims;
        private readonly IGroupConnector _groupConnector;

        public ChangePasswordValidator(ClaimsPrincipal user, IGroupConnector groupConnector)
        {
            _userClaims = user;
            _groupConnector = groupConnector;
            RuleFor(x => x.NewPassword)
               .NotEmpty().WithMessage("Please specify a Password")
               .Must(BeValidPassword).WithMessage("Password should contain at least one digit, one special character and at least {minPasswordLength} characters long");
        }

        private bool BeValidPassword(ChangePasswordDTO req, string password, ValidationContext<ChangePasswordDTO> context)
        {
            
            string groupId = _userClaims?.Claims?.FirstOrDefault(_ => _.Type == ClaimTypes.PrimaryGroupSid)?.Value;
            if (groupId == Guid.Empty.ToString() || groupId == null)  
            {
                return false;
            }
            if(Guid.TryParse(groupId, out Guid id))
            {
                Company company = _groupConnector.ReadCompany(new Common.Models.Group() { Id = id }).GetAwaiter().GetResult();

                if (company != null)
                {
                    minPasswordLength = company.CompanyConfiguration.MinimumPasswordLength;
                    context.MessageFormatter.AppendArgument("minPasswordLength", minPasswordLength);
                    string pattern = $@"^(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{{{company.CompanyConfiguration.MinimumPasswordLength},}}$";
                    return Regex.IsMatch(password, pattern);
                }
            }
            
            return false;
        }
    }
}
