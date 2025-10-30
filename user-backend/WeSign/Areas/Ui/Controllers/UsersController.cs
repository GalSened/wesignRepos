/*
 * Swagger URL:
 * https://wesign3.comda.co.il/userapi/swagger/index.html
 * https://localhost:44348/swagger/index.html
 */

namespace WeSign.areas.ui.Controllers
{
    using Common.Enums.Results;
    using Common.Extensions;
    using Common.Interfaces;
    using Common.Models;
    using Common.Models.Configurations;
    using Common.Models.Users;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using System;
    using System.Net;
    using Serilog;
    using System.Threading.Tasks;
    using WeSign.Models.Users;
    using WeSign.Models.Users.Responses;
    using System.Net.Mail;
    using System.Collections.Generic;

//#if DEBUG
//    [Route("userui/v3/users")]
//#else
//    [Route("ui/v3/users")]
//#endif
    [ApiController]
    [Area("Ui")]
    [Route("Ui/v3/[controller]")]
    //[ApiExplorerSettings(GroupName = "ui")]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class UsersController : ControllerBase
    {
        
        private readonly IValidator _validator;
        private readonly ILogger _logger;
        private readonly IOneTimeTokens _oneTimeTokens;
        private readonly IUsers _userBl;

        public UsersController(IUsers userBl, IOneTimeTokens oneTimeTokens,IValidator validator, ILogger logger)
        {
            _userBl = userBl;
            _validator = validator;
            _logger = logger;
            _oneTimeTokens = oneTimeTokens;
        }

        /// <summary>
        /// User Sign up
        /// </summary>
        /// <remarks>
        /// Not authorized API <br/>
        /// UserLanguage: en = 1, he = 2 <br/>
        /// </remarks>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(LinkResponse))]
        public async Task<IActionResult> SignUpAsync(CreateUserDTO input)
        {
            var user = new User()
            {
                Name = input.Name,
                Password = input.Password,
                Email = input.Email.ToLower(),
                UserConfiguration = new UserConfiguration()
                {
                    Language = input.Language
                },
                Username = input.Username,
            };

            await _userBl.ValidateReCAPCHAAsync(input.ReCAPCHA);
            string link = await _userBl.SignUp(user, input.SendActivationLink);

            return Ok(new LinkResponse { Link = link });
        }

        /// <summary>
        /// Update User
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// UserLanguage: en = 1, he = 2<br/>
        /// UserType : Basic = 1, Editor = 2, CompanyAdmin = 3<br/>
        /// </remarks>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPut]
        [Authorize]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateUser(UpdateUserDTO input)
        {
            var user = new User()
            {
                Name = input.Name,
                Email = input.Email,
                Username = input.Username,
            };
            user.UserConfiguration.Language = input.UserConfiguration?.Language ?? user.UserConfiguration.Language;
            user.UserConfiguration.ShouldNotifyWhileSignerSigned = input.UserConfiguration?.ShouldNotifyWhileSignerSigned ?? user.UserConfiguration.ShouldNotifyWhileSignerSigned;
            user.UserConfiguration.ShouldNotifyWhileSignerViewed = input.UserConfiguration?.ShouldNotifyWhileSignerViewed ?? user.UserConfiguration.ShouldNotifyWhileSignerViewed;
            user.UserConfiguration.ShouldSendSignedDocument = input.UserConfiguration?.ShouldSendSignedDocument ?? user.UserConfiguration.ShouldSendSignedDocument;
            user.UserConfiguration.ShouldNotifySignReminder = input.UserConfiguration?.ShouldNotifySignReminder ?? user.UserConfiguration.ShouldNotifySignReminder;
            user.UserConfiguration.ShouldDisplayNameInSignature = input.UserConfiguration?.ShouldDisplayNameInSignature ?? user.UserConfiguration.ShouldDisplayNameInSignature;
            user.UserConfiguration.SignReminderFrequencyInDays = input.UserConfiguration?.SignReminderFrequencyInDays ?? user.UserConfiguration.SignReminderFrequencyInDays;
            user.UserConfiguration.SignatureColor = input.UserConfiguration?.SignatureColor ?? user.UserConfiguration.SignatureColor;
            await _userBl.Update(user);

            return Ok();
        }

        /// <summary>
        /// Get your own user details
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// Language: en = 1, he = 2 <br/>
        /// UserType : Basic = 1, Editor = 2, CompanyAdmin = 3<br/>
        /// </remarks>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(ExtendedUserResponseDTO))]
        public async Task<IActionResult> GetUser()
        {
            (var user, var companySigner1Details) = await _userBl.GetExtendedUserInfo();

            return Ok(new ExtendedUserResponseDTO(user, companySigner1Details));
        }

        [HttpGet]
        [Authorize]
        [Route("groups")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(UserGroupsDTO))]
        public async Task<IActionResult> GetUserGroups()
        {
            List<Group> groups =await _userBl.GetUserGroups();
            return Ok(new UserGroupsDTO(groups));
        }

