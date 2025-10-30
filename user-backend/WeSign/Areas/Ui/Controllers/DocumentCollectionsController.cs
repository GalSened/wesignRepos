namespace WeSign.areas.ui.Controllers
{
    using Common.Enums;
    using Common.Enums.Contacts;
    using Common.Enums.Documents;
    using Common.Enums.Results;
    using Common.Extensions;
    using Common.Interfaces;
    using Common.Models;
    using Common.Models.Documents;
    using Common.Models.Documents.Signers;
    using Common.Models.Files.PDF;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Serilog;
    using Swashbuckle.AspNetCore.Annotations;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Net.Mime;
    using System.Text.RegularExpressions;
    using WeSign.Models.Documents;
    using WeSign.Models.Documents.Responses;
    using System.ComponentModel.DataAnnotations;
    using Common.Enums.Users;
    using Microsoft.AspNetCore.Http.Features;
    using Castle.Core.Internal;
    using System.IO.Abstractions;
    using Microsoft.FeatureManagement;
    using WeSign.Features;
    using Microsoft.FeatureManagement.Mvc;
    using WeSign.Models.Contacts.Responses;
    using System.Threading.Tasks;
    using Hangfire.Logging;
    using Google.Cloud.Vision.V1;
    using Google.Protobuf;
    using Newtonsoft.Json.Linq;

//#if DEBUG
//    [Route("userui/v3/documentcollections")]
//#else
//    [Route("ui/v3/documentcollections")]
//#endif
    [ApiController]
    [Area("Ui")]
    [Route("Ui/v3/[controller]")]
    //[ApiExplorerSettings(GroupName = "ui")]
    [Authorize]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class DocumentCollectionsController : ControllerBase
    {
        private readonly IDocumentCollections _documentCollectionsBl;
        private readonly IDater _dater;
        private ILogger _logger;
        private readonly IFeatureManager _featureManager;
        private readonly ITemplates _templatesBl;
        private readonly IContacts _contactsBl;

        public DocumentCollectionsController(ILogger logger, IDocumentCollections documentCollectionsBl , ITemplates templatesBl,
            IContacts contactsBl,  IDater dater, IFeatureManager featureManager)
        {

            _documentCollectionsBl = documentCollectionsBl;
            _dater = dater;
            _logger = logger;
            _featureManager = featureManager;
            _templatesBl = templatesBl;
            _contactsBl = contactsBl;

        }

        /// <summary>
        /// Get all document collections by search criteria
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/><br/>
        /// Request <br/>
        /// offset number start from 1 <br/> 
        /// limit number start from 1 <br/> <br/>
        /// Response <br/>
        /// SendingMethod: SMS = 1, Email = 2, Tablet = 3 <br/>
        /// DocumentStatus: Created = 1, Sent = 2, Viewed = 3, Signed = 4, Declined = 5, SendingFailed = 6, Deleted = 7, Canceled = 8 <br/>
        /// Mode: OrderedGroupSign = 1, GroupSign = 2, Online = 3, SelfSign = 100 <br/>
        /// SignerStatus:  Sent = 1, Viewed = 2, Signed = 3, Rejected = 4 <br/>
        /// </remarks>
        /// <param name="sent"></param>
        /// <param name="viewed"></param>
        /// <param name="signed"></param>
        /// <param name="declined"></param>
        /// <param name="userId"></param>
        /// <param name="key"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="searchParameter"></param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllDocumentCollectionsResposneDTO))]
        public async Task<IActionResult> Read(string key = null, bool sent = true, bool viewed = true, bool signed = true, bool declined = true, bool sendingFailed = true, bool canceled = true, string userId = null, string from = null, string to = null, int offset = 0, int limit = 20, SearchParameter searchParameter = SearchParameter.DocumentName)
        {
            if (offset < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (limit < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }
            if (from != null && to != null && (!DateTime.TryParse(from, out DateTime dateFrom) || !DateTime.TryParse(to, out DateTime dateTo)))
            {
                throw new InvalidOperationException(ResultCode.InvalidDateTimeFormat.GetNumericString());
            }
            (var documentCollections, int totalCount) =await _documentCollectionsBl.Read(key, sent, viewed, signed, declined, sendingFailed, canceled, from, to, offset, limit, searchParameter);

            var response = new List<DocumentCollectionResposneDTO>();
            foreach (var documentCollection in documentCollections)
            {
                response.Add(new DocumentCollectionResposneDTO(documentCollection));
            }
            Response.Headers.Add("x-total-count", totalCount.ToString());

            return Ok(new AllDocumentCollectionsResposneDTO() { DocumentCollections = response });
        }

        /// <summary>
        /// Download document collection
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// If documentCollection contain 1 document, it will download as PDF file. <br/>
        /// If documentCollection contain more than 1 document, it will download as ZIP file. <br/>
        /// </remarks>
        /// <param name="id">Document Collection ID</param>
        /// <returns>File stream of PDF or ZIP</returns>
        [HttpGet]
        [Route("{id}")]
        [Produces("application/pdf", "application/json", "application/octet-stream")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(FileContentResult))]
        public async Task<IActionResult> DownloadDocument(Guid id)
        {
            var documentCollection = new DocumentCollection()
            {
                Id = id
            };
            var files = await _documentCollectionsBl.Download(documentCollection);
            if (files.Count == 1)
            {
                var file = files.FirstOrDefault().Value;
                Response.Headers.Add("x-file-name", WebUtility.UrlEncode($"{documentCollection.Name.Replace(".", "_")}.pdf"));
                return File(new MemoryStream(file.content), MediaTypeNames.Application.Pdf, $"{documentCollection.Name.Replace(".", "_")}.pdf");
            }

            Response.Headers.Add("x-file-name", WebUtility.UrlEncode($"{documentCollection.Name.Replace(".", "_")}.zip"));
            var zipStream = CreateZipArchive(files);
            return File(zipStream, MediaTypeNames.Application.Octet, $"{documentCollection.Name.Replace(".", "_")}.zip");
        }

        [HttpPost]
        [Route("downloadbatch")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(FileContentResult))]
        public async Task<IActionResult> DownloadDocuments(DownloadBatchRequestDTO documentBatchRequestDTO)
        {
            var documentCollections = new List<DocumentCollection>();
            foreach (var id in documentBatchRequestDTO.Ids)
            {
                documentCollections.Add(new DocumentCollection() { Id = Guid.Parse(id) });
            };
            var files = await _documentCollectionsBl.DownloadAllSelected(documentCollections);
            var fileDownloadNameWithoutExtention = $"{documentCollections.FirstOrDefault().Name.Replace(".", "_")}_{files.Count()}_files";

            if (files.Count == 1)
            {
                var file = files.FirstOrDefault().Value;

                var isZipFile = file.content.Length > 0 && file.content[0] == 0x50 && file.content[1] == 0x4b;

                if (isZipFile)
                {
                    Response.Headers.Add("x-file-name", WebUtility.UrlEncode($"{fileDownloadNameWithoutExtention.Replace(".", "_")}.zip"));
                    var zipFileStream = new MemoryStream(file.content);
                    return File(zipFileStream, MediaTypeNames.Application.Octet, $"{fileDownloadNameWithoutExtention.Replace(".", "_")}.zip");
                }
                else
                {
                    Response.Headers.Add("x-file-name", WebUtility.UrlEncode($"{fileDownloadNameWithoutExtention.Replace(".", "_")}.pdf"));
                    return File(new MemoryStream(file.content), MediaTypeNames.Application.Pdf, $"{fileDownloadNameWithoutExtention.Replace(".", "_")}.pdf");
                }
            }
            Response.Headers.Add("x-file-name", WebUtility.UrlEncode($"{fileDownloadNameWithoutExtention.Replace(".", "_")}.zip"));
            var zipStream = CreateZipArchive(files);
            return File(zipStream, MediaTypeNames.Application.Octet, $"{fileDownloadNameWithoutExtention.Replace(".", "_")}.zip");
        }


        [HttpGet]
        [Route("{id}/ExtraInfo/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(DownloadDTO))]
        public async Task<IActionResult> DownloadDocumentExtraInfoJson(Guid id)
        {
            var documentCollection = new DocumentCollection()
            {
                Id = id
            };
            var files = await _documentCollectionsBl.Download(documentCollection);

            DownloadExtraInfoDTO result = new DownloadExtraInfoDTO();
            if (files.Count == 1)
            {

                var file = files.FirstOrDefault().Value;
                var fileResult = new FilesExtraInfoDTO();
                fileResult.Ext = "pdf";
                fileResult.Name = file.name;
                fileResult.TemplateId = file.templateId;
                fileResult.Data = Convert.ToBase64String(file.content);
                result.Files.Add(fileResult);
            }
            else
            {
                foreach (var file in files)
                {
                    var fileResult = new FilesExtraInfoDTO();
                    fileResult.Ext = "pdf";
                    fileResult.Name = file.Value.name;
                    fileResult.TemplateId = file.Value.templateId;
                    fileResult.Data = Convert.ToBase64String(file.Value.content);
                    result.Files.Add(fileResult);
                }
            }

            return Ok(result);

        }

        [HttpGet]
        [Route("{id}/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(DownloadDTO))]
        public async Task<IActionResult> DownloadDocumentJson(Guid id)
        {
            var documentCollection = new DocumentCollection()
            {
                Id = id
            };
            var files = await _documentCollectionsBl.Download(documentCollection);
            DownloadDTO result = new DownloadDTO();
            if (files.Count == 1)
            {

                var file = files.FirstOrDefault().Value;
                var fileResult = new FilesDTO();
                fileResult.Ext = "pdf";
                fileResult.Name = file.name;

                fileResult.Data = Convert.ToBase64String(file.content);
                result.Files.Add(fileResult);
            }
            else
            {
                foreach (var file in files)
                {
                    var fileResult = new FilesDTO();
                    fileResult.Ext = "pdf";
                    fileResult.Name = file.Value.name;

                    fileResult.Data = Convert.ToBase64String(file.Value.content);
                    result.Files.Add(fileResult);
                }
            }

            return Ok(result);
        }

        /// <summary>
        /// Download signer attachments of specified document collection id 
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="signerId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/signer/{signerId}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(FileContentResult))]
        public async Task<IActionResult> DownloadAttchment(Guid id, Guid signerId)
        {
            var documentCollection = new DocumentCollection()
            {
                Id = id,

            };

            IDictionary<string, (FileType, byte[])> files = await _documentCollectionsBl.DownloadAttachmentsForSigner(documentCollection, signerId);


            Response.Headers.Add("x-file-name", WebUtility.UrlEncode(documentCollection.Name));
            var zipStream = CreateZipArchiveWithFileType(files);
            return File(zipStream, MediaTypeNames.Application.Octet, $"{documentCollection.Name}.zip");
        }

        //TODO change route to "info/{id}"

        /// <summary>
        /// Get info for document collection 
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>        
        /// </remarks>/// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(DocumentCollectionResposneDTO))]
        [Route("info/{id}")]
        public async Task<IActionResult> GetDocumentCollectionData(Guid id)
        {
            var documentCollection = new DocumentCollection()
            {
                Id = id
            };
            var collectionData = await _documentCollectionsBl.Read(documentCollection);
            return Ok(new DocumentCollectionResposneDTO(collectionData));

        }

        /// <summary>
        /// Get sender signing link for live mode document collection 
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>        
        /// </remarks>/// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(SenderLiveSigningLinkDTO))]
        [Route("{id}/senderLink/{signerId}")]
        public async Task<IActionResult> GetDocumentCollectionLiveSenderLink(Guid id, Guid signerId)
        {
            var documentCollection = new DocumentCollection()
            {
                Id = id
            };
            var link =await _documentCollectionsBl.GetSenderLiveLink(documentCollection, signerId);
            return Ok(new SenderLiveSigningLinkDTO(link));

        }

        /// <summary>
        ///  Download document collection trace as separate file
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// offset of client in hours from UTC time <br/>
        /// for example in order to get Israel time you should pass '-3'
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="offset"></param>        
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/audit/{offset}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(FileContentResult))]
        public async Task<IActionResult> DownloadTraceDocument(Guid id, int offset = -3)
        {
            var documentCollection = new DocumentCollection()
            {
                Id = id
            };

            var traceFile =await _documentCollectionsBl.DownloadTrace(documentCollection, Math.Abs(offset));
            Response.Headers.Add("x-file-name", WebUtility.UrlEncode(traceFile.name.Replace(".", "_")));

            return File(new MemoryStream(traceFile.content), MediaTypeNames.Application.Pdf, $"{traceFile.name.Replace(".", "_")}.pdf");
        }


        /// <summary>
        /// Create document collection from template/s
        /// </summary> 
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// Mode: OrderedGroupSign = 1, GroupSign = 2, Online = 3 <br/>
        /// SendingMethod: SMS = 1, Email = 2, Tablet = 3 <br/>        
        /// OtpMode: None = 0, CodeRequired = 1, PasswordRequired = 2, CodeAndPasswordRequired = 3 <br/>
        /// AuthenticationMode: None = 0, IDP = 1 ,VisualIDP = 2,  <br/>
        /// If RediretUrl contains [docId] placeholder, this placeholder will replace by original document collection id <br/>
        /// For each signer you must attach contactId with sendingMethod or contactMeans with contactName<br/><br/>
        /// 
        /// 
        /// Response<br/>
        /// In case of documentCollection mode is OrderedGroupSign we will return link for first signer only
        /// </remarks>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(CreateDocumentCollectionResposneDTO))]
        public async Task<IActionResult> CreateDocument(CreateDocumentCollectionDTO input)
        {
            var documents = GetDocuments(input);
            var signers = await GetSigners(input);
            GetSharedAppendicesSigners(input, signers);
            var readOnlyFields = GetSignerFields(input.ReadOnlyFields, Guid.Empty);
            var senderIp = HttpContext.GetRemoteIPAddress();
            var documentCollection = new DocumentCollection()
            {
                Name = input.DocumentName,
                Mode = input.DocumentMode,
                Documents = documents,
                CreationTime = _dater.UtcNow(),
                RedirectUrl = input.RediretUrl,
                Signers = signers,
                SenderAppendices = input.SenderAppendices,
                ShouldEnableMeaningOfSignature = input.ShouldEnableMeaningOfSignature,
                Notifications = new DocumentNotifications()
                {
                    ShouldSendDocumentForSigning = input.Notifications?.ShouldSend,
                    ShouldSendSignedDocument = input.Notifications?.ShouldSendSignedDocument
                },
                ShouldSignUsingSigner1AfterDocumentSigningFlow = input.ShouldSignUsingSigner1AfterDocumentSigningFlow,
                SenderIP = senderIp.ToString(),
            };

            var links = Enumerable.Empty<SignerLink>();
            try
            {
                await _documentCollectionsBl.Create(documentCollection, readOnlyFields);
                await _documentCollectionsBl.Update(documentCollection, readOnlyFields);
                links = await   _documentCollectionsBl.SendSignerLinks(documentCollection);
            }
            catch (Exception ex)
            {
                try
                {
                    //If Document create in DB but not in FS
                    if (await _documentCollectionsBl.Read(documentCollection) != null)
                    {
                        await _documentCollectionsBl.Delete(documentCollection);
                    }
                    throw ex;
                }
                catch (Exception exception)
                {
                    throw ex;
                }
            }

            var response = new CreateDocumentCollectionResposneDTO()
            {
                DocumentCollectionId = documentCollection.Id,
                SignerLinks = documentCollection.Mode == DocumentMode.OrderedGroupSign ? new List<SignerLink> { links.FirstOrDefault() } : links
            };
            return Ok(response);
        }

        /// <summary>
        /// Document for "Lite" production version
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// If RediretUrl contains [docId] placeholder, this placeholder will replace by original document collection id <br/>
        /// </remarks>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(CreateDocumentCollectionResposneDTO))]
        [Route("simple")]
        public async Task<IActionResult> CreateSimpleDocument(CreateSimpleDocumentDTO input)
        {
            
            bool isOTPActive = false;
            Template template = await _templatesBl.GetTemplateByTemplateId(new Template() { Id = input.TemplateId });

            //why SendingMethod.Email is hard coded
        
            Contact selectedContact = await _contactsBl.GetContactForSimpleDocument(input.SignerMeans, input.SignerName, SendingMethod.Email);
            if (!string.IsNullOrWhiteSpace(input.SignerOTPMeans))
            {
                var emailValidator = new EmailAddressAttribute();
                isOTPActive = true;
                if (ContactsExtenstions.IsValidPhone(input.SignerOTPMeans) && selectedContact.DefaultSendingMethod == SendingMethod.Email)
                {
                    selectedContact.Phone = input.SignerOTPMeans;
                   
                   await _contactsBl.Update(selectedContact);
                }
                else if (emailValidator.IsValid(input.SignerOTPMeans) && selectedContact.DefaultSendingMethod == SendingMethod.SMS)
                {
                    selectedContact.Email = input.SignerOTPMeans;
                   
                    await _contactsBl.Update(selectedContact);

                }
            }

            CreateDocumentCollectionDTO documentCollection = new CreateDocumentCollectionDTO();
            documentCollection.DocumentMode = DocumentMode.GroupSign;
            documentCollection.DocumentName = input.DocumentName;
            documentCollection.Templates = new Guid[1] { template.Id };
            documentCollection.Signers = GetSignerForSimpleDoc(template, selectedContact, isOTPActive);
            documentCollection.RediretUrl = input.RediretUrl;
            documentCollection.CallBackUrl = input.CallBackUrl;
            documentCollection.Notifications = new DocumentNotificationsDTO() { ShouldSend = input.ShouldSend, ShouldSendSignedDocument = input.ShouldSendSignedDocument };

            return await CreateDocument(documentCollection);

        }

        /// <summary>        
        ///  Get pages count by document id
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="documentId"></param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(DocumentCountResponseDTO))]
        [Route("{id}/documents/{documentId}/pages")]
        public async Task<IActionResult> GetPagesCountByDocumentId(Guid id, Guid documentId)
        {
            var documentCollection = new DocumentCollection()
            {
                Id = id,
                Documents = new List<Document>()
                {
                    new Document()
                    {
                        Id = documentId
                    }
                }
            };
            (var document, string documentCollectionName) = await _documentCollectionsBl.GetPagesCountByDocumentId(documentCollection);
            var response = new DocumentCountResponseDTO()
            {
                DocumentId = document.Id,
                DocumentName = documentCollectionName,
                PagesCount = document.PagesCount,
                TemplateId = document.TemplateId
            };
            return Ok(response);
        }

        /// <summary>
        /// Get information about specific page in document
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// Page number start from 1 <br/> 
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="documentId"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/documents/{documentId}/pages/{page}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(DocumentPageResponseDTO))]
        public async Task<IActionResult> GetDocumentPageInfo(Guid id, Guid documentId, int page)
        {
            if (page < 1)
            {
                throw new InvalidOperationException(ResultCode.InvalidPageNumber.GetNumericString());
            }

            var documentCollection = new DocumentCollection()
            {
                Id = id,
                Documents = new List<Document>()
                {
                    new Document()
                    {
                        Id = documentId
                    }
                }
            };

            var document = await _documentCollectionsBl.GetPageInfoByDocumentId(documentCollection, page);
            var response = new DocumentPageResponseDTO()
            {
                DocumentId = document.Id,
                Name = document.Name,
                PdfFields = document.Fields,
                PageImage = document.Images.FirstOrDefault()?.Base64Image,
                PageHeight = document.Images.FirstOrDefault().Height,
                PageWidth = document.Images.FirstOrDefault().Width,
                PagesCount = document.PagesCount
            };
            return Ok(response);
        }

        /// <summary>
        /// Get information about pages range in document
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// offset number start from 1 <br/> 
        /// limit number start from 0 <br/> 
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="documentId"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/documents/{documentId}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(DocumentPagesRangeResponseDTO))]
        public async Task<IActionResult> GetDocumentPagesInfo(Guid id, Guid documentId, int offset = 1, int limit = 5, bool inViewMode = true)
        {
            if (offset < 1)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (limit < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }
            var documentCollection = new DocumentCollection()
            {
                Id = id,
                Documents = new List<Document>()
                {
                    new Document()
                    {
                        Id = documentId
                    }
                }
            };

            var document = await _documentCollectionsBl.GetPagesInfoByDocumentId(documentCollection, offset, limit);
            

            var response = new DocumentPagesRangeOcrResponseDTO();
            int pageNumber = offset;
            foreach (var image in document.Images)
            {
                var htmlOutput = await _documentCollectionsBl.GetOcrHtmlFromImage(image.Base64Image);

                response.DocumentPages.Add(new DocumentPageOcrResponseDTO
                {
                    DocumentId = document.Id,
                    Name = document.Name,
                    PdfFields = document.Fields.GetPDFFieldsByPage(pageNumber, inViewMode),
                    PageImage = image.Base64Image,
                    OcrString = htmlOutput,
                    PageHeight = image.Height,
                    PageWidth = image.Width,
                    PagesCount = document.PagesCount,
                    PageNumber = pageNumber
                });
                pageNumber++;
            }
           

            return Ok(response);
        }

