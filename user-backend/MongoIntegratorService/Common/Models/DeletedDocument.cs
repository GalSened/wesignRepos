using MongoDB.Bson.Serialization.Attributes;

namespace HistoryIntegratorService.Common.Models
{
    public class DeletedDocument
    {
        [BsonId]
        public Guid Id { get; set; }
        public Template? Template { get; set; }
    }
}
