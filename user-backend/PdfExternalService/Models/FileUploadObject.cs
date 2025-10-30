namespace PdfExternalService.Models
{
    public class FileUploadObject
    {
        public Guid FileId { get; set; }
        public string Base64File { get; set; }


        // Settings
        public int CompressFilesOverSizeBytes { get; set; }

        public bool ShouldDeleteFormFieldsBeforeCreateImage { get; set; }
        public double DPI { get; set; }



    }
}
