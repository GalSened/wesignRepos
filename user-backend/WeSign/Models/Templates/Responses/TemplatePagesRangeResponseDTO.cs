using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSign.Models.Templates.Responses
{
    public class TemplatePagesRangeResponseDTO
    {
        public List<TemplatePageResponseDTO> TemplatePages { get; set; }

        public TemplatePagesRangeResponseDTO()
        {
            TemplatePages = new List<TemplatePageResponseDTO>();
        }
    }
}
