using Microsoft.AspNetCore.Mvc;
using HistoryIntegratorService.Common.Interfaces;
using HistoryIntegratorService.Common.Models;
using HistoryIntegratorService.Common.Models.ManagementReports;
using HistoryIntegratorService.Requests;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace HistoryIntegratorService.Controllers
{
#if DEBUG
    [Route("historyintegrator/v3/managementreports")]
#else
    [Route("v3/managementreports")]
#endif
    [ApiController]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class ManagementReportsController : ControllerBase
    {
        private readonly IManagementReports _managementReports;
        private const string APP_KEY = "AppKey";

        public ManagementReportsController(IManagementReports managementReports)
        {
            _managementReports = managementReports;
        }

        [HttpGet]
        [Route("UsageByUserDetails")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<UsageByUserReport>))]
        public IActionResult ReadUsageByUserDetails([FromQuery] UsageByUserDetailsRequest usageByUserDetailsRequest)
        {
            if (Request.Headers.TryGetValue(APP_KEY, out var appKey))
            {
                var data = _managementReports.ReadUsageByUserDetails(appKey.ToString(), usageByUserDetailsRequest);
                return Ok(data);
            }
            return Unauthorized();
        }

        [HttpGet]
        [Route("UsageByCompanyAndGroups")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<UsageByCompanyReport>))]
        public IActionResult ReadUsageByCompanyAndGroups([FromQuery] UsageByCompanyAndGroupsRequest usageByCompanyAndGroupsRequest)
        {
            if (Request.Headers.TryGetValue(APP_KEY, out var appKey))
            {
                var data = _managementReports.ReadUsageByCompanyAndGroups(appKey.ToString(), usageByCompanyAndGroupsRequest);
                return Ok(data);
            }
            return Unauthorized();
        }

        [HttpGet]
        [Route("UsageByCompanyAndSignatureTypes")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(UsageBySignatureTypeReport))]
        public IActionResult ReadUsageByCompanyAndSignatureTypes([FromQuery] UsageByCompanyAndSignatureTypesRequest usageByCompanyAndSignatureTypesRequest)
        {
            if (Request.Headers.TryGetValue(APP_KEY, out var appKey))
            {
                var data = _managementReports.ReadUsageByCompanyAndSignatureTypes(appKey.ToString(), usageByCompanyAndSignatureTypesRequest);
                return Ok(data);
            }
            return Unauthorized();
        }
    }
}
