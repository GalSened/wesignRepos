using HistoryIntegratorService.Common.Enums;
using HistoryIntegratorService.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HistoryIntegratorService.DAL.DAOs.Templates
{
    [Table("TemplateSignatureFields")]
    public class TemplateSignatureFieldDAO
    {
        [Key]
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public SignatureFieldType SignatureFieldType { get; set; }
        public string CompanyName { get; set; }
        public TemplateSignatureFieldDAO()
        {
        }
        public TemplateSignatureFieldDAO(TemplateSignatureField templateSignatureField)
        {
            templateSignatureField.Id = templateSignatureField.Id;
            TemplateId = templateSignatureField.Id;
            SignatureFieldType = templateSignatureField.SignatureFieldType;
            CompanyName = templateSignatureField.CompanyName;
        }
    }
}
