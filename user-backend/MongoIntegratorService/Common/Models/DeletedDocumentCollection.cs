using HistoryIntegratorService.Common.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HistoryIntegratorService.Common.Models
{
    public class DeletedDocumentCollection
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid UserId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid GroupId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid DistributionId { get; set; }
        public DocumentStatus DocumentStatus { get; set; }
        public DateTime CreationTime { get; set; }
        public virtual ICollection<DeletedDocument> Documents { get; set; }
        public virtual User User { get; set; }

    }


}
