using Common.Models;
using DAL.DAOs.Companies;
using DAL.DAOs.Documents;
using DAL.DAOs.Groups;
using IO.ClickSend.ClickSend.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.DAOs.Contacts
{
    [Table("ContactsGroups")]
    public class ContactsGroupDAO
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CompanyId { get; set; }
        public Guid GroupId { get; set; }
        public virtual ICollection<ContactGroupMemberDAO> ContactGroupMembers { get; set; }

        public ContactsGroupDAO()
        {

        }

        public ContactsGroupDAO(ContactsGroup contactsGroup)
        {
            Id = contactsGroup.Id == Guid.Empty ? default : contactsGroup.Id;
            Name = contactsGroup.Name;
            CompanyId = contactsGroup.CompanyId;
            GroupId = contactsGroup.GroupId;
            ContactGroupMembers = new List<ContactGroupMemberDAO>();
            foreach (var member in contactsGroup.ContactGroupMembers ?? Enumerable.Empty<ContactGroupMember>())
            {
                ContactGroupMembers.Add(new ContactGroupMemberDAO(member));
            }
        }
    }
}
