namespace WeSign.areas.api.Controllers
{
    using Common.Enums.Results;
    using Common.Enums.Templates;
    using Common.Extensions;
    using Common.Interfaces;
    using Common.Models;
    using Common.Models.Documents;
    using Common.Models.Settings;
    using Humanizer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.Annotations;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Mime;
    using System.Threading.Tasks;
    using WeSign.Models.Documents;
    using WeSign.Models.Templates;
    using WeSign.Models.Templates.Responses;

//#if DEBUG
//    [Route("userapi/v3/templates")]
//#else
//    [Route("v3/templates")]
//#endif
    [ApiController]
    [Area("Api")]
    [Route("v3/[controller]")]
    //[ApiExplorerSettings(GroupName = "api")]
    [Authorize]
    [SwaggerResponse((int)HttpStatusCode.InternalServerError, Type = typeof(GeneralError))]
    [SwaggerResponse((int)HttpStatusCode.BadRequest, Type = typeof(GeneralError))]
    public class TemplatesController : ControllerBase
    {
        private readonly ITemplates _templatesBl;
        private readonly GeneralSettings _generalSettings;
        private const string SINGLE_LINK = "singlelink";

        public TemplatesController(ITemplates templatesBl,IOptions<GeneralSettings> settings)
        {
            _templatesBl = templatesBl;
            _generalSettings = settings.Value;
        }

        /// <summary>
        /// Create new template
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// XML Base64(optional-value) allow you to embed fields from pdf-meta-data<br /> 
        /// Encoding UTF-8 <br/>
        /// Type="Graphic_Signature" properties Fieldname , IsMandatory<br /> 
        /// Type="SmartCard_Signature" properties Fieldname , IsMandatory<br /> 
        /// Type="Server_Signature" properties Fieldname , IsMandatory<br /> 
        /// Type="Server_Signature" properties Fieldname , IsMandatory<br /> 
        /// Type="Checkbox" properties Fieldname , IsMandatory<br /> 
        /// Type="Text" properties Fieldname , IsMandatory<br /> 
        /// Type="Email" properties Fieldname , IsMandatory<br /> 
        /// Type="Phone" properties Fieldname , IsMandatory<br /> 
        /// Type="Time" properties Fieldname , IsMandatory<br /> 
        /// Type="Number" properties Fieldname , IsMandatory<br /> 
        /// Type="Date" properties Fieldname , IsMandatory<br /> 
        /// Type="ChoiceGroup" properties Fieldname , IsMandatory<br /> 
        /// Type="ChoiceGroup" Contains Choice field , Option and IsSelected<br /> 
        /// Type="RadioGroup" Contains RadioField field , Option , IsSelected and Value<br /> 
        /// </remarks>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(TemplateCountResponseDTO))]
        public async Task<IActionResult> Create(CreateTemplateDTO input)
        {
            var template = new Template
            {
                Name = input.Name,
                Base64File = input.Base64File,
                MetaData = input.MetaData ?? string.Empty,
                Status = input.IsOneTimeUseTemplate ? TemplateStatus.OneTimeUse : TemplateStatus.MultipleUse
            };
            await _templatesBl.Create(template);

            return Ok(new TemplateCountResponseDTO()
            {
                TemplateId = template.Id,
                TemplateName = template.Name,
                PagesCount = template.Images.Count,
            });
        }


