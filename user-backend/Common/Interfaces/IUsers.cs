namespace Common.Interfaces
{
    using Common.Models;
    using Common.Models.Configurations;
    using Common.Models.Users;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IUsers
    {
        Task<string> SignUp(User user, bool sendActivationLink = true);
        Task Update(User user);
        Task<(bool, UserTokens)> TryLogin(User user);
        Task<(bool, UserTokens userTokens)> LoginOtpFlow(UserTokens inputUserToken);
        Task<bool> ChangeExpiredPasswordFlow(string oldPassword, string newPassword, UserTokens userToken);
        Task<bool> TryChangePasswordAsync(User user, string newPassword);
        Task<UserTokens> Activation(User user);
        Task ReSendActivation(User user);
        Task<string> ResetPassword(User user);
        Task<UserTokens> UpdatePassword(User user, string token);
        Task<UserTokens> UpdatePasswordByDevAdminUser(User user);
        Task<(User, CompanySigner1Details)> GetUser();

        Task<(ExtendedUserInfo, CompanySigner1Details)> GetExtendedUserInfo();
        Task ValidateReCAPCHAAsync(string reCAPCHA);
        Task<UserTokens> ExternalLogin(string token);
        Task UnSubscribeUser();
        Task<string> GetGetChangePaymentRuleURL();
        Task<string> Logout();
        Task<UserTokens> ResendOtp(UserTokens userTokens);
        Task<List<Group>> GetUserGroups();
        Task<UserTokens> SwitchGroup(Guid groupId);
        Task StartUpdatePhoneProcess(UserOtpDetails userOtpDetails);
        Task ValidateUpdatePhoneInfo(UserOtpDetails userOtpDetails);
    }
}
