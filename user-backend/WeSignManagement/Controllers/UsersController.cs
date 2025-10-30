/*
 * Swagger URL:
 * https://wesign3.comda.co.il:444/managementapi/swagger/index.html
 * https://localhost:44333/swagger/index.html
 */

using Common.Interfaces;
using Common.Interfaces.ManagementApp;
using Common.Models;
using Common.Models.ManagementApp;
using Common.Models.Users;
using Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Utilities.Encoders;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using WeSignManagement.Models.Users;
using System.Threading.Tasks;

namespace WeSignManagement.Controllers
{

#if DEBUG
    [Route("managementapi/v3/users")]
#else
    [Route("v3/users")]
#endif    
    [ApiController]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class UsersController : ControllerBase
    {
        private readonly IManagementBL _bl;
        private readonly IEncryptor _encryptor;

        public UsersController(IManagementBL bl, IEncryptor encryptor)
        {
            _bl = bl;
            _encryptor = encryptor;
        }

        [HttpPost]
        [Route("login")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(UserTokensManagementResponseDTO))]
        public async Task<IActionResult> Login(LoginManagementRequestDTO input)
        {
            var user = new User()
            {
                Email = input.Email?.ToLower(),
                Password = input.Password
            };
            (var loginSuccsess, UserTokens userTokens) = await _bl.Users.TryLogin(user);
            if (loginSuccsess)
            {
                var loginResponseToken = new UserTokensManagementResponseDTO()
                {
                    Token = userTokens.JwtToken,
                    RefreshToken = userTokens.RefreshToken
                };
                return Ok(loginResponseToken);
            }
            return Forbid();
        }

        [Authorize(Roles = "SystemAdmin,Dev")]
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllUsersResponseDTO))]
        public async Task<IActionResult> Read(string key = null, int offset = 0, int limit = 20)
        {
            (var usersDetails, int totalCount) = await _bl.Users.Read(key, offset, limit, null);
           

            var response = new List<UserManagementResponseDTO>();
            foreach (var userDetail in usersDetails)
            {
                response.Add(new UserManagementResponseDTO(userDetail));
            }

            Response.Headers.Add("x-total-count", totalCount.ToString());
            return Ok(new AllUsersResponseDTO() { Users = response });
        }


        [Authorize(Roles = "SystemAdmin,Dev")]
        [HttpGet]
        [Route("UsersCompany/{companyId}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllUsersResponseDTO))]
        public async Task<IActionResult> ReadCompanyUsers(Guid companyId)
        {
            (IEnumerable<UserDetails> usersDetails, int totalCount) =await _bl.Users.ReadAllUsersInCompany(new Company { Id = companyId });


            var response = new List<UserManagementResponseDTO>();
            foreach (var userDetail in usersDetails)
            {
                response.Add(new UserManagementResponseDTO(userDetail));
            }

            Response.Headers.Add("x-total-count", totalCount.ToString());
            return Ok(new AllUsersResponseDTO() { Users = response });
        }


        [Authorize(Roles = "SystemAdmin")]
        [HttpPut]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDTO input)
        {
            var user = new User
            {
                Password = input.NewPassword
            };
            await _bl.Users.ResetPassword(user);
            return Ok();
        }

        [Route("refresh")]
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(RefreshManagementResponseDTO))]
        public async Task<IActionResult> Refresh(TokensManagementDTO input)
        {
            var tokens = new UserTokens()
            {
                JwtToken = input.JwtToken,
                RefreshToken = input.RefreshToken
            };
            await _bl.Users.Refresh(tokens);

            return Ok(new RefreshManagementResponseDTO()
            {
                Token = tokens.JwtToken
            });
        }
        
        [Authorize(Roles = "SystemAdmin")]
        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id , UpdateUserManagementDTO input)
        {
            var user = new User
            {
                Id = id,
                Type = input.UserType,
                Email = input.Email,
                Username = input.Username,
                Name = input.Name                
            };
            await _bl.Users.UpdateUser(user);
            return Ok();
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpDelete]
        [Route("{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = new User
            {
                Id = id
            };
            await _bl.Users.Delete(user);

            return Ok();
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateUser(CreateUserFromManagmentDTO createUserFromManagmentDTO)
        {
            var user = new User
            {
                Email = createUserFromManagmentDTO.UserEmail,
                Username = createUserFromManagmentDTO.UserUsername,
                GroupId = createUserFromManagmentDTO.GroupId,
                CompanyId = createUserFromManagmentDTO.CompanyId,
                Name = createUserFromManagmentDTO.UserName,
                Type = createUserFromManagmentDTO.UserType,
                Password = createUserFromManagmentDTO.Password
            };

            await _bl.Users.CreateUserFromManagment(user);

            return Ok();
        }

        /// <summary>
        /// Resend reset password
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [Authorize(Roles = "SystemAdmin" )]
        [HttpGet]
        [Route("password/{userId}")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> ResendResetPassword(Guid userId)
        {
            var user = new User
            {
                Id = userId
            };
            await _bl.Users.ResendResetPasswordMail(user);

            return Ok();
        }

        [Authorize(Roles = "Dev")]
        [HttpGet]
        [Route("templates/{userId}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(Dictionary<Guid, string>))]
        public async Task<IActionResult> ReadTemplates(Guid userId)
        {
            var user = new User
            {
                Id = userId
            };
            Dictionary<Guid, string> templates = await _bl.Users.ReadTemplates(user);

            return Ok(templates);
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpGet]
        [Route("encryptor/{value}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(string))]
        public IActionResult Encryptor(string value)
        {          
            return Ok(_encryptor.Encrypt(value));
        }

        [Authorize(Roles = "SystemAdmin")]
        [HttpGet]
        [Route("userNameEncryptor/{value}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(string))]
        public IActionResult UserNameEncryptor(string value)
        {
            var result = string.IsNullOrWhiteSpace(value) ? string.Empty : value.ToHashString();
            return Ok(result);
        }


        [Authorize(Roles = "Dev")]
        [HttpPost]
        [Route("templates")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateHtmlTemplate(CreateHtmlTemplateDTO input)
        {

            var template = new Template
            {
                Id = new Guid(input.TemplateId)
            };
            var user = new User
            {
                Id = new Guid(input.UserId)
            };
            await _bl.Users.CreateHtmlTemplate(user, template, input.HtmlBase64File, input.JSBase64File);

            return Ok();
        }

    }
}