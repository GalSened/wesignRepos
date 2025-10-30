using Microsoft.AspNetCore.Mvc;
using HistoryIntegratorService.Common.Interfaces;
using HistoryIntegratorService.Common.Models;
using HistoryIntegratorService.Common.Models.UserReports;
using HistoryIntegratorService.Requests;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace HistoryIntegratorService.Controllers
{
#if DEBUG
    [Route("historyintegrator/v3/wesignreports")]
#else
    [Route("v3/wesignreports")]
#endif
    [ApiController]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class WeSignReportsController : ControllerBase
    {
        private readonly IWeSignReports _weSignReports;
        private const string APP_KEY = "AppKey";

        public WeSignReportsController(IWeSignReports weSignReports)
        {
            _weSignReports = weSignReports;
        }

        [HttpGet]
        [Route("UserUsageDataReports")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<UsageDataReport>))]
        public IActionResult ReadUserUsageDataReports([FromQuery] UserUsageDataRequest userUsageDataRequest)
        {
            if (Request.Headers.TryGetValue(APP_KEY, out var appKey))
            {
                var data = _weSignReports.ReadUserUsageDataReports(appKey.ToString(), userUsageDataRequest);
                return Ok(data);
            }
            return Unauthorized();
        }
    }
}