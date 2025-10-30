namespace DAL.DAOs.Users
{
    using Common.Models.Users;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("UsersTokens")]
    public class UserTokensDAO
    {
        [Key]
        public Guid UserId { get; set; }
        public string  RefreshToken  { get; set; }
        public DateTime RefreshTokenExpiredTime { get; set; }
        public string ResetPasswordToken { get; set; }
        public Guid LastLoginGroupId { get; set; }
        public virtual UserDAO User { get; set; }

        public string AuthToken { get; set; }

        public UserTokensDAO(){ }

        public UserTokensDAO(UserTokens userTokens)
        {
            UserId = userTokens.UserId;
            RefreshToken = userTokens.RefreshToken;
            RefreshTokenExpiredTime = userTokens.RefreshTokenExpiredTime;
            ResetPasswordToken = userTokens.ResetPasswordToken;
            LastLoginGroupId = userTokens.LastLoginGroupId;
            AuthToken = userTokens.AuthToken;
        }
    }
}
