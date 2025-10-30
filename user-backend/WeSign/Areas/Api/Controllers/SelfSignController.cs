namespace WeSign.areas.api.Controllers
{
    using Castle.Core.Internal;
    using Common.Enums.Results;
    using Common.Enums.Templates;
    using Common.Extensions;
    using Common.Interfaces;
    using Common.Models;
    using Common.Models.Documents;
    using Common.Models.Documents.Signers;
    using Common.Models.Documents.SplitSignature;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Annotations;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using WeSign.Models.SelfSign;
    using WeSign.Models.SelfSign.Responses;
    using WeSign.Models.Templates;

//#if DEBUG
//    [Route("userapi/v3/selfsign")]
//#else
//    [Route("v3/selfsign")]
//#endif
    [ApiController]
    [Area("Api")]
    [Route("v3/[controller]")]
    //[ApiExplorerSettings(GroupName = "api")]
    [Authorize]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class SelfSignController : ControllerBase
    {

        private readonly ISelfSign _selfSignBl;
        private readonly IDoneDocuments _doneDocuments;
        private const string SEPERATOR = "_ASEPA_";

        public SelfSignController(ISelfSign selfSignBl, IDoneDocuments doneDocuments)
        {
            _selfSignBl = selfSignBl;
            _doneDocuments = doneDocuments;
        }

        /// <summary>
        /// Create SelfSign document
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(SelfSignCountResponseDTO))]
        public async Task<IActionResult> CreateDocument(CreateSelfSignDocumentDTO input)
        {
            Guid guid = Guid.Empty;
            if (!string.IsNullOrWhiteSpace(input.SourceTemplateId))
            {
                Guid.TryParse(input.SourceTemplateId, out guid);
            }
            var selfSignTemplate = new Template()
            {
                Name = input.Name,
                Base64File = input.Base64File,
                Status = TemplateStatus.OneTimeUse,
                Id = guid

            };
            var remoteIpAddress = HttpContext.GetRemoteIPAddress();
            var documentCollection = await _selfSignBl.Create(selfSignTemplate, remoteIpAddress.ToString());
            return Ok(new SelfSignCountResponseDTO()
            {
                DocumentCollectionId = documentCollection.Id,
                DocumentId = documentCollection.Documents.First().Id,
                Name = documentCollection.Name,
                PagesCount = documentCollection.Documents.First().PagesCount
            });
        }

        /// <summary>
        /// Update SelfSign document
        /// </summary>
        /// <remarks>   
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// DocumentOperation :  Save = 1, Decline = 2, Close = 3. <br/>
        /// Close will sign document
        /// </remarks>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPut]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(SelfSignUpdateDocumentResult))]
        public async Task<IActionResult> UpdateDocument(UpdateSelfSignDocumentDTO input)
        {
            var remoteIpAddress = HttpContext.GetRemoteIPAddress();
            var documentCollection = new DocumentCollection()
            {
                Id = input.DocumentCollectionId,
                Name = input.Name,
                Documents = new List<Document>()
                {
                    new Document()
                    {
                        Id = input.DocumentId,
                        Fields = input.Fields
                    }
                },
                Signers = new List<Signer> { new Signer { SignerAuthentication = input.signerAuthentication,IPAddress = remoteIpAddress } }
            };
            SelfSignUpdateDocumentResult selfSignUpdateDocumentResult = await _selfSignBl.Update(documentCollection, input.Operation, input.UseForAllFields);
            return Ok(selfSignUpdateDocumentResult);
        }

        ///// <summary>
        ///// Update Government SelfSign document
        ///// </summary>
        ///// <remarks>   
        ///// An authorized API call. The token should be passed via the request header.<br/>
        ///// DocumentOperation :  Save = 1, Decline = 2, Close = 3. <br/>
        ///// Close will sign document
        ///// </remarks>
        ///// <param name="input"></param>
        ///// <returns></returns>
        //[HttpPut]
        //[Route("gov")]
        //[SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(SelfSignUpdateDocumentResult))]
        //public async Task<IActionResult> UpdateGovDocument(UpdateSelfSignDocumentDTO input)
        //{
        //    //TODO validation input
        //    var remoteIpAddress = HttpContext.GetRemoteIPAddress();
        //    var documentCollection = new DocumentCollection()
        //    {
        //        Id = input.DocumentCollectionId,
        //        Name = input.Name,
        //        Documents = new List<Document>()
        //        {
        //            new Document()
        //            {
        //                Id = input.DocumentId,
        //                Fields = input.Fields
        //            }
        //        },
        //        Signers = new List<Signer> { new Signer { SignerAuthentication = input.signerAuthentication, IPAddress = remoteIpAddress } }
        //    };
        //    SelfSignUpdateDocumentResult selfSignUpdateDocumentResult = await _selfSignBl.UpdateGovDocument(documentCollection);
        //    return Ok(selfSignUpdateDocumentResult);
        //}

        /// <summary>
        /// Delete SelfSign document 
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var documentCollection = new DocumentCollection
            {
                Id = id
            };
            await _selfSignBl.Delete(documentCollection);
            return Ok();
        }

        /// <summary>
        /// Download smartCard desktop client installer
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <returns></returns>
        [HttpGet]
        [Route("download/smartcard")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(FileContentResult))]
        public IActionResult DownloadSmartCardDesktopClientInstaller()
        {
            byte[] installer = _doneDocuments.DownloadSmartCardDesktopClientInstaller();
            string fileName = "SmartCardDesktopClientSetup.exe";
            Response.Headers.Add("x-file-name", WebUtility.UrlEncode(fileName));

            return File(new MemoryStream(installer), MediaTypeNames.Application.Octet, fileName);
        }

        [HttpPost]
        [Route("sign")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(FileContentResult))]
        public async Task<IActionResult> SignUsingSigner1(Signer1FileSigingDTO signer1FileSigingDTO)
        {
            var signer1FileSiging = new Signer1FileSiging
            {
                Base64File = signer1FileSigingDTO.Base64File,
                SigingFileType = signer1FileSigingDTO.SigingFileType,
                Signer1Credential = signer1FileSigingDTO.Signer1Credential
            };

            Signer1FileSigingResult result = await _selfSignBl.SignFileUsingSigner1(signer1FileSiging);
            string fileName = $"Signed_{signer1FileSigingDTO.FileName}";
            Response.Headers.Add("x-file-name", WebUtility.UrlEncode(fileName));
            return File(new MemoryStream(result.Base64SignedFile), MediaTypeNames.Application.Octet, fileName);
        }
        
        [HttpPost]        
        [Route("CreateSmartCardSigningFlow")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateSmartCardSigningFlow(SmartCardSigningFlowDTO smartCardSigningFlowDTO)
        {
            var smartCardSigningFlow = new SmartCardSigningFlow();
          //  smartCardSigningFlow.Token = smartCardSigningFlowDTO.Token;
            smartCardSigningFlowDTO.Fields.ForEach(x =>
            {
                smartCardSigningFlow.Fields.Add(
                    new SmartCardSignFlowFields
                    {
                        DocumentId = x.DocumentId,
                        FieldName = x.FieldName,
                        Image = x.Image,
                    }
                    );
            });

            // add validator 
           Guid roomId =  await _selfSignBl.CreateSmartCardSigningFlow(smartCardSigningFlow);
            return Ok();
        }


        [HttpPost]
        [Route("sign/verify")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> VerifySigner1Credential(SignerAuthentication input)
        {
            await _selfSignBl.VerifySigner1Credential(input);
            return Ok();
        }



        [HttpPost]
        [Route("CheckidentityFlowEIDASSign")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IdentityCheckFlowResult))]
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
                if (items.Count() != 3)
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
                FieldName = fieldName,
                DocumentId = documentId
            };
            SplitDocumentProcess splitDocumentProcess = await _selfSignBl.ProcessAfterSignerAuth(identityFlow);
            SplitDocumentProcessDTO responseDTO = new SplitDocumentProcessDTO();
            responseDTO.ProcessStep = splitDocumentProcess.ProcessStep;
            responseDTO.Url = splitDocumentProcess.Url;
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
