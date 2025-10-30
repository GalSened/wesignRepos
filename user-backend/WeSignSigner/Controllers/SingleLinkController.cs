using System;
using System.Net;
using System.Threading.Tasks;
using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WeSignSigner.ActionFilters;
using WeSignSigner.Models.Requests;
using WeSignSigner.Models.Responses;

namespace WeSignSigner.Controllers
{
#if DEBUG
    [Route("signerapi/v3/singlelink")]
#else
    [Route("v3/singlelink")]
#endif
    //[Authorize(Policy = "CanCreateSingleLink")] // TODO
    [ApiController]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class SingleLinkController : ControllerBase
    {
        private readonly ISingleLink  _singleLinkHandler;
        public SingleLinkController(ISingleLink singleLink)
        {
            _singleLinkHandler = singleLink;
        }
        
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(DocumentResponse))]
        [ServiceFilter(typeof(SingleLinkValidation))]
        public async Task<IActionResult> CreateDocument(CreateDocumentDTO input)
        {
            var singleLink = new SingleLink()
            {
                TemplateId = input.TemplateId,
                Contact = input.SignerMeans,
                PhoneExtension = input.PhoneExtension,
                Fullname = input.Fullname
            };
            var result = await _singleLinkHandler.Create(singleLink);
            return Ok(new DocumentResponse() { Url = result.Link });
        }

        [HttpGet]
        [Route("{templateId}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(SingleLinkDataResponseDTO))]
        public async Task<IActionResult> GetData(Guid templateId)
        {
            var singleLink = new SingleLink()
            {
                TemplateId = templateId
            };

            SignleLinkGetDataResult signleLinkGetDataResult = await _singleLinkHandler.GetData(singleLink);

            return Ok(new SingleLinkDataResponseDTO { IsSmsProviderSupportGloballySend = signleLinkGetDataResult.IsSmsProviderSupportGloballySend,
                Language =  signleLinkGetDataResult.Language
            });
        }
    }
}
