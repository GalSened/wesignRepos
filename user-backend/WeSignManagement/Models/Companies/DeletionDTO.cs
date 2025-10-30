using Common.Models.Documents;

namespace WeSignManagement.Models.Companies
{
    public class DeletionDTO
    {
        public int DeleteSignedDocumentAfterXDays { get; set; }
        public int DeleteUnsignedDocumentAfterXDays { get; set; }

        public DeletionDTO(DocumentDeletionConfiguration documentDeletionConfiguration)
        {
            DeleteSignedDocumentAfterXDays = documentDeletionConfiguration?.DeleteSignedDocumentAfterXDays ?? 0;
            DeleteUnsignedDocumentAfterXDays = documentDeletionConfiguration?.DeleteUnsignedDocumentAfterXDays?? 0;
        }

    }
}