        /// <summary>
        /// Get all templates by search criteria
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="key"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <param name="popular"></param>
        /// <param name="recent"></param>
        /// <returns></returns>
        [HttpGet]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(AllTemplatesResponseDTO))]
        public async Task<IActionResult> Read(string key = null, string from = null, string to = null, int offset = 0, int limit = 20, bool popular = false, bool recent = false)
        {
            if (offset < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (limit < 0 && limit != -1)
            {
                throw new InvalidOperationException();
            }
            if ((from != null && !DateTime.TryParse(from, out DateTime dateFrom)) ||
                 (to != null && !DateTime.TryParse(to, out DateTime dateTo)))
            {
                throw new InvalidOperationException(ResultCode.InvalidDateTimeFormat.GetNumericString());
            }
            (var templates, int totalCount) = await _templatesBl.Read(key, from, to, offset, limit, popular, recent);

            var response = new List<TemplateResponseDTO>();
            foreach (var template in templates)
            {
                response.Add(new TemplateResponseDTO(template)
                {
                    SingleLinkUrl = GenerateSingleLinkUrl(template.Id)
                });
            }

            Response.Headers.Add("x-total-count", totalCount.ToString());
            return Ok(new AllTemplatesResponseDTO() { Templates = response });
        }

        /// <summary>
        /// Update template fields
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// TextFieldType: Text = 1, Date = 2, Number = 3, Phone = 4, Email = 5, Custom = 6, Time = 7 <br/>
        /// SignaturFieldType: Graphic = 1, SmartCard = 2, Server = 3 <br/>
        /// Date format: dd/MM/yyyy , for example 02/08/2019 <br/>
        /// Time format: HH:MM , for example 15:59 <br/>
        /// X , Y should be value between 0 to 1 (not include) <br/>
        /// Width of textField, choiceField and signatureField should be value between 0.08 to 0.51 (not include) <br/>
        /// Height of textField, choiceField and signatureField should be value between 0.01 to 0.09 (not include) <br/>
        /// Width of radio and checkBoxField should be value between 0.01 to 0.65 (not include) <br/>
        /// Height of radio and checkBoxField should be value between 0.007 to 0.045 (not include) <br/>
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Update(Guid id, UpdateTemplateDTO input)
        {
            var template = new Template
            {
                Id = id,
                Name = input.Name.Trim(),
                Fields = input.Fields
            };
            await _templatesBl.Update(template);
            return Ok();
        }

        /// <summary>
        /// Delete template
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
            var template = new Template
            {
                Id = id
            };
            await _templatesBl.Delete(template);
            return Ok();
        }

        /// <summary>
        /// Get pages count by template id
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/pages")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(TemplateCountResponseDTO))]
        public async Task<IActionResult> GetPagesCountByTemplateId(Guid id)
        {
            var template = new Template
            {
                Id = id
            };
            template =await _templatesBl.GetPagesCountByTemplateId(template);
            Response.Headers.Add("x-total-count", template.Images.Count.ToString());
            return Ok(new TemplateCountResponseDTO()
            {
                TemplateId = template.Id,
                TemplateName = template.Name,
                PagesCount = template.Images.Count
            });
        }

        /// <summary>
        /// Get page details by template id, first page is '1'.
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// page number start from 1 <br/> <br/> 
        /// Response <br/> 
        /// TextFieldType: Text = 1, Date = 2, Number = 3, Phone = 4, Email = 5, Custom = 6, Time = 7 <br/>
        /// SignaturFieldType: Graphic = 1, SmartCard = 2, Server = 3 <br/>
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/pages/{page}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(TemplatePageResponseDTO))]
        public async Task<IActionResult> GetPageInfoByTemplateId(Guid id, int page)
        {
            if (page < 1)
            {
                throw new InvalidOperationException(ResultCode.InvalidPageNumber.GetNumericString());
            }
            var template = new Template
            {
                Id = id
            };
            await _templatesBl.GetPageByTemplateId(template, page);

            var response = new TemplatePageResponseDTO()
            {
                TemplateId = template.Id,
                Name = template.Name,
                PdfFields = template.Fields,
                PageImage = template.Images.FirstOrDefault()?.Base64Image,
                PageHeight = template.Images.FirstOrDefault().Height,
                PageWidth = template.Images.FirstOrDefault().Width
            };
            return Ok(response);
        }


        /// <summary>
        /// Get page details by template id, first page is '1'.
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// offset number start from 1 <br/> 
        /// limit number start from 0 <br/> <br/> 
        /// Response <br/> 
        /// TextFieldType: Text = 1, Date = 2, Number = 3, Phone = 4, Email = 5, Custom = 6, Time = 7 <br/>
        /// SignaturFieldType: Graphic = 1, SmartCard = 2, Server = 3 <br/>
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/pages/range")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(TemplatePagesRangeResponseDTO))]
        public async Task<IActionResult> GetPagesInfoByTemplateId(Guid id, int offset = 1, int limit = 5, bool withImages = true)
        {
            if (offset < 1)
            {
                throw new InvalidOperationException(ResultCode.InvalidPositiveOffsetNumber.GetNumericString());
            }
            if (limit < 0)
            {
                throw new InvalidOperationException(ResultCode.InvalidLimitNumber.GetNumericString());
            }

            var template = new Template
            {
                Id = id
            };
            await _templatesBl.GetPagesByTemplateId(template, offset, limit);

            var response = new TemplatePagesRangeResponseDTO();
            int pageNumber = offset;
            foreach (var image in template.Images)
            {
                var toAdd = new TemplatePageResponseDTO
                {

                    TemplateId = template.Id,
                    Name = template.Name,
                    PdfFields = template.Fields.GetPDFFieldsByPage(pageNumber, false),
                    PageImage = withImages ? image.Base64Image : "",
                    PageHeight = image.Height,
                    PageWidth = image.Width,
                    PageNumber = pageNumber
                };
                response.TemplatePages.Add(toAdd);
                pageNumber++;
            }

            return Ok(response);
        }

        /// <summary>
        /// Duplicate existing template
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(DuplicateTemplateResponseDTO))]
        public async Task<IActionResult> DuplicateTemplate(Guid id, DuplicateTemplateDTO input)
        {
            var template = new Template
            {
                Id = id,
                Status = input == null? TemplateStatus.MultipleUse : input.IsOneTimeUseTemplate ? TemplateStatus.OneTimeUse : TemplateStatus.MultipleUse
            };
            var duplicateTemplate = await _templatesBl.DuplicateTemplate(template);

            return Ok(new DuplicateTemplateResponseDTO() { NewTemplateId = duplicateTemplate.Id , Name = duplicateTemplate.Name});
        }

        /// <summary>
        /// Download template
        /// </summary>
        /// <remarks>
        /// An authorized API call. The token should be passed via the request header.<br/>
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}/download")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(FileContentResult))]
        public async Task<IActionResult> DownloadTemplate(Guid id)
        {
            var template = new Template()
            {
                Id = id
            };
            (string name, byte[] content) file =await _templatesBl.Download(template);
            Response.Headers.Add("x-file-name", WebUtility.UrlEncode(file.name.Replace(".","_")));

            return File(new MemoryStream(file.content), MediaTypeNames.Application.Pdf, $"{file.name.Replace(".", "_")}.pdf");
        }

        [HttpPut]
        [Route("deletebatch")]
        [SwaggerResponse((int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteBatch(BatchRequestDTO batchRequestDTO)
        {

            var contactsBatch = new RecordsBatch();
            foreach (var id in batchRequestDTO.Ids)
            {
                var guidId = Guid.Parse(id);
                if (!contactsBatch.Ids.Contains(guidId))
                    contactsBatch.Ids.Add(guidId);
            }
            await _templatesBl.DeleteBatch(contactsBatch);
            return Ok();
        }


        [HttpPost]
        [Route("merge")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(TemplateCountResponseDTO))]
        public async Task<IActionResult> MergeTempletes(MergeTemplatesDTO input)
        {

            var mergeTemplates = new MergeTemplates(){
                Name = input.Name,
                IsOneTimeUseTemplate = input.IsOneTimeUseTemplate
                
            };
            foreach (var template in input.Templates)
            {
                if (Guid.TryParse(template, out Guid templateID))
                {
                    mergeTemplates.Templates.Add(
                                         new Template
                                         {
                                             Id = templateID
                                         }
                                         );
                }
                else
                {


                    mergeTemplates.Templates.Add(
                        new Template
                        {
                            Base64File = template
                        }
                        );
                }
            }



            Template mergeTemplateResult =await _templatesBl.MergeTemplates(mergeTemplates);

            return Ok(new TemplateCountResponseDTO()
            {
                TemplateId = mergeTemplateResult.Id,
                TemplateName = mergeTemplateResult.Name,
                PagesCount = mergeTemplateResult.Images.Count,
            });


        }

        #region Private Functions

        private string GenerateSingleLinkUrl(Guid id)
        {
            var baseUrl = _generalSettings.SignerFronendApplicationRoute;
            //var request = Request.Path.Value.Substring(0, Request.Path.Value.IndexOf("/templates"));
            return _generalSettings.SignerFronendApplicationRoute + $"/{SINGLE_LINK}/{id}";
        }

        #endregion
    }
}