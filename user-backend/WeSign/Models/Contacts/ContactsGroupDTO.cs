using System;
using System.Collections.Generic;

namespace WeSign.Models.Contacts
{
    public class ContactsGroupDTO
    {
        public string Name { get; set; }
        public List<ContactGroupMemberDTO> ContactsGroupMembers { get; set; }
    }
}
