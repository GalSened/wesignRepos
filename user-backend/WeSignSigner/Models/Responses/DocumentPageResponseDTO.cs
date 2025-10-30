
namespace WeSignSigner.Models.Responses
{
    using Common.Models.Files.PDF;
    using System;

    public class DocumentPageResponseDTO
    {
        public Guid DocumentId { get; set; }
        public string Name { get; set; }
        public PDFFields SignerFields { get; set; }
        public PDFFields OtherFields { get; set; }
        public string PageImage { get; set; }
        public string OcrString { get; set; }
        public int PageNumber { get; set; }
        public double PageWidth { get; set; }
        public double PageHeight { get; set; }
    }
}
