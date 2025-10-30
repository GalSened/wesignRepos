namespace WeSign.Models.Documents.Responses
{
    using Common.Models.Files.PDF;
    using System;

    public class DocumentPageResponseDTO
    {
        public Guid DocumentId { get; set; }
        public string Name { get; set; }
        public PDFFields PdfFields { get; set; }
        public int PagesCount { get; set; }
        public int PageNumber { get; set; }
        public string PageImage { get; set; }

        public double PageWidth { get; set; }
        public double PageHeight { get; set; }
    }
}
