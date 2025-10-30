namespace Common.Models.Documents
{
    public class DocumentDeletionConfiguration
    {
        public int DeleteSignedDocumentAfterXDays { get; set; }
        public int DeleteUnsignedDocumentAfterXDays { get; set; }
    }
}
