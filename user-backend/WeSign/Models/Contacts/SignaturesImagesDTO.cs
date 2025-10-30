using System;
using System.Collections.Generic;

namespace WeSign.Models.Contacts
{
    public class SignaturesImagesDTO
    {
        public Guid DocumentCollectionId { get; set; }
        public List<string> SignaturesImages { get; set; }
    }
}
