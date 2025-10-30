namespace WeSign.Models.Templates.Responses
{
    using Common.Models.Files.PDF;
    using System;

    public class TemplatePageResponseDTO
    {
        public Guid TemplateId { get; set; }
        public string Name { get; set; }
        public PDFFields PdfFields { get; set; }
        public string PageImage { get; set; }
        public string OcrString { get; set; }
        public double PageWidth { get; set; }
        public double PageHeight { get; set; }
        public int PageNumber { get; set; }
    }
}
