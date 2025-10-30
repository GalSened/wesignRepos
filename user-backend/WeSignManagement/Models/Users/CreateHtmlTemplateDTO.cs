using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSignManagement.Models.Users
{
    public class CreateHtmlTemplateDTO
    {
        public string TemplateId { get; set; }
        public string UserId { get; set; }
        public string HtmlBase64File { get; set; }
        public string JSBase64File { get; set; }
    }
}
