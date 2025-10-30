import { AfterViewInit, Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { ContactsGroup } from '@models/contacts/contacts-groups-result.model';
import { UserFilter } from '@models/managment/users/user-filter';
import { TranslateService } from '@ngx-translate/core';
import { ContactApiService } from '@services/contact-api.service';
import { PagerService } from '@services/pager.service';
import { SharedService } from '@services/shared.service';
import { Observable, fromEvent } from 'rxjs';
import { debounceTime, distinctUntilChanged, map, tap } from 'rxjs/operators';

@Component({
  selector: 'sgn-select-contacts-group',
  templateUrl: './select-contacts-group.component.html',
  styles: [
  ]
})
export class SelectContactsGroupComponent implements OnInit, AfterViewInit {

  @Output() public sendContactsGroup = new EventEmitter<ContactsGroup>();
  @Output() public hide = new EventEmitter<any>();
  @Input() public show: boolean;

  PAGE_SIZE = 10;
  public contactsGroupFilter = new UserFilter();
  @ViewChild('searchInput', { static: true }) searchInput: ElementRef;
  public contactsGroups$: Observable<ContactsGroup[]>;
  public pageCalc: any;
  currentPage = 1;
  totalGroupsCount = 0;
  inProcess = false;

  constructor(private pager: PagerService, private sharedService: SharedService,
    private contactApiService: ContactApiService, private translate: TranslateService) { }

  ngAfterViewInit(): void {
    this.updateData();
  }

  ngOnInit(): void {
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
  }

  pageChanged(page: number) {
    this.currentPage = page;
    this.updateData();
  }

  sendContactToParent(contactsGroup: ContactsGroup) {
    if (this.inProcess) {
      return;
    }
    this.inProcess = true;
    this.contactApiService.readGroup(contactsGroup).subscribe(
      contactGroup => {
        this.sendContactsGroup.emit(contactGroup);
      },
      err => {
      },
      () => {
        this.inProcess = false;
      });
  }

  updateData() {
    this.contactsGroupFilter.limit = this.PAGE_SIZE;
    this.contactsGroupFilter.offset = (this.currentPage - 1) * this.PAGE_SIZE;
    this.contactsGroups$ = this.contactApiService.readContactsGroups(this.contactsGroupFilter.key, this.contactsGroupFilter.offset, this.contactsGroupFilter.limit).pipe(
      tap((data) => {
        this.totalGroupsCount = +data.headers.get("x-total-count");
        if (this.currentPage > this.totalGroupsCount / this.PAGE_SIZE) {
          this.currentPage = Math.ceil(this.totalGroupsCount / this.PAGE_SIZE) > 0 ? Math.ceil(this.totalGroupsCount / this.PAGE_SIZE) : 1;
        }

        this.pageCalc = this.pager.getPager(this.totalGroupsCount, this.currentPage, this.PAGE_SIZE);

      }),
      map((res) => res.body.contactGroups),
      tap(_ => {
        this.sharedService.setBusy(false);

      }));
  }

  cancel() {
    this.hide.emit();
    this.contactsGroupFilter.key = "";
  }
}