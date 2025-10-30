using Common.Models;
using System.Collections.Generic;
using System;

namespace WeSign.Models.Templates
{
    public class TemplateSingleLinkDTO
    {
        public Guid TemplateId { get; set; }
        public List<SingleLinkAdditionalResource> SingleLinkAdditionalResources { get; set; }
        public TemplateSingleLinkDTO()
        {

        }
        public TemplateSingleLinkDTO(TemplateSingleLink templateSingleLink)
        {
            SingleLinkAdditionalResources = templateSingleLink.SingleLinkAdditionalResources;
            TemplateId = templateSingleLink.TemplateId;

        }
    }
}
