using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class AdditionalGroupMapper
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid UserId { get; set; }
        public Guid GroupId { get; set; }

        public virtual Company Company { get; set; }
        public virtual User User { get; set; }
        public virtual Group Group { get; set; }
    }
}
