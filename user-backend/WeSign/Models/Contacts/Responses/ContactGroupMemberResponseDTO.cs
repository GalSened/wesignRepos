using System;

namespace WeSign.Models.Contacts.Responses
{
    public class ContactGroupMemberResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid ContactId { get; set; }
        public int Order { get;  set; }
        public ContactResponseDTO Contact { get; set; }
    }
}
