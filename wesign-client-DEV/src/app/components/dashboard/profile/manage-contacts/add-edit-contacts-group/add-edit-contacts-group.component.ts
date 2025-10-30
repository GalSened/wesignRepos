import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Contact } from '@models/contacts/contact.model';
import { ContactGroupMember, ContactsGroup } from '@models/contacts/contacts-groups-result.model';
import { TranslateService } from '@ngx-translate/core';
import { SharedService } from '@services/shared.service';

@Component({
  selector: 'sgn-add-edit-contacts-group',
  templateUrl: './add-edit-contacts-group.component.html',
  styles: [
  ]
})
export class AddEditContactsGroupComponent implements OnInit{


  public submitted: boolean = false;

  @Input()
  public contactsGroup: ContactsGroup;
  @Input()
  public isUpdateMode: boolean;

  @Output()
  public accept = new EventEmitter<ContactsGroup>();
  @Output()
  public update = new EventEmitter<ContactsGroup>();

  @Output()
  public cancel = new EventEmitter<void>();
  public errorMsg: string = "";

  public currentSelectedMember : ContactGroupMember;
  public showContacts:boolean = false;
  constructor(private sharedService: SharedService,
    private translate: TranslateService,) {

     }
     remove(contactGroupMember){
      this.contactsGroup.contactsGroupMembers = this.contactsGroup.contactsGroupMembers.filter(x => x!=contactGroupMember );
     }

     openContactsBook(contactGroupMember){
      this.currentSelectedMember = contactGroupMember;
      this.showContacts = true;
     }

    back(){
this.contactsGroup.contactsGroupMembers = this.contactsGroup.contactsGroupMembers.filter(x => x.contactId );
      this.errorMsg = '';
      if(this.cancel)
      {
        this.cancel.emit();
      }
    }
  
    save()
  {
    // validate name
    // validate all the member selected
    
    this.errorMsg = '';
   if(!this.contactsGroup.name || !this.contactsGroup.name.trim())
   {
  
      this.submitted = false;
      this.translate.get(`CONTACTS.CONTACT_GROUP_NAME_IS_MANDATORY`).subscribe(
        (msg) => {
          this.errorMsg = msg;
        });    
    return ;
   }

   if(this.contactsGroup.contactsGroupMembers.some(x => !x.contactId ) )
   {
    this.submitted = false;
    this.translate.get(`CONTACTS.ITEMS_IN_GROUP_WITHOUT_CONTACT`).subscribe(
      (msg) => {
        this.errorMsg = msg;
      });    
  return ;
   }
    
   this.contactsGroup.contactsGroupMembers.forEach((value, index) => value.order = index + 1);

   if(this.isUpdateMode)
   {
    this.update.emit(this.contactsGroup);
   }
   else
   {
    this.accept.emit(this.contactsGroup);
   }


  }

  
  public updateSigner(contact: Contact, isFromSession: boolean, index: number) {

    this.showContacts = false;
    if(contact && this.currentSelectedMember )
    {
      this.currentSelectedMember.name = contact.name;
      this.currentSelectedMember.contactId = contact.id;
    }
    // this.contacts.push(contact);
    this.currentSelectedMember = null;
  }


  public moveSigner(index, direction){
    let deleted = this.contactsGroup.contactsGroupMembers.splice(index, 1);
    this.contactsGroup.contactsGroupMembers.splice(index + direction, 0, deleted.shift());
  }




  ngOnInit() {
  }




  addSigner()
  {
    if(this.contactsGroup.contactsGroupMembers.length < 25)
    {        
      this.contactsGroup.contactsGroupMembers.push(new ContactGroupMember());

    }
  }
}
