using Common.Enums.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class TemplateSingleLink
    {
        public Guid TemplateId { get; set; }
        public List<SingleLinkAdditionalResource> SingleLinkAdditionalResources { get; set; }

    }

    public class SingleLinkAdditionalResource
    {
             
        public Guid TemplateId { get; set; }
        public SingleLinkAdditionalResourceType Type { get; set; }
        public string Data { get; set; }
        public bool IsMandatory { get; set; }

    }
}
