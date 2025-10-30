using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces.SignerApp;
using Common.Models;
using Common.Models.Documents.Signers;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Net;
using System.Threading.Tasks;
using WeSignSigner.Models.Responses;

namespace WeSignSigner.Controllers
{
#if DEBUG
    [Route("signerapi/v3/contacts")]
#else
    [Route("v3/contacts")]
#endif
    [ApiController]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class ContactsController : ControllerBase
    {
        
        private readonly IContacts _contacts;

        public ContactsController(IContacts contacts)
        {
         
            _contacts = contacts;
        }

        [HttpGet]
        [Route("signatures/{token}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(SignaturesImagesDTO))]
        public async Task<IActionResult> ReadSignaturesImages(string token)
        {
            var signerTokenMapping = ValidateTokenFormat(token);
            var response = await _contacts.ReadSignaturesImages(signerTokenMapping);

            return Ok(new SignaturesImagesDTO { SignaturesImages = response });
        }

        [HttpPut]
        [Route("signatures/{token}")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateSignaturesImages(string token, SignaturesImagesDTO input)
        {
            var signerTokenMapping = ValidateTokenFormat(token);
            await _contacts.UpdateSignaturesImages(signerTokenMapping,input.SignaturesImages);

            return Ok();
        }

        #region Private Functions

        private SignerTokenMapping ValidateTokenFormat(string token)
        {
            if (!Guid.TryParse(token, out var newGuid))
            {
                throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
            }
            var signerTokenMapping = new SignerTokenMapping()
            {
                GuidToken = newGuid
            };
            return signerTokenMapping;
        }
        
        #endregion
    }
}
