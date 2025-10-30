using System.Collections.Generic;

namespace WeSign.Models.Contacts.Responses
{
    public class AllContactsGroupsResponseDTO
    {
        public List<ContactsGroupResponseDTO> ContactGroups { get; set; } = new List<ContactsGroupResponseDTO>();
    }
}
