using Microsoft.AspNetCore.Mvc;
using HistoryIntegratorService.Common.Interfaces;
using HistoryIntegratorService.Common.Models;
using HistoryIntegratorService.Requests;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace HistoryIntegratorService.Controllers
{

#if DEBUG
    [Route("historyintegrator/v3/documentcollections")]
#else
    [Route("v3/documentcollections")]
#endif
    [ApiController]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class DocumentCollectionController : ControllerBase
    {
        private readonly IDocumentCollection _documentCollection;
        private const string APP_KEY = "AppKey";

        public DocumentCollectionController(IDocumentCollection documentCollection)
        {
            _documentCollection = documentCollection;
        }

        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<DeletedDocumentCollection>))]
        public IActionResult Read([FromQuery] DeletedDocumentCollectionRequest deletedDocRequest)
        {
            if (Request.Headers.TryGetValue(APP_KEY, out var appKey))
            {
                var data = _documentCollection.Read(appKey.ToString(), deletedDocRequest, out long totalCount);
                Response.Headers.Add("x-total-count", totalCount.ToString());
                return Ok(data);
            }
            return Unauthorized();
        }

        [HttpPost]
        public IActionResult Write(DeletedDocumentCollection deletedDoc)
        {
            if (Request.Headers.TryGetValue(APP_KEY, out var appKey))
            {
                _documentCollection.Create(appKey.ToString(), deletedDoc);
                return Ok();
            }
            return Unauthorized();
        }
    }
}
