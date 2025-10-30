namespace WeSign.Models.SelfSign
{
    using Common.Models.Files.PDF;
    using System;

    public class SelfSignPageResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public PDFFields PdfFields { get; set; }
        public string PageImage { get; set; }
        public double PageWidth { get; set; }
        public double PageHeight { get; set; }
    }
}
