using MongoDB.Bson.Serialization.Attributes;

namespace HistoryIntegratorService.Common.Models
{
    public class Template
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public ICollection<TemplateSignatureField> TemplateSignatureFields { get; set; }
    }
}
