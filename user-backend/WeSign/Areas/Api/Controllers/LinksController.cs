using Common.Models.Links;
using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WeSign.Models.Documents.Responses;
using WeSign.Models.Links;
using WeSign.Models.Templates;


namespace WeSign.areas.api.Controllers
{
//#if DEBUG
//    [Route("userapi/v3/links")]
//#else
//    [Route("v3/links")]
//#endif
    [ApiController]
    [Area("Api")]
    [Route("v3/[controller]")]
    //[ApiExplorerSettings(GroupName = "api")]
    [Authorize]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class LinksController : ControllerBase
    {
        private readonly ILinks _linksBl;

        public LinksController(ILinks linksBl)
        {
            _linksBl = linksBl;
        }

        /// <summary>
        /// Get current user links to sign on all documents that not sign already
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="key"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(UserSigningLinksResponseDTO))]
        public async Task<IActionResult> GetSigningLinks(string key = null, int offset = 0, int limit = 20)
        {
            if (offset < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (limit < 0)
            {
                throw new InvalidOperationException();
            }
            (var links, int totalCount) =await _linksBl.Read(key, offset, limit);

            var response = new UserSigningLinksResponseDTO(links);
            Response.Headers.Add("x-total-count", totalCount.ToString());
            return Ok(response);
        }

        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(VideoConferenceResult))]
        [Route("videoConference")]
        public async Task<IActionResult> CreateVideoConference (CreateVideoConferencesDTO createVideoConferencesDTO)
        {

            CreateVideoConference createVideoConference = new CreateVideoConference()
            {
                VideoConferenceUsers = createVideoConferencesDTO.VideoConferenceUsers,
                DocumentCollectionName = createVideoConferencesDTO.DocumentCollectionName
            };

            VideoConferenceResult videoConferenceResult =  await _linksBl.CreateVideoConference(createVideoConference);
            return Ok(videoConferenceResult);
        }

        [HttpGet]
        [Route("Template/{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(TemplateSingleLinkDTO))]
        public async Task<IActionResult> GetSingleLinkInfo(Guid id)
        {
            Template template = new Template()
            {
                Id = id
            };
            TemplateSingleLink singleLinkInfo = await _linksBl.GetSingleLinkInfo(template);
            return Ok(new TemplateSingleLinkDTO(singleLinkInfo));

        }


        [HttpPost]
        [Route("template/{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateCreateSingleLinkInfo(Guid id, TemplateSingleLinkDTO templateSingleLink)
        {
            TemplateSingleLink singleLinkInfo = new TemplateSingleLink()
            {
                TemplateId = id,
                SingleLinkAdditionalResources = templateSingleLink.SingleLinkAdditionalResources
            };

            await _linksBl.UpdateCreateSingleLinkInfo(singleLinkInfo);
            return Ok();

        }
    }
}
