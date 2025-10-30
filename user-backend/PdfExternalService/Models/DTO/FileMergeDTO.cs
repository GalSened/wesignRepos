namespace PdfExternalService.Models.DTO
{
    public class FileMergeDTO
    {
        public string APIKey { get; set; }
        public List<string> Base64Files { get; set; }
       

    
        public FileMergeDTO()
        {
            Base64Files = new List<string>();
        }
    }
}
