import { AfterViewInit, Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { ContactFilter } from '@models/contacts/contact-filter.model';
import { Contact } from '@models/contacts/contact.model';
import { ContactApiService } from '@services/contact-api.service';
import { PagerService } from '@services/pager.service';
import { SharedService } from '@services/shared.service';
import { fromEvent, Observable } from 'rxjs';
import { map, tap, debounceTime, distinctUntilChanged } from "rxjs/operators";

@Component({
  selector: 'sgn-select-contact',
  templateUrl: './select-contact.component.html',
  styles: []
})
export class SelectContactComponent implements OnInit, AfterViewInit {

  public contacts$: Observable<Contact[]>;
  public contacts: Contact[];
  public contactsFilter: ContactFilter = new ContactFilter();
  @Input() public show: boolean;
  public pageCalc: any;
  public currentPage = 1;
  public showSearchSpinner: boolean = false;
  @Output() public hide = new EventEmitter<any>();
  @ViewChild('searchInput', { static: true }) searchInput: ElementRef;
  @Output() public sendContact = new EventEmitter<Contact>();
  PAGE_SIZE = 10;

  constructor(private contactsService: ContactApiService, private pager: PagerService, private sharedService: SharedService) { }

  ngOnInit() {
    this.updateData();
  }

  ngAfterViewInit(): void {
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
      , distinctUntilChanged(),
    ).subscribe((text: string) => {
      this.currentPage = 1;
      this.updateData();
    });
    this.searchInput.nativeElement.focus();
  }

  public updateData() {
    this.showSearchSpinner = true;
    this.contactsFilter.limit = this.PAGE_SIZE;
    this.contactsFilter.offset = (this.currentPage - 1) * this.PAGE_SIZE;
    this.contactsFilter.includeTabletMode = false;
    this.contacts$ = this.contactsService.getContacts(this.contactsFilter).pipe(
      tap((data) => {
        const total = +data.headers.get("x-total-count");
        this.contacts = data.body.contacts;
        if (this.currentPage > total / this.PAGE_SIZE) {

          this.currentPage = Math.ceil(total / this.PAGE_SIZE) > 0 ? Math.ceil(total / this.PAGE_SIZE) : 1;

        }
        this.pageCalc = this.pager.getPager(total, this.currentPage, this.PAGE_SIZE);

      }),
      map((res) => res.body.contacts),
      tap(_ => {
        this.sharedService.setBusy(false);
        this.showSearchSpinner = false;
      })
    );
  }

  cancel() {
    this.hide.emit();
    this.contactsFilter.key = "";
  }

  sendContactToParent(contact: Contact) {
    this.sendContact.emit(contact);
  }

  onContactMethodChanged(event, contact: Contact) {
    contact.defaultSendingMethod = event.target.selectedIndex + 1;
  }

  pageChanged(page: number) {
    this.currentPage = page;
    this.updateData();
  }
}