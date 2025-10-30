namespace PdfExternalService.Models
{
    public class PDFExternalGeneralSettings
    {
        public int CompressFilesOverSizeBytes { get; set; }

        public bool ShouldDeleteFormFieldsBeforeCreateImage { get; set; }
        public int DPI { get; set; }

        public string DefaultColor { get; set; }

        public int SplitWordsExtractOptions { get; set; }
        public int MaxRequestBodySize { get; set; }
        
        public int MaxFileSize { get; set; }
        public int MaxMergeFiles { get;  set; }
        public string UserAppKey { get; set; }
   
    }
}
