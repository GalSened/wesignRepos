using Common.Extensions;
using Common.Models;
using DAL.DAOs.Contacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Extensions
{
    public static class ContactsGroupExtentions
    {

        public static ContactsGroup ToContactsGroup(this ContactsGroupDAO contactsGroupDAO)
        {
            if (contactsGroupDAO == null)
            {
                return null;
            }
            var contactsGroup = new ContactsGroup()
            {
                Id = contactsGroupDAO.Id,
                CompanyId = contactsGroupDAO.CompanyId,
                GroupId = contactsGroupDAO.GroupId,
                Name = contactsGroupDAO.Name,

            };
            if (contactsGroupDAO.ContactGroupMembers != null)
            {
                contactsGroup.ContactGroupMembers = contactsGroupDAO.ContactGroupMembers.Select(item => item.ToContactGroupMember()).ToList();
            }
            return contactsGroup;
        }

        public static ContactGroupMember ToContactGroupMember(this ContactGroupMemberDAO contactGroupMemberDAO)
        {
            return contactGroupMemberDAO == null ? null :
                new ContactGroupMember()
                {
                    Id = contactGroupMemberDAO.Id,
                    Contact = contactGroupMemberDAO?.Contact?.ToContact(),
                    ContactId = contactGroupMemberDAO.ContactId,
                    ContactsGroupId = contactGroupMemberDAO.ContactsGroupId,
                    Order = contactGroupMemberDAO.Order,
                    
                };
        }
    }
}
