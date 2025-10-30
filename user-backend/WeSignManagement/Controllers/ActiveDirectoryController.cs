using System;
using System.Collections.Generic;
using System.Net;
using Common.Interfaces.ManagementApp;
using Common.Models;
using Common.Models.ActiveDirectory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Common.Extensions;
using Swashbuckle.AspNetCore.Annotations;
using WeSignManagement.Models.ActiveDirectory;
using Common.Models.Configurations;
using System.Linq;
using System.Threading.Tasks;

namespace WeSignManagement.Controllers
{
#if DEBUG
    [Route("managementapi/v3/ActiveDirectory")]
#else
    [Route("v3/ActiveDirectory")]
#endif
    [Authorize(Roles = "SystemAdmin")]
    [ApiController]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class ActiveDirectoryController : ControllerBase
    {

        private readonly IManagementBL _bl;

        public ActiveDirectoryController(IManagementBL bl)
        {
            _bl = bl;
        }

        [HttpGet]
        [Route("groups")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(ADGroupsResponseDTO))]
        public async Task<IActionResult> ReadActiveDirectoryGroups()
        {
            var activeDirectoryConfig = await _bl.ActiveDirectory.Read();
            List<string> groups = new List<string>();
            if (!string.IsNullOrEmpty(activeDirectoryConfig.Host) && !string.IsNullOrEmpty(activeDirectoryConfig.Domain))
            {
                (var resultGroups, var isSucsses) = await _bl.ActiveDirectory.ReadADGroups(activeDirectoryConfig);
                if(isSucsses)
                    groups = resultGroups.ToList();
            }

            return Ok(new ADGroupsResponseDTO() { ActiveDirectoryGroups = groups});
        }

        /// <summary>
        /// Read Active Directory configuration for companyId
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("configuration")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(ActiveDirectoryConfiguration))]
        public async Task<IActionResult> Read()
        {

           var  activeDirectoryConfig = await _bl.ActiveDirectory.Read();

            return Ok(activeDirectoryConfig);
        }


        #region Private Functions

        //private IEnumerable<ActiveDirectoryGroup> GetActiveDirectoryGroups(IEnumerable<ADGroupDTO> groups)
        //{
        //    var result = new List<ActiveDirectoryGroup>();
        //    groups.ForEach(x => result.Add(new ActiveDirectoryGroup
        //    {
        //        Name = x.Name,
        //        GroupType = x.GroupType
        //    }));

        //    return result;
        //}
        
        #endregion
    }
}
