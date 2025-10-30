namespace Common.Models.Documents
{
    using Common.Enums.Documents;
    using System;

    public class DocumentCountResult
    {
        public string DocumentName { get; set; }
        public DocumentMode DocumentMode { get; set; }
        public Guid TemplateId { get; set; }
        public int PagesCount { get; set; }
    }
}