        [HttpPost]
        [Authorize]
        [Route("SwitchGroup/{groupId}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(UserTokensResponseDTO))]
        public async Task<IActionResult> SwitchGroup(Guid groupId)
        {
            UserTokens userTokens =await _userBl.SwitchGroup(groupId);
            if (userTokens != null)
            {
                var loginResponseToken = new UserTokensResponseDTO()
                {
                    Token = userTokens.JwtToken,
                    RefreshToken = userTokens.RefreshToken,
                    AuthToken = userTokens.AuthToken

                };
                return Ok(loginResponseToken); 
            }
            return Forbid();
        }




        [HttpPost]
        
        [Route("resendOtp")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(UserTokensResponseDTO))]
        public async Task<IActionResult> ResendOtp(OtpResendDTO otpResendDTO)
        {
            var userTokens = new UserTokens()
            {
                RefreshToken = otpResendDTO.OtpToken
            };

            userTokens = await _userBl.ResendOtp(userTokens);
            var loginResponseToken = new UserTokensResponseDTO()
            {
                Token = userTokens.JwtToken,
                RefreshToken = userTokens.RefreshToken,
                AuthToken = userTokens.AuthToken

            };
            return Ok(userTokens);
            
        }


        [HttpPost]        
        [Route("validateOtpflow")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(UserTokensResponseDTO))]
        public async Task<IActionResult> OtpFlowLogin(OtpDTO input)
        {
            UserTokens userTokens = new UserTokens() { AuthToken = input.Code,RefreshToken = input.OtpToken.Trim() };
            (var loginSuccses, userTokens) = await _userBl.LoginOtpFlow(userTokens);
            if (loginSuccses)
            {
                var loginResponseToken = new UserTokensResponseDTO()
                {
                    Token = userTokens.JwtToken,
                    RefreshToken = userTokens.RefreshToken,
                    AuthToken = userTokens.AuthToken

                };
                return Ok(loginResponseToken);
            }
            return Forbid();

        }

