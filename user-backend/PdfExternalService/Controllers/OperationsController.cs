using Common.Enums.Results;
using Common.Extensions;
using Common.Models;
using Common.Models.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PdfExternalService.Interfaces;
using PdfExternalService.Models;
using PdfExternalService.Models.DTO;
using Swashbuckle.AspNetCore.Annotations;
using System.Net;

namespace PdfExternalService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    
    public class OperationsController : ControllerBase
    {
        
        private readonly IPdfOperations _pdfOperationsBL;

        public OperationsController(IPdfOperations pdfOperationsBL)
        {
            _pdfOperationsBL = pdfOperationsBL;
            
        }

        [HttpPost("mergefiles")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(MergeResultDTO))]
        public IActionResult MergePdfFiles(FileMergeDTO fileMergeDTO)
        {
            FileMergeObject fileMergeObject = new FileMergeObject()
            {
                OperationId = Guid.NewGuid(),
                Base64Files = fileMergeDTO.Base64Files,
                APIKey = fileMergeDTO.APIKey
            };
            var result = _pdfOperationsBL.MergeFiles(fileMergeObject);
            MergeResultDTO mergeResult = new MergeResultDTO()
            {
                Document = Convert.ToBase64String(result)
            };
            return Ok(mergeResult);

        }
    }
}
