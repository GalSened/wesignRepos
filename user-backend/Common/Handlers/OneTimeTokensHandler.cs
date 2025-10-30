namespace Common.Handlers
{
    using Common.Enums.Results;
    using Common.Extensions;
    using Common.Interfaces;
    using Common.Interfaces.DB;
    using Common.Models;
    using Common.Models.Settings;
    using Common.Models.Users;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using System;
    using System.Security.Cryptography;
    using System.Threading.Tasks;

    public class OneTimeTokensHandler : IOneTimeTokens
    {
        private const double DEFAULT_REFRESH_EXPIRED_MINUTES = 1440;
        private readonly IUserTokenConnector _userTokenConnector;
        private readonly IUserConnector _userConnector;
        private readonly IJWT _jwt;
        private readonly JwtSettings _jwtSettings;
        private readonly IDater _dater;
        private readonly IEncryptor _encryptor;

        public OneTimeTokensHandler(IUserTokenConnector userTokenConnector, IUserConnector userConnector, IJWT jwt, IOptions<JwtSettings> generalSettings, IDater dater, IEncryptor encryptor)
        {
            _userTokenConnector = userTokenConnector;
            _userConnector = userConnector;
            _jwt = jwt;
            _jwtSettings = generalSettings.Value;
            _dater = dater;
            _encryptor = encryptor;
        }

        #region Refresh Token

        public async Task Refresh(UserTokens userTokens)
        {
            var user = _jwt.GetUserFromExpiredToken(userTokens.JwtToken);
            var isValidRefreshToken = await CheckRefreshToken(user, userTokens);
            if (!isValidRefreshToken)
            {
                throw new InvalidOperationException(ResultCode.InvalidRefreshToken.GetNumericString());
            }

            userTokens.UserId = user.Id;
            
            var dbuserTokens = await _userTokenConnector.Read(user);
            
            userTokens.JwtToken = _jwt.GenerateToken(user);
            var expiredTime = dbuserTokens?.RefreshTokenExpiredTime;
            userTokens.RefreshTokenExpiredTime = expiredTime ?? DateTime.MinValue;
            userTokens.AuthToken = user.UserTokens?.AuthToken ?? "";
            
            await UpdateRefreshToken(userTokens);
        }

        public  Task GenerateRefreshToken(User user)
        {
            bool isValidMinutsCount = double.TryParse(_jwtSettings.RefreshTokenExpirationInMinutes, out double minuts);
            minuts = isValidMinutsCount ? minuts : DEFAULT_REFRESH_EXPIRED_MINUTES;
            UserTokens userTokens = new UserTokens()
            {
                RefreshToken = GenerateOneTimeToken(),
                RefreshTokenExpiredTime = _dater.UtcNow().AddMinutes(minuts),
                UserId = user.Id,
                AuthToken = user.UserTokens?.AuthToken??"",
                LastLoginGroupId = user.GroupId
                
            };
            return UpdateRefreshToken(userTokens);
        }

        public async Task<string> GetRefreshToken(User user)
        {
            var dbUserTokens = await _userTokenConnector.Read(user);

            return dbUserTokens?.RefreshToken;
        }

        public  Task UpdateRefreshToken(UserTokens userTokens)
        {
            return _userTokenConnector.UpdateRefreshToken(userTokens);
        }
        
        #endregion
        
        #region ResetPassword Token

        public async Task<string> GenerateResetPasswordToken(User user)
        {
            var resetPasswordToken = Guid.NewGuid().ToString();
            var userTokens = new UserTokens()
            {
                UserId = user.Id,
                ResetPasswordToken = resetPasswordToken,
                AuthToken = user.UserTokens?.AuthToken ?? ""
            };
          await  _userTokenConnector.CreateResetPasswordToken(userTokens);

            return resetPasswordToken;
        }

        public async Task<bool> CheckPasswordToken(User user, UserTokens userTokens)
        {
            var dbUserTokens =await _userTokenConnector.Read(user);
            return dbUserTokens == null || userTokens == null ?
                   false :
                   dbUserTokens.ResetPasswordToken == userTokens.ResetPasswordToken;
        }
        public Task ClearTokens(User user)
        {
            var userTokens = new UserTokens()
            {
                UserId = user.Id,
                JwtToken = "",
                RefreshToken = "",
                RefreshTokenExpiredTime = _dater.UtcNow(),
                AuthToken = ""
            };
            return _userTokenConnector.UpdateRefreshToken(userTokens);

        }

        #endregion

        #region Remote Login Token
        public async Task<string> GenerateRemoteLoginToken(User user, string userAuthToken, int expirationInSeconds)
        {
            var resetPasswordToken = Guid.NewGuid().ToString();
            var userTokens = new UserTokens()
            {
                UserId = user.Id,
                RefreshToken = resetPasswordToken,
                RefreshTokenExpiredTime = _dater.UtcNow().AddSeconds(expirationInSeconds),
                AuthToken = _encryptor.Encrypt(userAuthToken)
            };

           await _userTokenConnector.UpdateRefreshToken(userTokens);

            return resetPasswordToken;
            
        }

        public async Task<User> CheckRemoteLoginToken(UserTokens userTokens)
        {
            User user = null;

            var dbUserTokens = await _userTokenConnector.ReadTokenByRefreshToken(userTokens);
            if (dbUserTokens == null)
            {
                return user;
            }
            user = await _userConnector.Read(new User() { Id = dbUserTokens.UserId });

            if (user != null && user.Status == Enums.Users.UserStatus.Activated)
            {
                return user;
            }
            return null;
        }

        #endregion


        #region Private 

        private string GenerateOneTimeToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private async Task<bool> CheckRefreshToken(User user, UserTokens userTokens)
        {
            UserTokens dbUserTokens = await _userTokenConnector.Read(user);
            return dbUserTokens?.RefreshToken == userTokens.RefreshToken && _dater.UtcNow() < dbUserTokens?.RefreshTokenExpiredTime;
        }

      

        #endregion
    }
}
