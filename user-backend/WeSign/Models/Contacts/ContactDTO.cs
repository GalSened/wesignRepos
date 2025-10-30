namespace WeSign.Models.Contacts
{
    using Common.Enums.Documents;
    using System.Collections.Generic;
    using System.Linq;

    public class ContactDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PhoneExtension { get; set; }
        public SendingMethod DefaultSendingMethod { get; set; }
        public IEnumerable<SealDTO> Seals { get; set; }
        public string SearchTag { get; set; }

        public ContactDTO()
        {
            Seals = new List<SealDTO>();
        }
    }
}
