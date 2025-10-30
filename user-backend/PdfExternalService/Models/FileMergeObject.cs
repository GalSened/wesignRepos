namespace PdfExternalService.Models
{
    public class FileMergeObject
    {
        public Guid OperationId { get; set; }
        public List<string> Base64Files { get; set; }
        public string APIKey { get; set; }
    }
}
