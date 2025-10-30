namespace Common.Handlers
{
    using Common.Enums.Users;
    using Common.Interfaces;
    using Common.Models;
    using Common.Models.Documents.Signers;
    using Common.Models.Settings;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;

    public class JWTHandler : IJWT
    {
        private readonly JwtSettings _jwtSettings;
        private readonly byte[] _signerKey;
        private readonly IDater _dater;
        private readonly byte[] _loginKey;

        public JWTHandler(IOptions<JwtSettings> generalSettings, IDater dater)
        {
            _jwtSettings = generalSettings.Value;
            _loginKey = Encoding.ASCII.GetBytes(_jwtSettings.JwtBearerSignatureKey);
            _signerKey = Encoding.ASCII.GetBytes(_jwtSettings.JwtSignerSignatureKey);
            _dater = dater;
        }

        public bool CheckPasswordToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenParams = new TokenValidationParameters
                {
                    //ClockSkew = TimeSpan.Zero,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(_loginKey)
                };
                var claimsPrincipal = tokenHandler.ValidateToken(token, tokenParams, out SecurityToken validatedToken);
                return true;

            }
            catch
            {
                return false;
            }
        }      

        public string GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = GetTokenDescriptor(user, _loginKey);
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public string GenerateSignerToken(Signer signer, int expiredLinkInHours = 0)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = GetSignerTokenDescriptor(signer, _signerKey, expiredLinkInHours);
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public User GetUser(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenParams = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero,
                    ValidateLifetime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(_loginKey)
                };
                var claimsPrincipal = tokenHandler.ValidateToken(token, tokenParams, out SecurityToken validatedToken);

                Enum.TryParse(claimsPrincipal?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value, out UserType userType);
                return new User()
                {
                    Id = new Guid(claimsPrincipal?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Sid)?.Value),
                    Type = userType
                };
            }
            catch
            {
                throw new Exception("Failed to get user from JWT token.");
            }
        }

        public User GetUserFromExpiredToken(string jwtToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenParams = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero,
                    ValidateLifetime = false,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(_loginKey)
                };
                var claimsPrincipal = tokenHandler.ValidateToken(jwtToken, tokenParams, out SecurityToken validatedToken);

                Enum.TryParse(claimsPrincipal?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value, out UserType userType);
                return new User()
                {
                    Id = new Guid(claimsPrincipal?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Sid)?.Value),
                    Type = userType,
                    GroupId = new Guid(claimsPrincipal?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.PrimaryGroupSid)?.Value?? Guid.Empty.ToString())
                };
            }
            catch
            {
                throw new Exception("Failed to get user from JWT token.");
            }
        }        

        private SecurityTokenDescriptor GetTokenDescriptor(User user, byte[] key)
        {
            return new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Sid, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Type.ToString()),
                    new Claim(ClaimTypes.PrimaryGroupSid, user.GroupId.ToString())
                    
                }),
                Expires = _dater.UtcNow().AddMinutes(_jwtSettings.SessionExpireMinuteTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };
        }

        private SecurityTokenDescriptor GetSignerTokenDescriptor(Signer signer, byte[] key, int expiredLinkInHours)
        {
            return new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new Claim[]
               {
                   new Claim(ClaimTypes.Sid, signer.Id.ToString())
               }),
                Expires = expiredLinkInHours > 0 ? _dater.UtcNow().AddHours(expiredLinkInHours) : _dater.UtcNow().AddDays(_jwtSettings.SignerLinkExpirationInHours),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };
        }

        public Signer GetSigner(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenParams = new TokenValidationParameters
                {
                    ClockSkew = TimeSpan.Zero,
                    ValidateLifetime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(_signerKey)
                };
                var claimsPrincipal = tokenHandler.ValidateToken(token, tokenParams, out SecurityToken validatedToken);

                return new Signer()
                {
                    Id = new Guid(claimsPrincipal?.Claims?.FirstOrDefault(x => x.Type == ClaimTypes.Sid)?.Value)
                };
            }

            catch(Exception ex)
            {
                throw new Exception($"Failed to get signer from JWT token. {ex}");
            }
        }

        public bool IsTokenExpired(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
                throw new ArgumentException("Invalid JWT token format.");

            var jwtToken = handler.ReadJwtToken(token);

            // Expiration time is in "exp" claim as UNIX timestamp
            var expiry = jwtToken.ValidTo;

            // ValidTo is in UTC
            return expiry < DateTime.UtcNow;
        }
    }
}