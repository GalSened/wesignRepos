import { Contact } from './contact.model';

export class ContactsGroupsResponse
{
 public contactGroups : ContactsGroup[];
}


export class ContactsGroup
{
 
    public id:string;
    public name:string; 
    public contactsGroupMembers: ContactGroupMember[] = [];
}

export class ContactGroupMember
{

    public  id: string;
    public name : string;
    public contactId : string;
    public order : number;
    public contact: Contact;

}