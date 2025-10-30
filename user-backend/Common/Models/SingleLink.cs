using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class SingleLink
    {
        public Guid TemplateId { get; set; }
        public string Contact { get; set; }
        public string PhoneExtension { get; set; }
        public string Fullname { get; set; }
    }
}
