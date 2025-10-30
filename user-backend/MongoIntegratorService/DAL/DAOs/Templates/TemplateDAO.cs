using HistoryIntegratorService.Common.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HistoryIntegratorService.DAL.DAOs.Templates
{
    [Table("Templates")]
    public class TemplateDAO
    {
        [Key]
        public Guid Id { get; set; }

        public Guid TemplateId { get; set; }
        public virtual ICollection<TemplateSignatureFieldDAO> TemplateSignatureFields { get; set; }
        public TemplateDAO()
        {
        }
        public TemplateDAO(Template template)
        {
            TemplateId = template.TemplateId;
            
            TemplateSignatureFields = template.TemplateSignatureFields.Select(tsf => new TemplateSignatureFieldDAO(tsf)).ToList();
        }
    }
}
