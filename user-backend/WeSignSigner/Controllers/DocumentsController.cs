/*
 * Swagger URL:
 * https://wesign3.comda.co.il/signerapi/swagger/index.html
 * https://localhost:44357/swagger/index.html
 */

using Common.Enums.PDF;
using Common.Enums.Results;
using Common.Extensions;
using Common.Handlers;
using Common.Interfaces;
using Common.Interfaces.SignerApp;
using Common.Models;
using Common.Models.Documents;
using Common.Models.Documents.Signers;
using Common.Models.Files.PDF;
using Common.Models.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using WeSignSigner.Models.Requests;
using WeSignSigner.Models.Responses;

namespace WeSignSigner.Controllers
{
#if DEBUG
    [Route("signerapi/v3/documentCollections")]
#else
    [Route("v3/documentCollections")]
#endif
    [ApiController]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class DocumentsController : ControllerBase
    {
        private readonly IDoneDocuments _doneDocuments;
        private readonly IMemoryCache _memoryCache;
        private readonly IDocuments _documents;
        private readonly ISignerValidator _validator;
        private readonly EnvironmentExtraInfo _environmentExtraInfo;
        

        public DocumentsController(IDocuments document, IDoneDocuments doneDocuments, ISignerValidator validator,
             IMemoryCache memoryCache, IOptions<EnvironmentExtraInfo> environmentExtraInfo)
        {
            _documents = document;
            _validator = validator;
            _doneDocuments = doneDocuments;
            _memoryCache = memoryCache;
            _environmentExtraInfo = environmentExtraInfo.Value;
    
        }

        /// <summary>
        /// Get pages count
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{token}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(DocumentCollectionDataResponseDTO))]
        public async Task<IActionResult> GetDocumentCollectionData(string token)
        {

            var signerTokenMapping = ValidateTokenFormat(token);
            ValidateRequestInCache(signerTokenMapping.GuidToken);
            string externalSignerAuth = "";
            if(_environmentExtraInfo != null && _environmentExtraInfo.MamazFlow &&
                !string.IsNullOrWhiteSpace( _environmentExtraInfo.HostedAppHeaderKey))
            {
                externalSignerAuth = HttpContext.Request.Headers[_environmentExtraInfo.HostedAppHeaderKey];
            }

            var signerIP = HttpContext.GetRemoteIPAddress(true);
            var documentCollectionData = await _documents.GetDocumentCollectionData(signerIP, signerTokenMapping,  externalSignerAuth);
            var documentCollection = documentCollectionData.DocumentCollection;
            signerTokenMapping = documentCollectionData.SignerTokenMapping;
            documentCollection.Documents = documentCollection?.Documents ?? Enumerable.Empty<Document>();

            var documents = new List<DocumentCountResponseDTO>();

            foreach (var document in documentCollection.Documents)
            {
                documents.Add(new DocumentCountResponseDTO()
                {
                    Id = document.Id,
                    PagesCount = document.PagesCount
                });
            }
            var response = new DocumentCollectionDataResponseDTO(documentCollection, signerTokenMapping, documentCollectionData.SignerId);
            response.Signer.AreAllOtherSignersSigned = _validator.AreAllSignersSigned(documentCollection.Signers.Where(x => x.Id != documentCollectionData.SignerId));
            response.Signer.ADName = signerTokenMapping.ADName;
            if(!string.IsNullOrEmpty(signerTokenMapping.ADName))
            {
                response.Signer.AuthToken = signerTokenMapping.AuthToken;
            }
            
            response.Documents = documents;
            response.TotalPagesCount = documentCollectionData.TotalCount;   
            return Ok(response);
        }
        [HttpGet]
        [Route("{token}/flowinfo")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(DocumentCollectionDataFlowInfoResponseDTO))]
        public async Task<IActionResult> GetDocumentCollectionDataFlowInfoForUser(string token)
        {
             
            if (!Guid.TryParse(token, out var newGuid) || newGuid == Guid.Empty)
            {
                throw new InvalidOperationException(ResultCode.InvalidToken.GetNumericString());
            }
            ValidateRequestInCache(newGuid);


            DocumentCollectionDataFlowInfo documentCollectionDataFlowInfo = await _documents.GetDocumentCollectionDataFlowInfoForUser(newGuid);
            var response = new DocumentCollectionDataFlowInfoResponseDTO(documentCollectionDataFlowInfo);
            return Ok(response);
        }



        [HttpGet]
        [Route("{token}/documents/{documentId}/html")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(DocumentCollectionHtmlDataResponseDTO))]
        public async Task<IActionResult> GetDocumentCollectionHtmlData(string token)
        {
            var signerTokenMapping = ValidateTokenFormat(token);
            ValidateRequestInCache(signerTokenMapping.GuidToken);
            

            var result = await _documents.GetDocumentCollectionHtmlData(signerTokenMapping);
            var response = new DocumentCollectionHtmlDataResponseDTO
            {
                HtmlContent = result.HTML,
                JsContent = result.JS,
                FieldsData = string.IsNullOrWhiteSpace(result.HTML)? new List<FieldData>() : result.FieldsData
            };

            return Ok(response);
        }

        /// <summary>
        /// Get information about pages range in document
        /// Page number start from 1
        /// </summary>
        /// <param name="id"></param>
        /// <param name="documentId"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{token}/documents/{documentId}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(DocumentPagesRangeResponseDTO))]
        public async Task<IActionResult> GetPagesInfo(string token, Guid documentId,string code = "", int offset = 1, int limit = 5)
        {
            // comment
            // change
            if (offset < 1)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (limit < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }
            
            var signerTokenMapping = ValidateTokenFormat(token);
            ValidateRequestInCache(signerTokenMapping.GuidToken);
            ValidateRequestInCache($"{signerTokenMapping.GuidToken}_{documentId}_tokenForDoc");

             (var document, _ , PDFFields otherFields) = await _documents.GetPagesInfoByDocumentId(signerTokenMapping, documentId, offset, limit, code);

            var response = new DocumentPagesRangeResponseDTO();
            int pageNumber = offset;
            
            foreach (var image in document.Images)
            {

                var htmlOutput = await _documents.GetOcrHtmlFromImage(image.Base64Image);
                var newPage = new DocumentPageResponseDTO
                {
                    DocumentId = document.Id,
                    Name = document.Name,

                    PageImage = $"data:image/jpeg;base64,{image.Base64Image}",
                    OcrString = htmlOutput,
                    PageWidth = image.Width,
                    PageHeight = image.Height,
                    SignerFields = document.Fields.GetPDFFieldsByPage(pageNumber),
                    OtherFields = otherFields.GetPDFFieldsByPage(pageNumber, true, false),
                    PageNumber = pageNumber
                };
                newPage.SignerFields.SignatureFields.ForEach(sf =>
                {
                    sf.Mandatory = _documents.GetTemplateSignatureFieldMandatory(sf.Name);
                });
                response.DocumentPages.Add(newPage);
                pageNumber++;
            }
            Response.Headers.Add("x-total-count", document.PagesCount.ToString());

            return Ok(response);
        }

        /// <summary>
        /// Download signed document
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{token}/download")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(FileContentResult))]
        public async Task<IActionResult> Download(string token)
        {
            var signerTokenMapping = ValidateTokenFormat(token);

            (var files, var documentCollectionName) =await _documents.Download(signerTokenMapping);
            if (files.Count == 1)
            {
                var (_, content) = files.FirstOrDefault().Value;
                Response.Headers.Add("x-file-name", WebUtility.UrlEncode($"{documentCollectionName}.pdf"));
                return File(new MemoryStream(content), MediaTypeNames.Application.Pdf, $"{documentCollectionName}.pdf");
            }

            Response.Headers.Add("x-file-name", WebUtility.UrlEncode($"{documentCollectionName}.zip"));
            var zipStream = CreateZipArchive(files);
            return File(zipStream, MediaTypeNames.Application.Octet, $"{documentCollectionName}.zip");
        }

        /// <summary>
        /// Download document appendices
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{token}/appendix/{name}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(FileContentResult))]
        public async Task<IActionResult> DownloadAppendix(string token, string name)
        {
            var signerTokenMapping = ValidateTokenFormat(token);
            var appendix = await _documents.ReadAppendix(signerTokenMapping, name);
            string contentType = MimeTypes.Auto($"{appendix.Name}{appendix.FileExtention}");
            string fileName = WebUtility.UrlEncode($"{appendix.Name}{appendix.FileExtention}");
            Response.Headers.Add("x-file-name", fileName);
            Response.Headers.Add("x-file-content", contentType);
            return File(new MemoryStream(appendix.FileContent), contentType, $"{appendix.Name}{appendix.FileExtention}");
        }

        /// <summary>
        /// Download SmartCard Desktop Client Installer
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("download/smartcard")]
        public IActionResult DownloadSmartCardDesktopClientInstaller()
        {
            byte[] installer = _doneDocuments.DownloadSmartCardDesktopClientInstaller();
            string fileName = "SmartCardDesktopClientSetup.exe";
            Response.Headers.Add("x-file-name", WebUtility.UrlEncode(fileName));

            return File(new MemoryStream(installer), MediaTypeNames.Application.Octet, fileName);
        }

        /// <summary>
        /// Update document
        /// </summary>
        /// <remarks>
        /// WeSignFieldType : TextField = 1, ChoiceField = 2, CheckBoxField = 3, RadioGroupField = 4, SignatureField = 5\
        /// DocumentOperation : Save = 1, Decline = 2, Close = 3
        /// AuthenticationType Signer1 = 1, SmartCard = 2
        /// </remarks>
        /// <param name="token"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{token}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(UpdateDocumentCollectionResponse))]
        public async Task<IActionResult> Update(string token, UpdateDocumentCollectionDTO input)
        {
            var signerTokenMapping = ValidateTokenFormat(token);
            var documentCollection = new DocumentCollection() { };
            var remoteIpAddress = HttpContext.GetRemoteIPAddress();

            documentCollection.Signers = documentCollection.Signers.Append(new Signer()
            {
                Notes = new Notes() { SignerNote = input.SignerNote },
                SignerFields = new List<SignerField>(),
                SignerAttachments = input.SignerAttachments,
                SignerAuthentication = input.SignerAuthentication,
                IPAddress = remoteIpAddress.ToString()
            });

            foreach (var documentFields in input.Documents)
            {
                documentCollection.Documents = documentCollection.Documents.Append(new Document()
                {
                    Id = documentFields.DocumentId,
                    Fields = GeneratePdfFields(documentFields)
                });
                var signer = documentCollection.Signers?.First();
                foreach (var docField in documentFields.Fields)
                {
                    var field = new SignerField()
                    {
                        DocumentId = documentFields.DocumentId,
                        FieldName = docField.FieldName,
                        FieldValue = docField.FieldValue
                    };
                    signer.SignerFields = signer.SignerFields.Append(field);
                }
            }

            var (RedirectLink, Downloadlink) = await _documents.Update(signerTokenMapping, documentCollection, input.Operation, input.UseForAllFields);
            return Ok(new UpdateDocumentCollectionResponse
            {
                RedirectUrl = RedirectLink,
                DownloadUrl = Downloadlink
            }) ;
        }


        #region Private Functions

    
        private void ValidateRequestInCache(Guid newGuid)
        {

            var item = _memoryCache.Get(newGuid);
            if (item != null)
            {
                if (item is ResultCode resultCode)
                {
                    // need Log????
                    throw new InvalidOperationException(resultCode.GetNumericString());
                }
            }
        }

        private void ValidateRequestInCache(string key)
        {

            var item = _memoryCache.Get(key);
            if (item != null)
            {
                if (item is ResultCode resultCode)
                {
                    // need Log????
                    throw new InvalidOperationException(resultCode.GetNumericString());
                }
            }
        }
        private PDFFields GeneratePdfFields(UpdateDocumentDTO item)
        {
            var pdfFields = new PDFFields();

            foreach (var field in item.Fields)
            {
                switch (field.FieldType)
                {
                    case WeSignFieldType.TextField:
                        {
                            pdfFields.TextFields.Add(new TextField() { Name = field.FieldName, Value = field.FieldValue });
                            break;
                        }
                    case WeSignFieldType.SignatureField:
                        {
                            pdfFields.SignatureFields.Add(new SignatureField() { Name = field.FieldName, Image = field.FieldValue });
                            break;
                        }
                    case WeSignFieldType.RadioGroupField:
                        {
                            pdfFields.RadioGroupFields.Add(new RadioGroupField() { Name = field.FieldName, SelectedRadioName = field.FieldValue });
                            break;
                        }
                    case WeSignFieldType.ChoiceField:
                        {
                            pdfFields.ChoiceFields.Add(new ChoiceField() { Name = field.FieldName, SelectedOption = field.FieldValue });
                            break;
                        }
                    case WeSignFieldType.CheckBoxField:
                        {
                            bool.TryParse(field.FieldValue, out bool result);
                            pdfFields.CheckBoxFields.Add(new CheckBoxField() { Name = field.FieldName, IsChecked = result });
                            break;
                        }
                    default:
                        break;

                }
            }
            return pdfFields;
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

        private MemoryStream CreateZipArchive(IDictionary<Guid, (string name, byte[] content)> files)
        {
            var zipStream = new MemoryStream();
            using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var zipItem in files)
                {
                    var entry = zip.CreateEntry($"{zipItem.Value.name}.pdf");
                    using (var entryStream = entry.Open())
                    {
                        var content = new MemoryStream(zipItem.Value.content);
                        content.CopyTo(entryStream);
                    }
                }
            }
            zipStream.Position = 0;
            return zipStream;
        }
     
        #endregion
    }
}
