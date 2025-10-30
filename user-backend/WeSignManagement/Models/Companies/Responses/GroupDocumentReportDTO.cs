using Common.Models;

namespace WeSignManagement.Models.Companies.Responses
{
    public class GroupDocumentReportDTO
    {
        public string GroupName { get; set; }
        public int CreatedDocs { get; set; }
        public int SentDocs { get; set; }
        public int ViewedDocs { get; set; }
        public int SignedDocs { get; set; }
        public int DeclinedDocs { get; set; }
        public int DeletedDocs { get; set; }
        public int CanceledDocs { get; set; }
        public int ServerSignedDocs { get; set; }

        public GroupDocumentReportDTO(GroupDocumentReport groupDocumentReport)
        {
            GroupName = groupDocumentReport.GroupName;
            CreatedDocs = groupDocumentReport.CreatedDocs;
            SentDocs = groupDocumentReport.SentDocs;
            ViewedDocs= groupDocumentReport.ViewedDocs;
            SignedDocs = groupDocumentReport.SignedDocs;
            DeclinedDocs = groupDocumentReport.DeclinedDocs;
            DeletedDocs = groupDocumentReport.DeletedDocs;
            CanceledDocs = groupDocumentReport.CanceledDocs;
            ServerSignedDocs = groupDocumentReport.ServerSignedDocs;

        }
    }
}
