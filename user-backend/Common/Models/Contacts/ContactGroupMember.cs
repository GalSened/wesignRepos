using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class ContactGroupMember
    {
        public Guid Id { get; set; }
        public Guid ContactsGroupId { get; set; }
        public Guid ContactId { get; set; }
        public int Order { get; set; }
       
        public  Contact Contact { get; set; }

    }
}
