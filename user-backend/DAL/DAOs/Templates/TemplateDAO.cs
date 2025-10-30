namespace DAL.DAOs.Templates
{
    using Common.Enums.Templates;
    using Common.Models;
    using DAL.DAOs.Documents;
    using DAL.DAOs.Users;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Templates")]
    public class TemplateDAO
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid GroupId { get; set; }
        public string Name { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastUpdatetime { get; set; }

        public DateTime LastUsedTime { get; set; } = DateTime.UtcNow;
        public TemplateStatus Status { get; set; }
        public int UsedCount { get; set; }
        public virtual ICollection<DocumentDAO> Documents { get; set; }
        public virtual ICollection<TemplateTextFieldDAO> TemplateTextFields { get; set; }
        public virtual ICollection<TemplateSignatureFieldDAO> TemplateSignatureFields { get; set; }
     
        public TemplateDAO() {
            LastUsedTime = DateTime.UtcNow;
        }

        public TemplateDAO(Template template) {
            Id = template.Id == Guid.Empty ? default : template.Id;
            UserId = template.UserId == Guid.Empty ? default : template.UserId;
            GroupId = template.GroupId;
            Name = template.Name;
            CreationTime = template.CreationTime;
            LastUpdatetime = template.LastUpdatetime;
            UsedCount = template.UsedCount;
            Status = template.Status;
        }
    }
}
