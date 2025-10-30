
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.Interfaces;
using Common.Models;
using Common.Models.Configurations;
using iTextSharp.text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WeSign.Models.Configuration;

namespace WeSign.areas.ui.Controllers
{
    //#if DEBUG
    //    [Route("userui/v3/configuration")]
    //#else
    //    [Route("ui/v3/configuration")]
    //#endif
    [ApiController]
    [Area("Ui")]
    [Route("Ui/v3/[controller]")]
    //[ApiExplorerSettings(GroupName = "ui")]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class ConfigurationController : Controller
    {
        private readonly IConfiguration _configuration;

        public ConfigurationController(IConfiguration configuratiuon)
        {
            _configuration  = configuratiuon;
        }

        /// <summary>
        /// Read init configuration 
        /// </summary>
        /// <remarks>
        /// Not authorized API <br/>
        /// </remarks>
        /// <returns></returns>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(InitConfigurationDTO))]
        public async Task< IActionResult> ReadInitConfiguration()
        {
            Common.Models.Configurations.Configuration config = await _configuration.ReadAppConfiguration();

            return Ok(new InitConfigurationDTO {
                EnableFreeTrailUsers = false ,  //config.EnableFreeTrailUsers For now not show it ,
                EnableTabletsSupport = config.EnableTabletsSupport,
                EnableSigner1ExtraSigningTypes = config.EnableSigner1ExtraSigningTypes,
                ShouldUseReCaptchaInRegistration = config.ShouldUseReCaptchaInRegistration,
                ShouldUseSignerAuth = config.ShouldUseSignerAuth,
                ShouldUseSignerAuthDefault = config.ShouldUseSignerAuthDefault,
                EnableShowSSOOnlyInUserUI = config.EnableShowSSOOnlyInUserUI,
            });
        }

        /// <summary>
        /// Read tablets list 
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [Route("tablets")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(TabletsConfigurationDTO))]
        public async Task<IActionResult> ReadTablesConfiguration(string key)
        {
            IEnumerable<Tablet> tablets =await _configuration.ReadTablesConfiguration(key);

            return Ok(tablets);
        }
    }
}
