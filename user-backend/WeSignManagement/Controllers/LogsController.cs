using Common.Enums.Logs;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces.ManagementApp;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using WeSignManagement.Models.Logs;

namespace WeSignManagement.Controllers
{
#if DEBUG
    [Route("managementapi/v3/logs")]
#else
    [Route("v3/logs")]
#endif
    [Authorize(Roles = "SystemAdmin")]
    [ApiController]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class LogsController : ControllerBase
    {

        private readonly IManagementBL _bl;

        public LogsController(IManagementBL bl)
        {
            _bl = bl;
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllLogsResponseDTO))]
        public IActionResult Read(LogApplicationSource source = LogApplicationSource.UserApp, string key = null, string from = null, string to = null, LogLevel logLevel = LogLevel.All, int offset = 0, int limit = 20)
        {
            if (offset < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (limit < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }
            if (from != null && to != null && (!DateTime.TryParse(from, out DateTime dateFrom) || !DateTime.TryParse(to, out DateTime dateTo)))
            {
                throw new InvalidOperationException(ResultCode.InvalidDateTimeFormat.GetNumericString());
            }
            var logs = _bl.Logs.Read(source, key, from, to, logLevel, offset, limit, out int totalCount);
            var response = new List<LogMessageDTO>();
            foreach (var log in logs)
            {
                response.Add(new LogMessageDTO(log));
            }
            Response.Headers.Add("x-total-count", totalCount.ToString());

            return Ok(new AllLogsResponseDTO { Logs = response });
        }

    }
}