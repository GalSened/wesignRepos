using Common.Enums.Results;
using Common.Extensions;
using Common.Interfaces.SignerApp;
using Common.Models;
using Common.Models.Documents.Signers;
using Common.Models.Documents.SplitSignature;
using CTHashSigner;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WeSignSigner.Models.Requests;
using WeSignSigner.Models.Responses;

namespace WeSignSigner.Controllers
{
#if DEBUG
    [Route("signerapi/v3/Identification")]
#else
    [Route("v3/Identification")]
#endif
    [ApiController]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class IdentificationController : ControllerBase
    {
        private readonly ISignerIdentity _signerIdentity;
        private const string SEPERATOR = "_ASEPA_";
        public IdentificationController(ISignerIdentity signerIdentity)
        {
            _signerIdentity = signerIdentity;
        }

        [HttpPost]
        [Route("CreateidentityFlowEIDASSign")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IdentityFlowResponseDTO))]
        public IActionResult CreateidentityFlowEIDASSign(IdentityFlowDTO input)
        {
            if (input == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
            }
            SignerTokenMapping signerTokenMapping = ValidateTokenFormat(input.SignerToken);
            
            IdentityCreateFlowResult identityFlowResult =  _signerIdentity.GetURLForStartAuthForEIdasFlow(signerTokenMapping);
            IdentityFlowResponseDTO responseDTO = new IdentityFlowResponseDTO { IdentityFlowURL = identityFlowResult.Url };
            return Ok(responseDTO);
        }

        [HttpPost]
        [Route("CheckidentityFlowEIDASSign")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IdentityCheckFlowResultDTO))]
        public async Task<IActionResult> CheckIdentityFlowEIDASSign(IdentityFlowDTO input)
        {
            if (input == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
            }
            string signerToken = input.SignerToken;
            string fieldName = "";
            Guid documentId = Guid.Empty;
            if (signerToken.Contains(SEPERATOR))
            {
                var items = signerToken.Split(SEPERATOR);
                if(items.Count() != 3)
                {
                    return BadRequest();
                }
                if (!Guid.TryParse(items[2], out documentId))
                {
                    return BadRequest();
                }
                signerToken = items[0];
                fieldName = items[1];
                
            }
            

            SignerTokenMapping signerTokenMapping = ValidateTokenFormat(signerToken);
            IdentityFlow identityFlow = new IdentityFlow
            {
                Code = input.Code,
                SignerToken = signerTokenMapping.GuidToken,
               FieldName =  fieldName,
                DocumentId = documentId
            };
            SplitDocumentProcess splitDocumentProcess =  await _signerIdentity.ProcessAfterSignerAuth(identityFlow);
            SplitDocumentProcessDTO responseDTO = new SplitDocumentProcessDTO() ;
            responseDTO.ProcessStep = splitDocumentProcess.ProcessStep;
            responseDTO.Url = splitDocumentProcess.Url;
            return Ok(responseDTO);
        }


        [HttpGet]
        [Route("{token}/oauthidentity/{code}/code")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> OauthIdentityFlow(string token, string code)
        {

            var signerTokenMapping = ValidateTokenFormat(token);
            var signerIP = HttpContext.GetRemoteIPAddress();
            //var documentCollectionData = await _documents.GetDocumentCollectionData(signerIP, signerTokenMapping);
           // _documents.ValidateOauthCodeFlow(documentCollectionData.DocumentCollection, code, documentCollectionData.SignerId);
            return Ok();

        }

        [HttpPost]
        [Route("CreateidentityFlow")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IdentityFlowResponseDTO))]
        public async Task<IActionResult> CreateidentityFlow(IdentityFlowDTO input)
        {
            if (input == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
            }
            SignerTokenMapping signerTokenMapping = ValidateTokenFormat(input.SignerToken);
            IdentityCreateFlowResult identityFlowResult = await _signerIdentity.CreateIdentityFlow(signerTokenMapping);
            IdentityFlowResponseDTO responseDTO = new IdentityFlowResponseDTO { IdentityFlowURL = identityFlowResult.Url };
            return Ok(responseDTO);
        }

        [HttpPost]
        [Route("CheckIdentityFlow")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IdentityCheckFlowResultDTO))]
        public async Task<IActionResult> CheckIdentityFlow(IdentityFlowDTO input)
        {
            if (input == null)
            {
                throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
            }

            SignerTokenMapping signerTokenMapping = ValidateTokenFormat(input.SignerToken);

            IdentityCheckFlowResult identityFlowResult = await _signerIdentity.CheckIdentityFlow(signerTokenMapping, input.Code);
            IdentityCheckFlowResultDTO responseDTO = new IdentityCheckFlowResultDTO { Token = identityFlowResult.Token };
            return Ok(responseDTO);
        }

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


    }
}
