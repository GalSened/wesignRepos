namespace WeSign.Models.Templates.Responses
{
    using System.Collections.Generic;

    public class AllTemplatesResponseDTO
    {
        public IEnumerable<TemplateResponseDTO> Templates { get; set; }
    }
}
