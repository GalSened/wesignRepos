namespace WeSign.Models.Templates
{
    using Common.Models.Files.PDF;

    public class UpdateTemplateDTO
    {
        public string Name { get; set; }
        public PDFFields Fields { get; set; }
    }
}
