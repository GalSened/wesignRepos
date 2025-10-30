namespace WeSign.Models.Contacts.Responses
{
    using System.Collections.Generic;   

    public class AllContactResponseDTO
    {
        public IEnumerable<ContactResponseDTO> Contacts { get; set; }
    }
}
