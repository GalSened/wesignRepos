namespace PdfExternalService.Models.DTO
{
    public class FileUploadDTO
    {
        public string Base64File { get; set; }

        public int? CompressFilesOverSizeBytes { get; set; } = null;

        public bool? ShouldDeleteFormFieldsBeforeCreateImage { get; set; } = null;
        public double? DPI { get; set; } = null;


        public FileUploadDTO(FileUploadObject fileProperties)
        {
            this.Base64File = fileProperties.Base64File;
            this.CompressFilesOverSizeBytes = fileProperties.CompressFilesOverSizeBytes;
            this.ShouldDeleteFormFieldsBeforeCreateImage = fileProperties.ShouldDeleteFormFieldsBeforeCreateImage;
            this.DPI = fileProperties.DPI;

        }

        public FileUploadDTO()
        {

        }
    }
}
