using Common.Interfaces;
using Common.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeSign.Models.Documents;
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
using WeSign.Models.Contacts;
using WeSign.Models.Distribution.Requests;
using WeSign.Models.Distribution.Responses;
using PdfHandler.Signing;
using Common.Interfaces.PDF;
using Common.Models.Configurations;
using Comda.Authentication.Models;
using Microsoft.AspNetCore.Http.Features;

namespace WeSign.areas.ui.Controllers
{
//#if DEBUG
//    [Route("userui/v3/distribution")]
//#else
//    [Route("ui/v3/distribution")]
//#endif
    [ApiController]
    [Area("Ui")]
    [Route("Ui/v3/[controller]")]
    //[ApiExplorerSettings(GroupName = "ui")]
    [Authorize]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class DistributionController : ControllerBase
    {
        private readonly IDistribution _distributionBl;

        public DistributionController(IDistribution distributionBl)
        {
            _distributionBl = distributionBl;
        }

        /// <summary>
        /// Extract signers from excel file
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// <br/>
        /// Excel file columns representation : <br/> 
        /// <b>Mandatory columns:</b><br/> 
        /// column 1 - FirstName, column 2 - LastName, column 3 - Phone/Email <br/> 
        /// <b>Optional columns:</b><br/> 
        /// column 4 --to-- column N - FieldName <br/>
        /// <br/>
        /// Expected files type: <br/> 
        /// xls ----- data:application/vnd.ms-excel;base64,.... <br/> 
        /// xlsx ------ data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,....<br/> 
        /// </remarks>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("signers")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(ReadSignersFromFileResponseDTO))]
        public async Task<IActionResult> ReadSignersFromFile(SignersForDistributionMechanismDTO input)
        {
            IEnumerable<BaseSigner> signersFromExcel =await _distributionBl.ExtractSignersFromExcel(input.Base64File);

            return Ok(new ReadSignersFromFileResponseDTO { Signers = signersFromExcel });
        }

        /// <summary>
        /// Send new document collection for each signer and set all fields in document collection to signer
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> DistributionMechanism(CreateDistributionDocumentsDTO input)
        {
            List<DocumentCollection> documentCollections = new List<DocumentCollection>();
            string senderIpAddress = HttpContext.GetRemoteIPAddress();
            foreach (BaseSigner signer in input.Signers ?? Enumerable.Empty<BaseSigner>())
            {

                string email = ContactsExtenstions.IsValidEmail(signer.SignerMeans) ? signer.SignerMeans:
                    ContactsExtenstions.IsValidEmail(signer.SignerSecondaryMeans) ? signer.SignerSecondaryMeans  : ""  ;
                string phone = ContactsExtenstions.IsValidPhone(signer.SignerMeans) ? signer.SignerMeans :
                    ContactsExtenstions.IsValidPhone(signer.SignerSecondaryMeans) ? signer.SignerSecondaryMeans : "";
                DocumentCollection documentCollection = new DocumentCollection
                {
                    Name = input.Name,
                    ShouldSignUsingSigner1AfterDocumentSigningFlow = input.SignDocumentWithServerSigning,
                    ShouldEnableMeaningOfSignature = input.ShouldEnableMeaningOfSignature,
                    Documents = new List<Document> { new Document { TemplateId = input.TemplateId } },
                    Signers = new List<Signer> { new Signer {
                        SendingMethod = ContactsExtenstions.IsValidEmail(signer.SignerMeans) ? SendingMethod.Email : SendingMethod.SMS,
                        SignerFields = GetSignerFields(signer.Fields),
                        Contact = new Contact {
                            Email = email,
                            Phone = phone,
                            PhoneExtension = signer.PhoneExtension,
                            Name = signer.FullName,
                        } } },
                    SenderIP = senderIpAddress
                };

                if (signer.ShouldSendOTP)
                {
                    documentCollection.Signers.First().SignerAuthentication = new SignerAuthentication
                    {
                        OtpDetails = new OtpDetails
                        {
                            Mode = OtpMode.CodeRequired
                        }
                    };
                }

                documentCollections.Add(documentCollection);
            }

            await _distributionBl.SendDocumentsUsingDistributionMechanism(documentCollections);

            return Ok();
        }

        /// <summary>
        /// Get all distribution document collections by search criteria
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/><br/>
        /// Request <br/>
        /// offset number start from 1 <br/> 
        /// limit number start from 1 <br/> <br/>
        /// Response <br/>
        /// </remarks>
        /// <param name="key"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllDistributionDocumentsResposneDTO))]
        public async Task<IActionResult> Read(string key = null, string from = null, string to = null, int offset = 0, int limit = 20)
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
            (IEnumerable<DocumentCollection> distributionDocuments, int totalCount) =await _distributionBl.Read(key, from, to, offset, limit);

            List<DistributionDocumentResposneDTO>  response = new List<DistributionDocumentResposneDTO>();
            foreach (DocumentCollection distributionDocument in distributionDocuments ?? Enumerable.Empty<DocumentCollection>())
            {
                response.Add(new DistributionDocumentResposneDTO(distributionDocument));
            }
            Response.Headers.Add("x-total-count", totalCount.ToString());

            return Ok(new AllDistributionDocumentsResposneDTO() { DocumentCollections = response });
        }

