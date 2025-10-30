using System;

namespace WeSign.Models.Templates.Responses
{
    public class DuplicateTemplateResponseDTO
    {
        public string Name { get; set; }
        public Guid NewTemplateId { get; set; }
    }
}
