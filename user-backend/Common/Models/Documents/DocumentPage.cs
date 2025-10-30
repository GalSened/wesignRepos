using Common.Models.Files.PDF;

namespace Common.Models.Documents
{
    public class DocumentPage
    {
        public int PageNumber { get; set; }
        public string Image { get; set; }
        public PDFFields Fields { get; set; }
    }
}
