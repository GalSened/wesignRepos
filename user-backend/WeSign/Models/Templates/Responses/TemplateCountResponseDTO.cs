namespace WeSign.Models.Templates.Responses
{
    using System;

    public class TemplateCountResponseDTO
    {
        public Guid TemplateId { get; set; }
        public string TemplateName { get; set; }
        public int PagesCount { get; set; }
        public DateTime CreationTime { get; set; }
        public string UserName { get; set; }
    }
}
