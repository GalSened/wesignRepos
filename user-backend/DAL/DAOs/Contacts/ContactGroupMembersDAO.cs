using Common.Models;
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
    [Table("ContactGroupMembers")]
    public class ContactGroupMemberDAO
    {
        
        [Key]
        public Guid Id { get; set; }
        public Guid ContactsGroupId { get; set; }
        public Guid ContactId { get; set; }
        public int Order { get; set; }
        public virtual ContactsGroupDAO ContactsGroup { get; set; }
        public virtual ContactDAO Contact { get; set; }

        public ContactGroupMemberDAO()
        {

        }
        public ContactGroupMemberDAO(ContactGroupMember contactGroupMember)
        {
            Id = contactGroupMember.Id == Guid.Empty ? default : contactGroupMember.Id;
            ContactsGroupId = contactGroupMember.ContactsGroupId == Guid.Empty ? default : contactGroupMember.ContactsGroupId;
            ContactId = contactGroupMember.ContactId;
            Order = contactGroupMember.Order;

        }


    }
}
