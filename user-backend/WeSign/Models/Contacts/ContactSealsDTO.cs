namespace WeSign.Models.Contacts
{
    using System.Collections.Generic;

    public class ContactSealsDTO
    {
       public IEnumerable<SealDTO> Seals { get; set; }

        public ContactSealsDTO()
        {
            Seals = new List<SealDTO>();
        }
    }
}
