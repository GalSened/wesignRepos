using Common.Enums.Documents;
using System;
using System.Collections.Generic;

namespace Common.Models.Documents
{
    public class DeletedDocumentCollection
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid GroupId { get; set; }
        public Guid DistributionId { get; set; }
        public DocumentStatus DocumentStatus { get; set; }
        public DateTime CreationTime { get; set; }
        public ICollection<DeletedDocument> Documents { get; set; }
        public virtual DeletedDocumentUser User { get; set; }
    }



    public class DeletedDocumentUser
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
    }
}
