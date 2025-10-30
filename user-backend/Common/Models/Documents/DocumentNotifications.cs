namespace Common.Models.Documents
{
    public class DocumentNotifications
    {
        public bool? ShouldNotifyWhileSignerSigned { get; set; }
        public bool?  ShouldSendSignedDocument { get; set; }
        public bool? ShouldSendDocumentForSigning { get; set; }
    }
}
