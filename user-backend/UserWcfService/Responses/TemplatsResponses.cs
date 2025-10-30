using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UserSoapService.HttpClientLogic;

namespace UserSoapService.Responses
{
    public class GetTemplateAsyncResponse : BaseResult
    {

    }

    public class GetTemplatesResponse : BaseResult
    {
        public AllTemplatesResponseDTO Templates { get; set; }
    }

    public class CreateTemplateResponse : BaseResult
    {
        public TemplateCountResponseDTO TemplateCount { get; set; }
    }

    public class DuplicateTemplateResponse : BaseResult
    {
        public DuplicateTemplateResponseDTO DuplicateTemplate { get; set; }
    }

    public class DownloadTemplateResponse : BaseResult
    {
        public DownloadFileResponse DownloadResponse { get; set; }
    }

    public class GetTemplatePagesCountResponse : BaseResult
    {
        public TemplateCountResponseDTO TemplateCount { get; set; }
    }

    public class GetTemplatesPagesInfoResponse: BaseResult
    {
        public TemplatePagesRangeResponseDTO TemplatePagesRange { get; internal set; }

    }

}