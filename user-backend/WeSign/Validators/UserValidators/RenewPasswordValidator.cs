namespace WeSign.Validators.UserValidators
{
    using Comda.Authentication.Models;
    using Common.Interfaces;
    using Common.Interfaces.DB;
    using Common.Models;
    using Common.Models.Users;
    using DAL.Migrations;
    using FluentValidation;
    using System;
    using System.Text.RegularExpressions;
    using WeSign.Models.Users;

    public class RenewPasswordValidator : AbstractValidator<RenewPasswordDTO>
    {
        private readonly IOneTimeTokens _oneTimeTokens;
        private readonly IUserConnector _userConnector;
        private readonly ICompanyConnector _companyConnector;
        private readonly IUserTokenConnector _userTokenConnector;
        private int minPasswordLength = 8;
        public RenewPasswordValidator(IOneTimeTokens oneTimeTokens, IUserConnector userConnector, ICompanyConnector companyConnector , IUserTokenConnector userTokenConnector)
        {
            _oneTimeTokens = oneTimeTokens;
            _userConnector = userConnector;
            _companyConnector = companyConnector;
            _userTokenConnector = userTokenConnector;
            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Please specify a Password")
                .Must(BeValidPassword).WithMessage("Password should contain at least one digit, one special character and at least {minPasswordLength} characters long");
            RuleFor(x => x.RenewPasswordToken)
                .NotEmpty().WithMessage("Please specify a Token");
        }
        private bool BeValidPassword(RenewPasswordDTO req, string password, ValidationContext<RenewPasswordDTO> context)
        {
            UserTokens userTokens = new UserTokens() { RefreshToken = req.RenewPasswordToken.Trim() };
            userTokens  = _userTokenConnector.ReadTokenByRefreshToken(userTokens, true).GetAwaiter().GetResult();
            if (userTokens == null)
            {
                userTokens = new UserTokens() { ResetPasswordToken = req.RenewPasswordToken.Trim() };
                userTokens = _userTokenConnector.Read(userTokens).GetAwaiter().GetResult();
                if (userTokens == null)
                {
                    return false;
                }
            }
            
            var user = _userConnector.Read(new Common.Models.User { Id = userTokens.UserId}).GetAwaiter().GetResult();
            if (user == null)
            {
                return false;
            }
            Company company = _companyConnector.Read(new Company() { Id = user.CompanyId }).GetAwaiter().GetResult();
            if (company != null)
            {
                minPasswordLength = company.CompanyConfiguration.MinimumPasswordLength;
                context.MessageFormatter.AppendArgument("minPasswordLength", minPasswordLength);
                string pattern = $@"^(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).{{{company.CompanyConfiguration.MinimumPasswordLength},}}$";
                return Regex.IsMatch(password, pattern);
            }
            return false;
        }
    }
}
