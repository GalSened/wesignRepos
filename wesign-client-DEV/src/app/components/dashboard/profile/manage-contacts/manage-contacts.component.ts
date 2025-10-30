import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { ContactsGroup } from '@models/contacts/contacts-groups-result.model';
import { UserFilter } from '@models/managment/users/user-filter';
import { Modal } from '@models/modal/modal.model';
import { TranslateService } from '@ngx-translate/core';
import { ContactApiService } from '@services/contact-api.service';
import { PagerService } from '@services/pager.service';
import { SharedService } from '@services/shared.service';
import { Observable, fromEvent } from 'rxjs';
import { debounceTime, distinctUntilChanged, map, tap } from 'rxjs/operators';


@Component({
  selector: 'sgn-manage-contacts',
  templateUrl: './manage-contacts.component.html',
  styles: [
  ]
})
export class ManageContactsComponent  implements  OnInit, AfterViewInit{
  
  private PAGE_SIZE = 10;
  public currentPage = 1;
  public userFilter: UserFilter = new UserFilter();
  @ViewChild('searchInput', { static: true }) searchInput: ElementRef;
  public showSearchSpinner: boolean = false;
  public orderByField: string = 'name';
  public orderByDesc: boolean = false;
  public pageCalc: any; 
  public showManageGroup: boolean = false;
  public contactsGroups$: Observable<ContactsGroup[]>;
  public selectedContactsGroup  : ContactsGroup  = new ContactsGroup();
  public deleteGroupPopupData: Modal = new Modal();
  public isUpdateMode: boolean = false;
  public okModel: Modal = new Modal();
  private totalGroupsCount : number = 0;
  constructor(  private pager: PagerService,
  private sharedService: SharedService,
  private contactApiService : ContactApiService,
  private translate: TranslateService)
  {
    
  }
  ngOnInit(): void {
    this.updateData(true);
  }
  trackByFn(index) {
    return index;
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
      , distinctUntilChanged()
    ).subscribe((text: string) => {
      this.showSearchSpinner = true;
      this.updateData(false);
      
    });
  }
  
  toggleEditContactGroup(selectedContactGroup)
  {

    this.isUpdateMode = true;
    this.selectedContactsGroup = selectedContactGroup;
    this.showManageGroup = !this.showManageGroup;

  }
  deleteContactGroup(selectedContactGroup)
  {
    this.selectedContactsGroup = selectedContactGroup;
    this.deleteGroupPopupData.showModal = true;
    this.translate.get(['MANAGEMENT.DELETE_GROUP', 'MANAGEMENT.MODALTEXT', 'REGISTER.CANCEL', 'GLOBAL.DELETE'])
    .subscribe((res: object) => {
      let keys = Object.keys(res);
      this.deleteGroupPopupData.title = res[keys[0]];
      this.deleteGroupPopupData.content = (res[keys[1]] as string).replace("{#*#}",  this.selectedContactsGroup.name);
      this.deleteGroupPopupData.rejectBtnText = res[keys[2]];
      this.deleteGroupPopupData.confirmBtnText = res[keys[3]];
    });
 // need message are you sure and then delete
  }
  doDeleteGroupEvent(){
    this.deleteGroupPopupData.showModal = false;
    this.contactApiService.DeleteContactsGroup(this.selectedContactsGroup.id).subscribe(
      ()=>{
        this.updateData(true);
      }
    )
  }
  updateContactGroup($event)
  {
    this.contactApiService.UpdateContactGroup(this.selectedContactsGroup).subscribe(
()=>{
  this.updateData(true);
}
    )

    this.selectedContactsGroup = null;
    this.showManageGroup = !this.showManageGroup;

    
    
  }
  createContactGroup($event)
  {

    this.contactApiService.createContactsGroup($event).subscribe(
      ()=>{
        this.updateData(true);
      }      
    )

    this.selectedContactsGroup = null;
    this.showManageGroup = !this.showManageGroup;

  }
  cancel()
  {
    this.selectedContactsGroup = null;
    this.showManageGroup = !this.showManageGroup;
  }
  public updateData(showLoading) {
    this.userFilter.limit = this.PAGE_SIZE;
    this.userFilter.offset = (this.currentPage - 1) * this.PAGE_SIZE;

    if (showLoading) {
      this.sharedService.setBusy(true, "MANAGEMENT.LOADING");
    }

    this.contactsGroups$ = this.contactApiService.readContactsGroups(this.userFilter.key, this.userFilter.offset, this.userFilter.limit).pipe(
      tap((data) => {
        this.totalGroupsCount = +data.headers.get("x-total-count");
        this.pageCalc = this.pager.getPager(this.totalGroupsCount, this.currentPage, this.PAGE_SIZE);
      }),
      map((res) => res.body.contactGroups),
      tap(_ => {
        this.sharedService.setBusy(false);
        this.showSearchSpinner = false;
      })
    );

  }
  public toggleNewGroup()
  {
    this.isUpdateMode = false;
     if(this.totalGroupsCount > 29)
     {
      this.okModel.showModal = true;
      this.translate.get(['MANAGEMENT.ADD_CONTACTS_GROUP', 'CONTACTS.CONTACT_GROUPS_SIZE_LIMIT',  'BUTTONS.CLOSE'])
        .subscribe((res: object) => {
          let keys = Object.keys(res);
          this.okModel.title = res[keys[0]];
          this.okModel.content = res[keys[1]];            
          this.okModel.confirmBtnText = res[keys[2]];
  
        });
      return;
     }
    
   this.selectedContactsGroup = new ContactsGroup();
   this.showManageGroup = !this.showManageGroup;
  }

  public pageChanged(page: number) {
    this.currentPage = page;
    this.showSearchSpinner = true;
    this.updateData(false);
  }

  public orderByFunction(prop: string) {
    if (prop) {
      if (this.orderByField == prop) {
        this.orderByDesc = !this.orderByDesc;
      }
      this.orderByField = prop;
    }
  }
}
