namespace WeSign.Models.Documents.Responses
{
    using System;

    public class DocumentCountResponseDTO
    {
        public Guid DocumentId { get; set; }
        public Guid TemplateId { get; set; }
        public string DocumentName { get; set; }
        public int PagesCount { get; set; }
    }
}
