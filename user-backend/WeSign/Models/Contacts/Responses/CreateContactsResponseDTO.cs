using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeSign.Models.Contacts.Responses
{
    public class CreateContactsResponseDTO
    {
        public CreateContactsResponseDTO()
        {
            this.ContactsId = new List<Guid>();
        }
        public IEnumerable<Guid> ContactsId { get; set; }
    }
}
