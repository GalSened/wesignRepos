using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Extensions;
using Common.Interfaces;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;
using Common.Models;

namespace WSE_ADAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class ValuesController : ControllerBase
    {
        private IEncryptor _encryptor;

        public ValuesController(IEncryptor encryptor)
        {
            _encryptor = encryptor;
        }

        [HttpGet("")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(string))]
        public IActionResult Encrypt(string val)
        {
            return Ok(_encryptor.Encrypt(val));
        }

        [HttpGet("")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(string))]
        public IActionResult Decrypt(string val)
        {
            return Ok(_encryptor.Decrypt(val));
        }
    }
}
