namespace DAL.DAOs.Contacts
{
    using Common.Enums;
    using Common.Enums.Contacts;
    using Common.Enums.Documents;
    using Common.Models;
    using DAL.DAOs.Documents.Signers;
    using DAL.DAOs.Users;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    public class ContactDAO
    {
        [Key]
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid GroupId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneExtension { get; set; }
        public string Phone { get; set; }
        public SendingMethod DefaultSendingMethod { get; set; }
        public CreationSource CreationSource { get; set; }
        public ContactStatus Status { get; set; }
        
        public DateTime LastUsedTime { get; set; } = DateTime.UtcNow;
        public virtual ICollection<ContactSealsDAO> Seals { get; set; }
        public virtual ICollection<SignerDAO> Signers { get; set; }
        public virtual ICollection<ContactGroupMemberDAO> ContactGroupsMember { get; set; }
        public virtual UserDAO User { get; set; }
        public string SearchTag { get; set; }

        public ContactDAO() {
            LastUsedTime = DateTime.UtcNow;
         }

        public ContactDAO(Contact contact)
        {
            Id = contact.Id == Guid.Empty ? default : contact.Id;
            UserId = contact.UserId == Guid.Empty ? default : contact.UserId;
            GroupId = contact.GroupId;
            Name = contact.Name;
            Email = contact.Email;
            Phone = contact.Phone;
            PhoneExtension = contact.PhoneExtension;
            LastUsedTime = contact.LastUsedTime;    
            DefaultSendingMethod = contact.DefaultSendingMethod;
            Status = contact.Status;
            Seals = ToContactSealsDAOs(contact.Seals);
            CreationSource = contact.CreationSource;
            SearchTag = contact.SearchTag;
        }

        private ICollection<ContactSealsDAO> ToContactSealsDAOs(IEnumerable<Seal> seals)
        {
            List<ContactSealsDAO> result = new List<ContactSealsDAO>();
            foreach (var seal in seals ?? Enumerable.Empty<Seal>())
            {
                result.Add(new ContactSealsDAO(seal));
            }
            return result;
        }

    
    }
}
