using System.Net;
using System.Threading.Tasks;
using Common.Enums.License;
using Common.Interfaces.License;
using Common.Interfaces.ManagementApp;
using Common.Models;
using Common.Models.License;
using Common.Models.ManagementApp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WeSignManagement.Models.License;

namespace WeSignManagement.Controllers
{

#if DEBUG
    [Route("managementapi/v3/licenses")]
#else
    [Route("v3/licenses")]
#endif
    [Authorize(Roles = "SystemAdmin,Dev")]
    [ApiController]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class LicensesController : ControllerBase
    {
        private readonly ILicense _license;

        public LicensesController(ILicense license)
        {
            _license = license;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks> LicenseStatus : Succsess = 0, Activated = 1, SentToDMZ = 2, Failed = 3
        /// </remarks>
        /// <param name="userInfo"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(GenerateLicenseKeyResponse))]
        public async Task<IActionResult> GenerateLicenseKey(UserInfoDTO userInfo)
        {
            UserInfo info = new UserInfo()
            {
                Id = userInfo.Id,
                Company = userInfo.Company,
                Email = userInfo.Email,
                Name = userInfo.Name,
                Phone = userInfo.Phone

            };
            var response = await _license.GenerateLicense(info);

            return Ok(response);
        }

        /// <summary>
        /// Activate License
        /// </summary>
        /// <param name="activateLicense"></param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(LicenseStatus))]
        public IActionResult AcitvateLicense(ActivateLicenseDTO activateLicense)
        {
            var response = _license.ActivateLicense(activateLicense.License);

            return Ok(response);
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(LicenseInfoReponse))]
        public async Task<IActionResult> LicenseInformationAndUsing()
        {
            (IWeSignLicense licenseLimits, LicenseCounters licenseUsage) =await _license.GetLicenseInformationAndUsing( true);
            var response = new LicenseInfoReponse
            {
                LicenseLimits = licenseLimits,
                LicenseUsage = licenseUsage
            };

            return Ok(response);
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(LicenseInfoReponse))]
        [Route("simpleInfo")]
        public IActionResult LicenseInformation()
        {
            IWeSignLicense licenseLimits = _license.ReadLicenseInformation();
            var response = new LicenseInfoReponse
            {
                LicenseLimits = licenseLimits
            };

            return Ok(response);
        }

    }
}