//        public static string OcrJsonToPositionedHtml(string ocrJson)
//        {
//            var jObj = JObject.Parse(ocrJson);
//            var response = jObj["responses"]?.FirstOrDefault() ?? jObj;
//            var page = response["fullTextAnnotation"]?["pages"]?.FirstOrDefault();
//            if (page == null) return "";

//            int width = page["width"]?.Value<int>() ?? 1000;
//            int height = page["height"]?.Value<int>() ?? 1400;

//            var sb = new System.Text.StringBuilder();

//            // Add the ocr-overlay style as a <style> block (if not already present in your app)
//            sb.AppendLine(@"<style>
//.ocr-overlay {
//  position: absolute !important;
//  pointer-events: none !important
//  opacity: 1 !important;
//  color: rgb(255, 0, 0) !important;
//  z-index: 99 !important;
//}
//</style>");

//            // Use the ocr-overlay class on the holding div
//            sb.AppendLine($"<div class='ocr-overlay' style='width:{width}px !important;height:{height}px !important;'>");

//            foreach (var block in page["blocks"] ?? new JArray())
//            {
//                foreach (var paragraph in block["paragraphs"] ?? new JArray())
//                {
//                    foreach (var word in paragraph["words"] ?? new JArray())
//                    {
//                        var wordText = string.Concat(word["symbols"]?.Select(s => s["text"]?.ToString()) ?? Enumerable.Empty<string>());
//                        var vertices = word["boundingBox"]?["vertices"];
//                        if (vertices != null && vertices.Count() >= 2)
//                        {
//                            int x = vertices[0]["x"]?.Value<int>() ?? 0;
//                            int y = vertices[0]["y"]?.Value<int>() ?? 0;
//                            int x2 = vertices[2]["x"]?.Value<int>() ?? x + 50;
//                            int y2 = vertices[2]["y"]?.Value<int>() ?? y + 20;
//                            int w = Math.Abs(x2 - x);
//                            int h = Math.Abs(y2 - y);

