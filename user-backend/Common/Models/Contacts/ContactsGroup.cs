using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class ContactsGroup
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CompanyId { get; set; }
        public Guid GroupId { get; set; }
        public  List<ContactGroupMember> ContactGroupMembers { get; set; } = new List<ContactGroupMember>();

    }
}

