namespace Common.Models.Users
{
    using Common.Enums.Users;
    using System;

    public class UserTokens
    {
        public Guid UserId { get; set; }
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
        public Guid LastLoginGroupId { get; set; }
        public DateTime RefreshTokenExpiredTime { get; set; }
        public string ResetPasswordToken { get; set; }
        public string AuthToken { get; set; }
    }
}
