namespace Common.Interfaces.DB
{
    using Common.Models;
    using Common.Models.Users;
    using System.Threading.Tasks;

    public interface IUserTokenConnector
    {
        Task UpdateRefreshToken(UserTokens userTokens);
        Task<UserTokens> Read(User user);
        Task<UserTokens> Read(UserTokens userTokens);
        Task CreateResetPasswordToken(UserTokens userTokens);
        Task DeleteResetPasswordToken(UserTokens userTokens);
        Task<UserTokens> ReadTokenByRefreshToken(UserTokens userTokens, bool returnExpired = false);
        
    }
}
