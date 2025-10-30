namespace WeSign.Models.Templates.Responses
{
    using Common.Models;
    using System;

    public class TemplateResponseDTO
    {
        public Guid TemplateId { get; set; }
        public string Name { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public int PagesCount { get; set; }
        public DateTime TimeCreated { get; set; }

        public string SingleLinkUrl { get; set; }


        public TemplateResponseDTO(Template template)
        {
            if (template != null)
            {
                TemplateId = template.Id;
                Name = template.Name;
                UserId = template.UserId;
                UserName = template.UserName;
                TimeCreated = template.CreationTime;
                PagesCount = template.Images?.Count ?? 0;
            }
        }

    }
}
