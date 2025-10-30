using MongoDB.Bson.Serialization.Attributes;
using HistoryIntegratorService.Common.Enums;

namespace HistoryIntegratorService.Common.Models
{
    public class TemplateSignatureField
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public SignatureFieldType SignatureFieldType { get; set; }
        public string CompanyName { get; set; }
    }
}
