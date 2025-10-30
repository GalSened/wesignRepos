namespace WeSign.areas.ui.Controllers
{
    using Common.Enums.Groups;
    using Common.Interfaces;
    using Common.Models;
    using Common.Models.Users;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using WeSign.Models.Admins;
    using WeSign.Models.Admins.Response;
    using WeSign.Models.Users;
    using WeSign.Models.Users.Responses;

//#if DEBUG
//    [Route("userui/v3/admins")]
//#else
//    [Route("ui/v3/admins")]
//#endif
    //TODO custom Authorize for enum UserTypeCompanyAdmin
    [ApiController]
    [Area("Ui")]
    [Route("Ui/v3/[controller]")]
    //[ApiExplorerSettings(GroupName = "ui")]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class AdminsController : ControllerBase
    {
        private readonly IAdmins _adminsBl;
        private readonly IUsers _usersBl;

        public AdminsController(IAdmins adminsBl, IUsers users)
        {
            _adminsBl = adminsBl;
            _usersBl = users;
        }

        #region Groups

        /// <summary>
        /// Create Group
        /// </summary>
        /// <remarks>
        /// An authorized API call for CompanyAdmin user only. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize(Roles = "CompanyAdmin,SystemAdmin")]
        [HttpPost]
        [Route("groups")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(CreateGroupResponseAdminDTO))]
        public async Task<IActionResult> CreateGroup(AdminCreateGroupDTO input)
        {
            Group group = new Group()
            {
                Name = input.Name
            };
            await _adminsBl.Create(group);
            return Ok(new CreateGroupResponseAdminDTO() { GroupId = group.Id });
        }

        /// <summary>
        /// Get all groups in my company
        /// </summary>
        /// <remarks>
        /// An authorized API call for CompanyAdmin user only. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <returns></returns>
        [Authorize(Roles = "CompanyAdmin,SystemAdmin")]
        [HttpGet]
        [Route("groups")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AdminAllGroupsResponseDTO))]
        public async Task<IActionResult> GetAllGroups()
        {
            List<GroupResponseAdminDTO> groupsResponse = await ReadAllGroups();

            Response.Headers.Add("x-total-count", groupsResponse.Count.ToString());
            return Ok(new AdminAllGroupsResponseDTO() { Groups = groupsResponse });
        }


        /// <summary>
        /// Update  group
        /// </summary>
        /// <remarks>
        /// An authorized API call for CompanyAdmin user only. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <returns></returns>
        [Authorize(Roles = "CompanyAdmin,SystemAdmin")]
        [HttpPut]
        [Route("groups/{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateGroup(Guid id, AdminCreateGroupDTO input)
        {
            Group group = new Group()
            {
                Id = id,
                Name = input.Name
            };
            await _adminsBl.Update(group);
            return Ok();
        }

        /// <summary>
        /// Delete group
        /// </summary>
        /// <remarks>
        /// An authorized API call for CompanyAdmin user only. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "CompanyAdmin,SystemAdmin")]
        [HttpDelete]
        [Route("groups/{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AdminAllGroupsResponseDTO))]
        public async Task<IActionResult> DeleteGroup(Guid id)
        {
            Group group = new Group()
            {
                Id = id
            };
            await _adminsBl.Delete(group);
            List<GroupResponseAdminDTO> groupsResponse =await ReadAllGroups();
            return Ok(new AdminAllGroupsResponseDTO() { Groups = groupsResponse });
        }

        #endregion

        #region Users

        /// <summary>
        /// Create user
        /// </summary>
        /// <remarks>
        /// An authorized API call for CompanyAdmin user only. The token should be passed via the request header.<br/>
        ///  UserType: Basic = 1, Editor = 2, CompanyAdmin = 3 <br/>
        /// </remarks>
        /// <param name="input"></param>
        /// <returns></returns>
        [Authorize(Roles = "CompanyAdmin,SystemAdmin")]
        [HttpPost]
        [Route("users")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(CreateUserResponseAdminDTO))]
        public async Task<IActionResult> CreateUser(AdminCreateUserDTO input)
        {
            

            User user = new User()
            {
                Name = input.Name,
                GroupId = input.GroupId,
                Email = input.Email,
                Username = input.Username,
                Type = input.Type,
                AdditionalGroupsMapper= input?.AdditionalGroupsIds?.Select(x => new AdditionalGroupMapper() { GroupId = x }).ToList(),

            };
            await _adminsBl.Create(user);
            return Ok(new CreateUserResponseAdminDTO() { UserId = user.Id });
        }

        /// <summary>
        /// Get all users in my company
        /// </summary>
        /// <remarks>
        /// An authorized API call for CompanyAdmin user only. The token should be passed via the request header.<br/>
        ///  UserType: Basic = 1, Editor = 2, CompanyAdmin = 3 <br/>
        /// </remarks>
        /// <returns></returns>
        [Authorize(Roles = "CompanyAdmin,SystemAdmin")]
        [HttpGet]
        [Route("users")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AdminAllUsersResponseDTO))]
        public async Task<IActionResult> GetAllUsers(string key = null, int offset = 0, int limit = 20)
        {
            (IEnumerable<User> users, int totalCount) =await _adminsBl.ReadUsers(key, offset, limit, null);

            List<UserResponseAdminDTO> usersResponse = new List<UserResponseAdminDTO>();
            foreach (User user in users)
            {
                usersResponse.Add(new UserResponseAdminDTO(user));
            }

            Response.Headers.Add("x-total-count", totalCount.ToString());
            return Ok(new AdminAllUsersResponseDTO() { Users = usersResponse });
        }

        /// <summary>
        /// Update user
        /// </summary>
        /// <remarks>
        /// An authorized API call for CompanyAdmin user only. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <returns></returns>
        [Authorize(Roles = "CompanyAdmin,SystemAdmin")]
        [HttpPut]
        [Route("users/{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Update(Guid id, AdminCreateUserDTO input)
        {
            User user = new User()
            {
                Id = id,
                Name = input.Name,
                GroupId = input.GroupId,
                Email = input.Email,
                Username = input.Username,
                Type = input.Type,
                AdditionalGroupsMapper =  input?.AdditionalGroupsIds?.Select(x => new AdditionalGroupMapper() { UserId = id, GroupId = x }).ToList(),
                
            };
            
            await _adminsBl.Update(user);
            return Ok();
        }

        /// <summary>
        /// Delete user
        /// </summary>
        /// <remarks>
        /// An authorized API call for CompanyAdmin user only. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "CompanyAdmin,SystemAdmin")]
        [HttpDelete]
        [Route("users/{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Delete(Guid id)
        {
            User user = new User()
            {
                Id = id,
            };
            await _adminsBl.Delete(user);
            return Ok();
        }

        /// <summary>
        /// Update password by dev admin user
        /// </summary>
        /// <remarks>
        /// Authorized API <br/>
        /// </remarks>
        /// <returns></returns>
        [HttpPut]
        [Authorize(Roles = "Dev")]
        [Route("dev/password")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(UserTokensResponseDTO))]
        public async Task<IActionResult> UpdatePasswordByDevAdminUser(RenewPasswordByDevAdminUserDTO input)
        {
            User user = new User()
            {
                Id = input.UserId,
                Email = input.Email,
                Password = input.NewPassword,
            };
            UserTokens loginToken = await _usersBl.UpdatePasswordByDevAdminUser(user);

            return Ok(new UserTokensResponseDTO()
            {
                Token = loginToken.JwtToken,
                RefreshToken = loginToken.RefreshToken,
                AuthToken = loginToken.AuthToken
            });
        }
        #endregion


        private async Task<List<GroupResponseAdminDTO>> ReadAllGroups()
        {
            IEnumerable<Group> groups = await _adminsBl.ReadGroups();

            List<GroupResponseAdminDTO> groupsResponse = new List<GroupResponseAdminDTO>();
            foreach (Group group in groups ?? Enumerable.Empty<Group>())
            {
                groupsResponse.Add(new GroupResponseAdminDTO(group));
            }

            return groupsResponse;
        }

    }
}
