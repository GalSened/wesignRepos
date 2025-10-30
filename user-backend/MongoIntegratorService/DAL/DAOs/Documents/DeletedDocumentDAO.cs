using HistoryIntegratorService.Common.Models;
using HistoryIntegratorService.DAL.DAOs.Templates;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HistoryIntegratorService.DAL.DAOs.Documents
{
    [Table("Documents")]
    public class DeletedDocumentDAO
    {
        [Key]
        public Guid Id { get; set; }
        public virtual TemplateDAO Template { get; set; }
        public DeletedDocumentDAO()
        {
        }

        public DeletedDocumentDAO(DeletedDocument deletedDoc)
        {
            Id = deletedDoc.Id;
            if (deletedDoc.Template != null)
            {
                Template = new TemplateDAO(deletedDoc.Template);
            }
        }
    }
}
