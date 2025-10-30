using Common.Extensions;
using Common.Models;
using Common.Models.Documents.Signers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System;
using System.Net;
using System.Threading.Tasks;
using WeSign.Models.Signers;
using Common.Interfaces;
using Common.Enums.Documents;

namespace WeSign.areas.api.Controllers
{
//#if DEBUG
//    [Route("userapi/v3/signers")]
//#else
//    [Route("v3/signers")]
//#endif
    [ApiController]
    [Area("Api")]
    [Route("v3/[controller]")]
    //[ApiExplorerSettings(GroupName = "api")]
    [Authorize]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class SignersController : ControllerBase
    {
        private readonly IContacts _contacts;
        private readonly ISigners _signers;

        public SignersController(IContacts contacts, ISigners signers)
        {
            _contacts = contacts;
            _signers = signers;
        }

        /// <summary>
        /// Replace old signer with new signer
        /// </summary>
        /// <param name="id"></param>
        /// <param name="signerId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{id}/signer/{signerId}/replace")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> ReplaceSigner(Guid id, Guid signerId, ReplaceSignerWithDetailsDTO input)
        {
            await _signers.ReplaceSigner(id, signerId, input.NewSignerName, input.NewSignerMeans, input.NewNotes, input.NewOtpMode, input.NewOtpIdentification, input.NewAuthenticationMode);
            return Ok();
        }
    }
}