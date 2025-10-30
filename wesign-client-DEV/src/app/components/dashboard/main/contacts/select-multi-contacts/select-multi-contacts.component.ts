import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { ContactFilter } from '@models/contacts/contact-filter.model';
import { Contact, OtpContact, SendingMethod } from '@models/contacts/contact.model';
import { ContactsOriginMode } from '@models/enums/contacts-origin-mode.enum';
import { ContactApiService } from '@services/contact-api.service';
import { fromEvent, Observable, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, map } from 'rxjs/operators';

@Component({
  selector: 'sgn-select-multi-contacts',
  templateUrl: './select-multi-contacts.component.html',
  styles: []
})
export class SelectMultiContactsComponent implements OnInit {

  public contacts$: Observable<OtpContact[]>;
  public contactsFilter = new ContactFilter();
  @Input() public show: boolean;
  @Input() public contactsOriginMode = ContactsOriginMode.FromSystem;
  @Input() public contactsFromFile: OtpContact[] = [];
  @Output() public hide = new EventEmitter<any>();
  @ViewChild('searchInput', { static: true }) searchInput: ElementRef;
  @Output() public sendContacts = new EventEmitter<Contact[]>();

  public selectedContacts: OtpContact[] = [];
  areAllContactsSelected: any;
  private UNLIMITED = -1;
  useEmail = true;
  areAllContactsOtpSelected: any;

  constructor(private contactsService: ContactApiService) { }

  ngOnInit() {
    this.contactsFilter.limit = this.UNLIMITED;
    this.updateData();
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.initSearchInput();
    }, 2000);
  }

  initSearchInput() {
    fromEvent(this.searchInput.nativeElement, 'keyup').pipe(
      // get value
      map((event: any) => {
        return event.target.value;
      })
      // if character length greater then 2
      //,filter(res => res.length > 2)
      //Search function will not get called when the field is empty
      //,filter(Boolean)
      // Time in milliseconds between key events
      , debounceTime(1000)
      // If previous query is diffent from current   
      , distinctUntilChanged()
    ).subscribe((text: string) => {
      this.updateData();
    });
    this.searchInput.nativeElement.focus();
  }

  updateData() {
    if (this.contactsOriginMode == ContactsOriginMode.FromFile) {
      this.contacts$ = of(this.contactsFromFile);
      setTimeout(() => {
        let element = document.getElementById("selectAllId");
        if (element) {
          (<HTMLInputElement>element).checked = true;
          this.selectedAll({ target: { checked: true } });
        }
      }, 1000);
    }

    else {
      this.contacts$ = this.contactsService
        .getContacts(this.contactsFilter)
        .pipe(map(res => res.body.contacts.filter(x => x.defaultSendingMethod != SendingMethod.TABLET)
          .map(c => {
            let otpContact = new OtpContact(c);
            let elements = document.getElementsByName("otpCheckboxOverlay[]");
            elements.forEach(element => {
              if (element.id == c.id) {
                otpContact.shouldSendOTP = (<HTMLInputElement>element).checked;
              }
            });
            return otpContact;
          })));
      setTimeout(() => {
        this.markAllSelectedContacts();
      }, 1000);
    }
  }

  cancel() {
    this.hide.emit();
    this.contactsFilter.key = "";
  }

  sendContactsToParent() {
    this.sendContacts.emit(this.selectedContacts);
  }

  onContactMethodChanged(event, contact: OtpContact) {
    contact.defaultSendingMethod = event.target.selectedIndex + 1;
  }

  onRowClick(contact: OtpContact) {
    let element = document.getElementById(contact.id);
    if (element) {
      let isChecked = (<HTMLInputElement>element).checked;
      (<HTMLInputElement>element).checked = !isChecked;
      this.selected({ target: { checked: !isChecked } }, contact);
    }
  }

  selected(event, contact: OtpContact) {
    this.contacts$.forEach(element => {
      let c = element.find(x => x.id == contact.id);
      contact.shouldSendOTP = c.shouldSendOTP;
    });
    if (event.target.checked && !this.selectedContacts.find(x => x.id == contact.id)) {
      this.selectedContacts.push(contact);
    }
    else {
      this.selectedContacts = this.selectedContacts.filter(function (obj) {
        return obj.id !== contact.id;
      });
    }
  }

  selectOTP(event, contact: OtpContact) {

    if (event.target.checked) {
      //  if (event.target.checked && !this.selectedContacts.find(x => x.id == contact.id)) {
      contact.shouldSendOTP = true;
    }

    else {
      contact.shouldSendOTP = false;
    }

    let selectedContact = this.selectedContacts.find(x => x.id == contact.id);
    if (selectedContact) {
      selectedContact.shouldSendOTP = contact.shouldSendOTP;
    }
  }

  markAllSelectedContacts() {
    let elements = document.getElementsByName("checkboxOverlay[]");
    for (let index = 0; index < elements.length; index++) {
      const element = elements[index];
      let foundContact = this.selectedContacts.find(x => x.id == (<HTMLInputElement>element).id);
      if (foundContact) {
        (<HTMLInputElement>element).checked = true;
      }
    }
  }

  selectedAll(event) {
    this.areAllContactsSelected = event.target.checked;
    let elements = document.getElementsByName("checkboxOverlay[]");
    for (let index = 0; index < elements.length; index++) {
      const element = elements[index];
      (<HTMLInputElement>element).checked = this.areAllContactsSelected;
    }
    if (!this.areAllContactsSelected) {
      this.selectedContacts = [];
    }
    else {
      this.contacts$.forEach(element => {
        element.forEach(contact => {
          this.selectedContacts.push(contact);
        });
      });
    }
  }

  otpSelectedAll(event) {
    this.areAllContactsOtpSelected = event.target.checked;
    let elements = document.getElementsByName("otpCheckboxOverlay[]");
    for (let index = 0; index < elements.length; index++) {
      const element = elements[index];
      (<HTMLInputElement>element).checked = this.areAllContactsOtpSelected;
    }
    this.selectedContacts.forEach(element => {
      element.shouldSendOTP = this.areAllContactsOtpSelected;
    });
  }
}