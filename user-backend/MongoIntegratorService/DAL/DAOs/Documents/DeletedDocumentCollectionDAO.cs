using HistoryIntegratorService.Common.Enums;
using HistoryIntegratorService.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HistoryIntegratorService.DAL.DAOs.Documents
{
    [Table("DeletedDocumentCollections")]
    public class DeletedDocumentCollectionDAO
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public Guid GroupId { get; set; }
        public Guid DistributionId { get; set; }
        public DocumentStatus DocumentStatus { get; set; }
        public DateTime CreationTime { get; set; }
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string UserEmail { get; set; }
        public virtual ICollection<DeletedDocumentDAO> Documents { get; set; }
        

        public DeletedDocumentCollectionDAO()
        {
        }

        public DeletedDocumentCollectionDAO(DeletedDocumentCollection deletedDocCollection)
        {
            Id = deletedDocCollection.Id;
            UserId = deletedDocCollection.UserId;
            GroupId = deletedDocCollection.GroupId;
            DistributionId = deletedDocCollection.DistributionId;
            DocumentStatus = deletedDocCollection.DocumentStatus;
            CreationTime = deletedDocCollection.CreationTime;
            Documents = deletedDocCollection.Documents.Select(d => new DeletedDocumentDAO(d)).ToList();
            CompanyId = deletedDocCollection.User.CompanyId;
            CompanyName = deletedDocCollection.User.CompanyName;
            UserEmail = deletedDocCollection.User.Email;

        }
    }
}
