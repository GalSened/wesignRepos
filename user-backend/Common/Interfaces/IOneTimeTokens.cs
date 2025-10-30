namespace Common.Interfaces
{
    using Common.Models;
    using Common.Models.Users;
    using System;
    using System.Threading.Tasks;

    public interface IOneTimeTokens
    {
        Task Refresh(UserTokens userTokens);
        Task GenerateRefreshToken(User user);
        Task<string> GetRefreshToken(User user);
        Task UpdateRefreshToken(UserTokens userTokens);
        Task<string> GenerateResetPasswordToken(User user);
        Task<bool> CheckPasswordToken(User user, UserTokens userTokens);

        Task<string> GenerateRemoteLoginToken(User user, string userAuthToken, int expirationInSeconds);
        Task<User> CheckRemoteLoginToken(UserTokens userTokens);
        Task ClearTokens(User user);
    }
}
