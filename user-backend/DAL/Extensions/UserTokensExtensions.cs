namespace DAL.Extensions
{
    using Common.Models.Users;
    using DAL.DAOs.Users;

    public static class UserTokensExtentions
    {
        public static UserTokens ToUserTokens(this UserTokensDAO userTokensDAO)
        {
            return userTokensDAO == null ? null : new UserTokens()
            {
                UserId = userTokensDAO.UserId,
                RefreshToken = userTokensDAO.RefreshToken,
                RefreshTokenExpiredTime = userTokensDAO.RefreshTokenExpiredTime,
                ResetPasswordToken = userTokensDAO.ResetPasswordToken,
                AuthToken = userTokensDAO.AuthToken,
                LastLoginGroupId = userTokensDAO.LastLoginGroupId,
            };

        }
    }
}
