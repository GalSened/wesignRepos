using Common.Models;
using Common.Interfaces.SignerApp;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace WeSignSigner.Controllers
{
#if DEBUG
    [Route("signerapi/v3/logs")]
#else
    [Route("v3/logs")]
#endif
    [ApiController]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class LogsController : ControllerBase
    {
        private readonly ILogs _logs;

        public LogsController(ILogs logs)
        {
            _logs = logs;
        }

        //[HttpPost]        
        //public IActionResult Create(LogMessageDTO input)
        //{
           
        //    var logMessage = new LogMessage
        //    {
        //        Message = $"{input.ApplicationName} | Client IP = [{input.ClientIP}] | {input.Message}",
        //        LogLevel = input.LogLevel,
        //        TimeStamp = input.TimeStamp,
        //        Exception = input.Exception
        //    };
        //    _logs.Create(input.Token, logMessage);

        //    return Ok();
        //}
    }
}
