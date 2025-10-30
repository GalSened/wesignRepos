using System;
using System.Collections.Generic;

namespace WeSign.Models.Contacts.Responses
{
    public class ContactsGroupResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<ContactGroupMemberResponseDTO> ContactsGroupMembers { get; set; } = new List<ContactGroupMemberResponseDTO>();
    }
}