        [HttpPost]
        [Route("validateExpiredPasswordFlow")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> ExpiredPasswordFlowUpdate(RenewPasswordDTO input)
        {
            UserTokens userTokens = new UserTokens() { RefreshToken = input.RenewPasswordToken.Trim() };
            if (await _userBl.ChangeExpiredPasswordFlow(input.OldPassword, input.NewPassword, userTokens))
            {
                return Ok();
            }
            return Forbid();

        }

        /// <summary>
        /// User login case the company force OTP will return token for OTP
        /// </summary>
        /// <remarks>
        /// Not authorized API <br/>
        /// </remarks>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("login")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(UserTokensResponseDTO))]
        public async Task<IActionResult> Login(LoginRequestDTO input)
        {
            var user = new User()
            {
                Password = input.Password
            };
            // input.Email represents the email address or the username
            if (input.Email.ToString().IsValidEmail())
                user.Email = input.Email.ToLower();
            else
                user.Username = input.Email;
            (var loginSuccess, UserTokens userTokens) = await _userBl.TryLogin(user);
            if (loginSuccess)
            {
                var loginResponseToken = new UserTokensResponseDTO()
                {
                    Token = userTokens.JwtToken,
                    RefreshToken = userTokens.RefreshToken,
                    AuthToken = userTokens.AuthToken

                };
                return Ok(loginResponseToken);
            }
            return Forbid();
        }


        [HttpGet]
        [Authorize]
        [Route("Logout")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(LogoutResponseDTO))]
        public async Task<IActionResult> Logout()
        {
            string routeURL =await _userBl.Logout();

            return Ok(new LogoutResponseDTO() { LogoutURL = routeURL });

        }
        /// <summary>
        /// User activation
        /// <remarks
        /// Not authorized API <br/>
        /// </remarks>
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("activation")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(UserTokensResponseDTO))]
        public async Task<IActionResult> Activation(ActivationDTO input)
        {
            var user = new User()
            {
                Id = new Guid(input.Token)
            };
            var loginToken = await _userBl.Activation(user);
            //_certificate.Create(user);

            return Ok(new UserTokensResponseDTO()
            {
                Token = loginToken.JwtToken,
                RefreshToken = loginToken.RefreshToken,
                AuthToken = loginToken.AuthToken

            });
        }


        /// <summary>
        /// External login for login using AD or SAML login
        /// </summary>
        /// <remarks>
        /// Not authorized API <br/>
        /// </remarks>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("externalLogin")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(UserTokensResponseDTO))]
        public async Task<IActionResult> ExternalLogin(ExternalLoginDTO input)
        {
            if (!Guid.TryParse(input.Token, out var newGuid))
            {
                throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
            }

            var loginToken =await _userBl.ExternalLogin(input.Token);

            return Ok(new UserTokensResponseDTO()
            {
                Token = loginToken.JwtToken,
                RefreshToken = loginToken.RefreshToken,
                AuthToken = loginToken.AuthToken
            });
        }

        /// <summary>
        /// Resend activation link
        /// </summary>
        /// <remarks>
        /// Not authorized API <br/>
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        [Route("activation")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> ResendActivationLink(BaseUserDTO input)
        {
            var user = new User()
            {
                Email = input.Email.ToLower()
            };
            await _userBl.ReSendActivation(user);
            return Ok();
        }

        /// <summary>
        /// Reset password, send reset password mail to user
        /// </summary>
        /// <remarks>
        /// Not authorized API <br/>
        /// </remarks>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("password")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> ResetPassword(BaseUserDTO input)
        {
            var user = new User()
            {
                Email = input.Email.ToLower().Trim()
            };
            await _userBl.ResetPassword(user);
            return Ok();
        }

        /// <summary>
        /// Update password
        /// </summary>
        /// <remarks>
        /// Not authorized API <br/>
        /// </remarks>
        /// <returns></returns>
        [HttpPut]
        [Route("password")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(UserTokensResponseDTO))]
        public async Task<IActionResult> UpdatePassword(RenewPasswordDTO input)
        {
            var user = new User()
            {
                Password = input.NewPassword,
            };
            var loginToken = await _userBl.UpdatePassword(user, input.RenewPasswordToken);

            return Ok(new UserTokensResponseDTO()
            {
                Token = loginToken.JwtToken,
                RefreshToken = loginToken.RefreshToken,
                AuthToken = loginToken.AuthToken
            });
        }

        /// <summary>
        /// Refresh token 
        /// </summary>
        /// <remarks>
        /// Not authorized API <br/>
        /// </remarks>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("refresh")]
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(RefreshResponseDTO))]
        public async Task<IActionResult> Refresh(TokensDTO input)
        {
            var tokens = new UserTokens()
            {
                JwtToken = input.JwtToken,
                RefreshToken = input.RefreshToken,
                AuthToken = input.AuthToken
            };
            await _oneTimeTokens.Refresh(tokens);

            return Ok(new RefreshResponseDTO()
            {
                Token = tokens.JwtToken
            });
        }

        /// <summary>
        /// Change password
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="input"></param>
        /// <returns></returns>
        [Route("change")]
        [HttpPost]
        [Authorize]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO input)
        {
            var user = new User()
            {
                Password = input.OldPassword,
            };
            if (await _userBl.TryChangePasswordAsync(user, input.NewPassword))
            {
                return Ok();
            }
            return BadRequest();
        }

        [Route("unsubscribeuser")]
        [HttpPost]
        [Authorize]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> UnSubscribeUser()
        {
            await _userBl.UnSubscribeUser();

            return Ok();


        }

        [Route("changepaymentrule")]
        [HttpPost]
        [Authorize]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(LinkResponse))]
        public async Task<IActionResult> ChangePaymentRule()
        {
            string url = await _userBl.GetGetChangePaymentRuleURL();
            LinkResponse linkResponse = new LinkResponse { Link = url };
            return Ok(linkResponse);
        }



        [Route("UpdatePhone")]
        [HttpPost]
        [Authorize]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdatePhone(UpdateUserPhoneDTO updateUserPhoneDTO)
        {
            var otpDetails = new UserOtpDetails()
            {
                AdditionalInfo = updateUserPhoneDTO.PhoneNumber,

            };

             await _userBl.StartUpdatePhoneProcess(otpDetails);
            
            return Ok();
        }
        [Route("UpdatePhoneValidateOtp")]
        [HttpPost]
        [Authorize]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdatePhoneValidateOtp(UpdateUserPhoneDTO updateUserPhoneDTO)
        {
            var otpDetails = new UserOtpDetails()
            {
                Code = updateUserPhoneDTO.Code,

            };
            await _userBl.ValidateUpdatePhoneInfo(otpDetails);

            return Ok();
        }



    }

}
