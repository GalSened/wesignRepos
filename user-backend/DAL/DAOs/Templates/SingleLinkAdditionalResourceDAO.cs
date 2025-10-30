using Common.Enums.Templates;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DAOs.Templates
{
    [Table("SingleLinkAdditionalResources")]
    public class SingleLinkAdditionalResourceDAO
    {
        [Key]
        public Guid Id { get; set; }
        public Guid TemplateId { get; set; }
        public SingleLinkAdditionalResourceType Type { get; set; }
        public string Data { get; set; }
        public bool IsMandatory { get; set; }
    }
}
