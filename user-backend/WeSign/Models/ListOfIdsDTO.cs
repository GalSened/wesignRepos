using Common.Enums.Contacts;
using Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Extensions;

namespace WeSign.Models
{
    public class ListOfIdsDTO
    {
        public IEnumerable<Guid> Ids { get; set; }
    }


    public class ModelsToDeleteDTO
    {
        public List<ModelToDelete> Models { get; set; }
        
        public ModelsToDeleteDTO()
        {
            Models = new List<ModelToDelete>();
        }

        public ModelsToDeleteDTO(IEnumerable<Contact> contacts)
        {
            Models = new List<ModelToDelete>();
            contacts.ForEach(contact => {
                Models.Add(new ModelToDelete(contact));
            });
        }
    }

    public class ModelToDelete
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }

        public ModelToDelete(Contact contact)
        {
            Id = contact.Id;
            IsDeleted = contact.Status == ContactStatus.Deleted;

        }
    }
}