        /// <summary>
        /// Get all documents of distribution id
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/><br/>
        /// Request <br/>
        /// offset number start from 1 <br/> 
        /// limit number start from 1 <br/> <br/>
        /// Response <br/>
        /// DocumentStatus: Created = 1, Sent = 2, Viewed = 3, Signed = 4, Declined = 5, SendingFailed = 6, Deleted = 7, Canceled = 8 <br/>
        /// SignerStatus:  Sent = 1, Viewed = 2, Signed = 3, Rejected = 4 <br/>
        /// SendingMethod: SMS = 1, Email = 2, Tablet = 3 <br/>
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="key"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllDistributionDocumentsExpandedResposneDTO))]
        public async Task<IActionResult> Read(Guid id, string key = null, string from = null, string to = null, int offset = 0, int limit = 20)
        {
            if (offset < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (limit < -1)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }
            if (from != null && to != null && (!DateTime.TryParse(from, out DateTime dateFrom) || !DateTime.TryParse(to, out DateTime dateTo)))
            {
                throw new InvalidOperationException(ResultCode.InvalidDateTimeFormat.GetNumericString());
            }
            Dictionary<DocumentStatus, int> statusCounts = new Dictionary<DocumentStatus, int>();
            (IEnumerable<DocumentCollection> distributionDocuments, int totalCount) =await _distributionBl.Read(id, key, from, to, offset, limit, statusCounts);

            List<DistributionDocumentExpandedResposneDTO> documentsResponse = new List<DistributionDocumentExpandedResposneDTO>();

            foreach (DocumentCollection distributionDocument in distributionDocuments ?? Enumerable.Empty<DocumentCollection>())
            {
                documentsResponse.Add(new DistributionDocumentExpandedResposneDTO(distributionDocument));
            }
            Response.Headers.Add("x-total-count", totalCount.ToString());
            AllDistributionDocumentsExpandedResposneDTO response = new AllDistributionDocumentsExpandedResposneDTO() { DocumentCollections = documentsResponse };

            response.TotalFailed = statusCounts[DocumentStatus.SendingFailed];
            response.TotalDeclined = statusCounts[DocumentStatus.Declined];
            response.TotalSigned = statusCounts[DocumentStatus.Signed];
            response.TotalServerSigned = statusCounts[DocumentStatus.ExtraServerSigned];
            response.TotalPending = statusCounts[DocumentStatus.Sent];
            response.TotalCreatedButNotSent = statusCounts[DocumentStatus.Created];
            response.TotalViewed = statusCounts[DocumentStatus.Viewed];
            response.ShouldSignUsingSigner1AfterDocumentSigningFlow = distributionDocuments?.FirstOrDefault()?.ShouldSignUsingSigner1AfterDocumentSigningFlow ?? false;

            return Ok(response);
        }

        /// <summary>
        /// Delete all documents of distribution id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _distributionBl.DeleteAllDocuments(id);
            return Ok();
        }

        /// <summary>
        /// Resend all unsigned documents of distribution id to there signers
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("resend/{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Resend(Guid id)
        {
           await _distributionBl.ReSendUnSignedDocuments(id);
            return Ok();
        }

        [HttpGet]
        [Route("resend/{id}/status/{status}")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> ResendInStatus(Guid id, DocumentStatus status)
        {
            await _distributionBl.ReSendDocumentsInStatus(id, status);
            return Ok();
        }

        /// <summary>
        /// Download all signed documents of distribution id
        /// As zip file, every document name inside zip file will be the signer name
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
      
        [Route("download/{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK,Type = typeof(FileContentResult))]

        public async Task<IActionResult> Download(Guid id)
        {
            (List<(string SignerName, IDictionary<Guid, (string name, byte[] content)> Files)> documentCollectionsFiles, string documentName) = await _distributionBl.DownloadSignedDocuments(id);

            Response.Headers.Add("x-file-name", WebUtility.UrlEncode($"{documentName}.zip"));
            MemoryStream zipStream = CreateZipArchive(documentCollectionsFiles);
            return File(zipStream, MediaTypeNames.Application.Octet, $"{documentName}.zip");

            
        }

        #region Private Functions

        private IEnumerable<SignerField> GetSignerFields(IEnumerable<FieldNameToValuePair> fields)
        {
            List<SignerField> signerFields = new List<SignerField>();
            fields.ForEach(f =>
            {
                signerFields.Add(new SignerField
                {
                    FieldName = f.FieldName,
                    FieldValue = f.FieldValue
                });
            });

            return signerFields;
        }


        private MemoryStream CreateZipArchive(List<(string SignerName, IDictionary<Guid, (string name, byte[] content)> Files)> documents)
        {
            MemoryStream zipStream = new MemoryStream();
            using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                foreach (var item in documents)
                {
                    var files = item.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        string fileName = files.Count == 1 ? item.SignerName : $"{item.SignerName}_{i + 1}";
                        var entry = zip.CreateEntry($"{fileName}.pdf");
                        using (var entryStream = entry.Open())
                        {
                            var content = new MemoryStream(files.ElementAt(i).Value.content);
                            content.CopyTo(entryStream);
                        }
                    }
                }
            }
            zipStream.Position = 0;
            return zipStream;
        }

  
        #endregion
    }
}
