namespace WeSign.Models.Contacts.Responses
{
    using Common.Enums.Documents;
    using Common.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ContactResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PhoneExtension { get; }
        public SendingMethod DefaultSendingMethod { get; set; }
        public IEnumerable<Seal> Seals { get; set; }
        public ContactResponseDTO()
        {
            Seals = new List<Seal>();
        }
        public string SearchTag { get; set; }

        public ContactResponseDTO(Contact contact)
        {
            if (contact != null)
            {
                Id = contact.Id;
                DefaultSendingMethod = contact.DefaultSendingMethod;                
                Email = contact.Email;
                Name = contact.Name;
                Phone = contact.Phone;
                PhoneExtension = string.IsNullOrWhiteSpace(contact.PhoneExtension) ? "+972" : contact.PhoneExtension;
                Seals = contact.Seals;
                SearchTag = contact.SearchTag;
            }
        }
    }
}
