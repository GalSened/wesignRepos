namespace Common.Models.Documents
{
    using Common.Models.Files.PDF;
    using System;
    using System.Collections.Generic;

    public class Document
    {
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public string Name { get; set; }
        public int PagesCount { get; set; }
        public IList<PdfImage> Images { get; set; }
        public PDFFields Fields { get; set; }
    }
}