//                            sb.AppendLine(
//                                $"<span style='position:absolute !important;left:{x}px !important;top:{y}px !important;width:{w}px !important;height:{h}px !important;" +
//                                $"font-size:{h * 0.8}px !important;white-space:nowrap !important;'>{System.Net.WebUtility.HtmlEncode(wordText)}</span>");
//                        }
//                    }
//                }
//            }

//            sb.AppendLine("</div>");
//            return sb.ToString();
//        }

//        private static byte[] ConvertToTiff(byte[] imageBytes)
//        {
//            using (var inputStream = new MemoryStream(imageBytes))
//            using (var image = System.Drawing.Image.FromStream(inputStream))
//            using (var outputStream = new MemoryStream())
//            {
//                image.Save(outputStream, System.Drawing.Imaging.ImageFormat.Tiff);
//                return outputStream.ToArray();
//            }
//        }

        /// <summary>
        /// Delete document collection by id
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteDocumentCollection(Guid id)
        {
            var documentCollection = new DocumentCollection()
            {
                Id = id
            };
            await _documentCollectionsBl.Delete(documentCollection);
            return Ok();
        }

        [HttpPut]
        [Route("deletebatch")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteBatch(DeleteBatchRequestDTO documentBatchRequestDTO)
        {

            var documentBatch = new RecordsBatch();
            foreach (var id in documentBatchRequestDTO.Ids)
            {
                var guidId = Guid.Parse(id);
                if (!documentBatch.Ids.Contains(guidId))
                    documentBatch.Ids.Add(guidId);
            }
            await _documentCollectionsBl.DeleteBatch(documentBatch);
            return Ok();
        }

        /// <summary>
        /// Cancel document collection by id
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{id}/cancel")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> CancelDocumentCollection(Guid id)
        {
            var documentCollection = new DocumentCollection()
            {
                Id = id
            };
            await _documentCollectionsBl.Cancel(documentCollection);
            return Ok();
        }

        /// <summary>
        /// Resend document collection link to signer
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// SendingMethod: SMS = 1, Email = 2, Tablet = 3 <br/>
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="signerId"></param>
        /// <param name="sendingMethod"></param>
        /// <param name="shouldSend"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/signers/{signerId}/method/{sendingMethod}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(SignerLink))]
        public async Task<IActionResult> ResendDocument(Guid id, Guid signerId, SendingMethod sendingMethod, bool shouldSend = true)
        {
            var documentCollection = new DocumentCollection()
            {
                Id = id,
                Signers = new List<Signer>() { new Signer() { Id = signerId, SendingMethod = sendingMethod } },
                Notifications = new DocumentNotifications() { ShouldSendDocumentForSigning = shouldSend }
            };
            var link = await _documentCollectionsBl.ResendDocument(documentCollection);
            return Ok(link);
        }

        /// <summary>
        /// Reactivate document collection to signer
        /// </summary>
        /// <remarks>An authorized API call. The token should be passed via the request header.</remarks>
        /// <param name="collectionId"></param>
        /// <param name="signerId"></param>
        /// <param name="shouldSend"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{collectionId}/reactivate")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(List<SignerLink>))]
        public async Task<IActionResult> ReactivateDocument(Guid collectionId, bool shouldSend = true)
        {
            var docNotifications = new DocumentNotifications()
            {
                ShouldSendDocumentForSigning = shouldSend
            };

            var documentCollection = new DocumentCollection()
            {
                Id = collectionId,
                Notifications = docNotifications
            };

            var link = await _documentCollectionsBl.ReactivateDocument(documentCollection);
            return Ok(link);
        }

        /// <summary>
        /// Get signing links for document
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="id"></param>
        [HttpGet]
        [Route("{id}/DocumentCollectionLinks")]
        [FeatureGate(FeatureFlags.EnableGetDocumentSigningLinks)]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(SignerLink))]
        public async Task<IActionResult> GetDocumentSigningLinks(Guid id)
        {
            var documentCollection = new DocumentCollection()
            {
                Id = id,
            };
            var links =await _documentCollectionsBl.GetDocumentCollectionSigningLinks(documentCollection);
            return Ok(links);
        }

        /// <summary>
        /// Share document collection to contact
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="contactId"></param>
        /// <param name="shouldSendSignedDocument"></param>
        /// <param name="sendingMethod"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("share")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> ShareDocument(ShareDTO input)
        {
            Contact contact =await _contactsBl.GetOrCreateContact(input.SignerName, input.SignerMeans);
            SendingMethod sendingMethod = ContactsExtenstions.IsValidPhone(input.SignerMeans) ? SendingMethod.SMS : SendingMethod.Email;

            var documentCollection = new DocumentCollection()
            {
                Id = input.DocumentCollectionId,
                Signers = new List<Signer>() { new Signer() { SendingMethod = sendingMethod, Contact = contact } },
                Notifications = new DocumentNotifications() { ShouldSendSignedDocument = true }
            };

            await _documentCollectionsBl.ShareDocument(documentCollection);
            return Ok();
        }

        /// <summary>
        /// Export documents collections details to CSV file
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <returns></returns>
        [HttpGet]
        [Route("export")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(FileContentResult))]
        public async Task<IActionResult> Export(bool sent = true, bool viewed = true, bool signed = true, bool declined = true, bool sendingFailed = true, bool canceled = true, Language language = Language.en)
        {
            if (!sent && !viewed && !signed && !declined && !sendingFailed && !canceled)
            {
                throw new InvalidOperationException(ResultCode.ExportDocumentsForThisTypeNotSupported.GetNumericString());
            }
            byte[] file =await _documentCollectionsBl.ExportDocumentsCollection(sent, viewed, signed, declined, sendingFailed, canceled, language);
            string fileName = "Documents";
            Response.Headers.Add("x-file-name", fileName);
            return File(new MemoryStream(file), MediaTypeNames.Application.Octet, $"{fileName}.csv");
        }

        [HttpGet]
        [Route("exportDistribution")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(FileContentResult))]
        public async Task<IActionResult> ExportDistribution(Language language = Language.en)
        {

            byte[] file =await _documentCollectionsBl.ExportDistributionDocumentsCollection(language);
            string fileName = "DistributionDocuments";
            Response.Headers.Add("x-file-name", fileName);
            return File(new MemoryStream(file), MediaTypeNames.Application.Octet, $"{fileName}.csv");
        }


        /// <summary>
        /// Export document collection fields to XML file
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/fields")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(FileContentResult))]
        public async Task<IActionResult> ExportPdfFields(Guid id)
        {
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Id = id
            };

            (byte[] file, _) = await _documentCollectionsBl.ExportFieldsFromPdf(documentCollection, true);
            string fileName = "fields";
            Response.Headers.Add("x-file-name", fileName);
            return File(new MemoryStream(file), MediaTypeNames.Application.Xml, $"{fileName}.xml");
        }

        [HttpGet]
        [Route("{id}/fields/json")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(PDFFields))]
        public async Task<IActionResult> ExportPdfFieldsData(Guid id, bool includeSigantures = false)
        {
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Id = id
            };
            var documentData = await _documentCollectionsBl.ExportFieldsFromPdfData(documentCollection);
            if (!includeSigantures)
            {
                documentData?.SignatureFields?.Clear();
            }
            return Ok(documentData);
        }
        /// <summary>
        /// Export document collection fields to XML file and to csv file in a zip file
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/fields/CsvXml")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(FileContentResult))]
        public  async Task<IActionResult> ExportPdfFieldsCSVAndXML(Guid id)
        {
            DocumentCollection documentCollection = new DocumentCollection()
            {
                Id = id
            };
            (byte[] xml, byte[] csv) = await _documentCollectionsBl.ExportFieldsFromPdf(documentCollection, false);
            Dictionary<string, (FileType, byte[])> result = new Dictionary<string, (FileType, byte[])>();
            result.Add($"{id.ToString("N")}_1", (FileType.XML, xml));
            result.Add($"{id.ToString("N")}_2", (FileType.CSV, csv));

            Response.Headers.Add("x-file-name", WebUtility.UrlEncode(documentCollection.Id.ToString("N")));
            var zipStream = CreateZipArchiveWithFileType(result);



            return File(zipStream, MediaTypeNames.Application.Zip, $"{documentCollection.Id.ToString("N")}.zip");

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
        public async Task<IActionResult> ReplaceSigner(Guid id, Guid signerId, ReplaceSignerDTO input)
        {
            
            Contact contact = await _contactsBl.GetOrCreateContact(input.NewSignerName, input.NewSignerMeans);
            SendingMethod sendingMethod = ContactsExtenstions.IsValidPhone(input.NewSignerMeans) ? SendingMethod.SMS : SendingMethod.Email;

            var documentCollection = new DocumentCollection()
            {
                Id = id,
                Signers = new List<Signer>() { new Signer() { SendingMethod = sendingMethod, Contact = contact } }
            };
            await _documentCollectionsBl.ReplaceSigner(documentCollection, signerId);
            return Ok();
        }

        /// <summary>
        /// Extra server document collection signing by id
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{id}/serversign")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> ExtraServerSigning(Guid id)
        {
            var documentCollection = new DocumentCollection()
            {
                Id = id
            };
            await _documentCollectionsBl.ExtraServerSigning(documentCollection);
            return Ok();
        }

        #region Private

        private List<Document> GetDocuments(CreateDocumentCollectionDTO input)
        {
            var documents = new List<Document>();
            if (input.Templates != null)
            {
                foreach (var templateId in input.Templates)
                {
                    var document = new Document()
                    {
                        TemplateId = templateId
                    };
                    documents.Add(document);
                }
            }

            return documents;
        }

        private IEnumerable<SignerDTO> GetSignerForSimpleDoc(Template template, Contact selectedContact, bool isOTPActive)
        {
            var signers = new List<SignerDTO>();
            var fields = GetAllFieldsForSimpleDocument(template);
            
            var signer = new SignerDTO()
            {
                ContactId = selectedContact.Id,
                SendingMethod = selectedContact.DefaultSendingMethod,
                SignerFields = fields,
                OtpMode = isOTPActive ? OtpMode.CodeRequired : OtpMode.None,

            };
            signers.Add(signer);
            return signers;
        }

        private List<SignerFieldDTO> GetAllFieldsForSimpleDocument(Template template)
        {
            var fields = new List<SignerFieldDTO>();
            List<BaseField> fieldsInTemplate = new List<BaseField>(template.Fields.SignatureFields);
            fieldsInTemplate.AddRange(template.Fields.TextFields);
            fieldsInTemplate.AddRange(template.Fields.CheckBoxFields);
            fieldsInTemplate.AddRange(template.Fields.ChoiceFields);

            //fieldsInTemplate.AddRange(template.Fields.RadioGroupFields);
            foreach (var signerField in fieldsInTemplate)
            {
                var field = new SignerFieldDTO()
                {
                    TemplateId = template.Id,

                    FieldName = signerField.Name,
                };
                fields.Add(field);
            }
            foreach (var signerField in template.Fields.RadioGroupFields)
            {
                foreach (var item in signerField.RadioFields)
                {
                    var field = new SignerFieldDTO()
                    {
                        TemplateId = template.Id,
                        FieldName = item.Name,
                    };
                    fields.Add(field);
                }
            }
            return fields;

        }

        private async Task<List<Signer>> GetSigners(CreateDocumentCollectionDTO input)
        {
            var signers = new List<Signer>();
            if (input.Signers != null)
            {
                foreach (var signerDTO in input.Signers)
                {
                    List<SignerAttachment> attachments = GetSignerAttchments(signerDTO);
                    List<SignerField> fields = GetSignerFields(signerDTO.SignerFields, signerDTO.ContactId);
                    Notes notes = null;
                    if (!string.IsNullOrWhiteSpace(input.SenderNote) || !string.IsNullOrWhiteSpace(signerDTO.SenderNote))
                    {
                        notes = new Notes
                        {
                            UserNote = !string.IsNullOrWhiteSpace(input.SenderNote) ? $"{input.SenderNote}{Environment.NewLine}{signerDTO.SenderNote}" : signerDTO.SenderNote
                        };
                    }
                    SignerAuthentication signerAuthentication = new SignerAuthentication
                    {
                        OtpDetails = new OtpDetails
                        {
                            Identification = signerDTO.OtpIdentification,
                            Mode = signerDTO.OtpMode
                        },
                        AuthenticationMode = signerDTO.AuthenticationMode
                    };

                    Contact contact = new Contact
                    {
                        Id = signerDTO.ContactId,
                        Email = !string.IsNullOrWhiteSpace(signerDTO.ContactMeans) && signerDTO.ContactMeans.Contains("@") ? signerDTO.ContactMeans : string.Empty,
                        Phone = !string.IsNullOrWhiteSpace(signerDTO.ContactMeans) && signerDTO.ContactMeans.Contains("@") ? string.Empty : signerDTO.ContactMeans,
                        PhoneExtension = string.IsNullOrWhiteSpace(signerDTO.PhoneExtension) ? "+972" : signerDTO.PhoneExtension,
                        Name = signerDTO.ContactName,
                        DefaultSendingMethod = !string.IsNullOrWhiteSpace(signerDTO.ContactMeans) && signerDTO.ContactMeans.Contains("@") ? SendingMethod.Email : SendingMethod.SMS
                    };
                    var sendingMethod = signerDTO.SendingMethod == default ? contact.DefaultSendingMethod : signerDTO.SendingMethod;
                    if (signerDTO.ContactId == default)
                    {
                        
                        contact =  await _contactsBl.GetContactForSimpleDocument(signerDTO.ContactMeans, signerDTO.ContactName, sendingMethod, contact.PhoneExtension);
                    }
                    var signer = new Signer()
                    {
                        Contact = contact,
                        SendingMethod = sendingMethod,
                        Status = SignerStatus.Sent,
                        SignerAttachments = attachments,
                        SignerFields = fields,
                        Notes = notes,
                        SenderAppendices = signerDTO.SenderAppendices,
                        SignerAuthentication = signerAuthentication
                    };
                    signers.Add(signer);
                }
            }

            return signers;
        }

        private void GetSharedAppendicesSigners(CreateDocumentCollectionDTO input, List<Signer> signers)
        {

            foreach (SharedAppendixDTO sharedAppendixDTO in input.SharedAppendices ?? Enumerable.Empty<SharedAppendixDTO>())
            {

                foreach (int signerIndex in sharedAppendixDTO.SignerIndexes)
                {
                    var updatedSenderAppendices = signers[signerIndex].SenderAppendices.ToList();
                    updatedSenderAppendices.Add(sharedAppendixDTO.Appendix);
                    signers[signerIndex].SenderAppendices = updatedSenderAppendices;
                 
                }
              
            }
        }

        private List<SignerAttachment> GetSignerAttchments(SignerDTO signerDTO)
        {
            var attachments = new List<SignerAttachment>();
            if (signerDTO.SignerAttachments != null)
            {
                foreach (var signerAttachment in signerDTO.SignerAttachments)
                {
                    var attachment = new SignerAttachment()
                    {
                        Name = signerAttachment.Name,
                        IsMandatory = signerAttachment.IsMandatory
                    };
                    attachments.Add(attachment);
                }
            }

            return attachments;
        }

        private List<SignerField> GetSignerFields(IEnumerable<SignerFieldDTO> signerFields, Guid contactId)
        {
            var fields = new List<SignerField>();
            if (signerFields != null)
            {
                foreach (var signerField in signerFields)
                {
                    var field = new SignerField()
                    {
                        TemplateId = signerField.TemplateId,
                        ContactId = contactId,
                        FieldName = signerField.FieldName,
                        FieldValue = signerField.FieldValue
                    };
                    fields.Add(field);
                }
            }

            return fields;
        }

        private MemoryStream CreateZipArchive(IDictionary<Guid, (string name, string templateId, byte[] content)> files)
        {
            var zipStream = new MemoryStream();
            using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var zipItem in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(zipItem.Value.name);
                    string fileExtension = Path.GetExtension(zipItem.Value.name);

                    if (fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        fileExtension = ".zip";
                    }

                    if (string.IsNullOrEmpty(fileExtension))
                    {
                        fileExtension = ".pdf";
                    }
                    var entry = zip.CreateEntry($"{fileName.Replace(".", "_")}{fileExtension}");
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

        // need to do it better, need to merge this code with the function above.
        private MemoryStream CreateZipArchiveWithFileType(IDictionary<string, (FileType, byte[])> files)
        {
            var zipStream = new MemoryStream();
            using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var zipItem in files)
                {
                    var entry = zip.CreateEntry($"{zipItem.Key.Replace(".", "_")}.{zipItem.Value.Item1.ToString().ToLower()}");
                    using (var entryStream = entry.Open())
                    {
                        var content = new MemoryStream(zipItem.Value.Item2);
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

