using Common.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using WeSign.Models.Contacts;

namespace WeSign.Validators.ContactValidators
{
    public class ContactsGroupValidator : AbstractValidator<ContactsGroupDTO>
    {
        public ContactsGroupValidator() {
            RuleFor(input => input.Name).NotEmpty().NotNull().WithMessage(
                "Group name is mandatory").Length(1, 50).WithMessage("Group name length limit to 50"); ;
            RuleFor(input => input.ContactsGroupMembers).Must(CheckGroupMembersContactIDExist).WithMessage("Need to set contactId for all members");
            RuleFor(input => input.ContactsGroupMembers).Must(CheckGroupMembersOrderIsCorrect).WithMessage("wrong order in Contacts in group");
            RuleFor(input => input.ContactsGroupMembers).Must(CheckGroupMembersLessThanLimit).WithMessage("Max member amount in group is 25");
        }

        private bool CheckGroupMembersLessThanLimit(List<ContactGroupMemberDTO> contacts)
        {
            if (contacts != null)
            {
                if (contacts.Count > 25)
                {
                   return false;
                }
            }
            return true;
        }

        private bool CheckGroupMembersOrderIsCorrect(List<ContactGroupMemberDTO> contacts)
        {
            if(contacts != null)
            {
          
                var orderGroup = contacts.OrderBy(x => x.Order).ToList();

                for (int i = 0; orderGroup.Count() < i; ++i)
                {
                    if (orderGroup[i].Order != i + 1)
                    {
                        return false;
                    }

                }
            }
            return true;
        }

        private bool CheckGroupMembersContactIDExist(List<ContactGroupMemberDTO> contacts)
        {
            if (contacts != null)
            {



                foreach (var contact in contacts)
                {
                    if(contact == null || contact.ContactId == Guid.Empty) 
                    {
                        return false;
                    }

                }
            }
            return true;
        }
    }
}
