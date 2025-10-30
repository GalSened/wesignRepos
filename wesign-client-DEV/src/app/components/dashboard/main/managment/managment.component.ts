import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { AdminApiService } from '@services/admin-api.service';
import { UserFilter } from '@models/managment/users/user-filter';
import { Observable, fromEvent } from 'rxjs';
import { User } from '@models/account/user.model';
import { PagerService } from '@services/pager.service';
import { SharedService } from '@services/shared.service';
import { ModalService } from '@services/modal.service';
import { TranslateService } from '@ngx-translate/core';
import { tap, map, debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { Errors } from '@models/error/errors.model';
import { userType } from '@models/enums/user-type.enum';
import { group } from '@models/managment/groups/management-groups.model';
import { UserApiService } from '@services/user-api.service';
import { Modal } from '@models/modal/modal.model';
import { Store } from '@ngrx/store';
import { IAppState } from '@state/app-state.interface';
import * as appActions from "@state/actions/app.actions";
@Component({
  selector: 'sgn-managment',
  templateUrl: './managment.component.html',
  styles: []
})
export class ManagmentComponent implements OnInit, AfterViewInit {

  private PAGE_SIZE = 10;
  public userFilter: UserFilter = new UserFilter();
  public pageCalc: any;
  public currentPage = 1;
  public orderByField: string = 'name';
  public orderByDesc: boolean = false;

  public users$: Observable<User[]>;
  public groups: group[];
  public groupsNames: string[];
  public newUser: User = new User();
  public allUsers: User[];
  public currentUser: User = new User();
  public activeUserId: string;
  @ViewChild('searchInput', { static: true }) searchInput: ElementRef;
  public showAddUser: boolean;
  public showManageGroup: boolean;
  userType: typeof userType = userType;
  isUpdateMode: boolean;
  errorMsg: any;
  deleteModal: Modal;
  deleteSubscription: any;
  public groupsToView: string;
  public showSearchSpinner: boolean = false;
  public get UsersTypeOptions() {
    return SharedService.EnumToArrayHelper<userType>(userType)
  };

  public groupsOptions() {
    return this.groups.reduce((prev, curr) => {
      prev[curr.groupId] = curr.name;
      return prev;
    }, {});
  }

  constructor(private adminApiService: AdminApiService,
    private usersApiService: UserApiService,
    private pager: PagerService,
    private sharedService: SharedService,
    private modalService: ModalService,
    private translate: TranslateService,
    private store: Store<IAppState>,) { }


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
      this.updateData(true);
    });
  }

  onRowClick(user: User) {
    if (this.activeUserId == user.id) {
      this.activeUserId = "";
    }
    else {
      this.activeUserId = user.id;
      this.groupsToView = "";
      user.additionalGroupsIds.forEach(groupid => {
        let foundGroup = this.groups.find(x => x.groupId == groupid);
        if (foundGroup) {
          if (this.groupsToView) {
            this.groupsToView += ", ";
          }
          this.groupsToView += foundGroup.name;
        }
      })
    }
  }

  ngOnInit() {
    this.updateData(true);
    this.usersApiService.getCurrentUser().subscribe(
      x => {
        this.currentUser = x;
      }
    );

    this.deleteSubscription =
      this.modalService.checkConfirm()
        .pipe(
          switchMap(res => { return res; }),
          switchMap((res) => {
            this.sharedService.setBusy(true, "MANAGEMENT.DELETING_USER");
            return this.adminApiService.deleteUser(res)
          })
        ).subscribe(
          res => {
            this.updateData(true);
            this.sharedService.setSuccessAlert("MANAGEMENT.DELETED_SUCCESSFULY");
            this.sharedService.setBusy(false);
          }, err => {
            this.sharedService.setErrorAlert(new Errors(err.error));
            this.sharedService.setBusy(false);
          }, () => { this.sharedService.setBusy(false); }
        );


  }

  public ngOnDestroy() {
    this.deleteSubscription.unsubscribe();
  }

  public updateData(showLoading) {
    this.userFilter.limit = this.PAGE_SIZE;
    this.userFilter.offset = (this.currentPage - 1) * this.PAGE_SIZE;

    if (showLoading) {
      this.sharedService.setBusy(true, "MANAGEMENT.LOADING");
    }

    this.users$ = this.adminApiService.getUsers(this.userFilter).pipe(
      tap((data) => {
        const total = +data.headers.get("x-total-count");
        this.pageCalc = this.pager.getPager(total, this.currentPage, this.PAGE_SIZE);
      }),
      map((res) => {
        this.allUsers = res.body.users;
        return res.body.users
      }),
      tap(_ => {
        this.sharedService.setBusy(false);
        this.showSearchSpinner = false;
      })
    );
    // two functions.
    this.adminApiService.getAllGroups().subscribe(x => {
      this.groups = x.groups;
      this.groupsNames = new Array();
      this.groups.forEach(element => {
        this.groupsNames.push(element.name);

      });
    }, (error) => {

      this.sharedService.setErrorAlert(new Errors(error.error));
    });
  }


  public restoreVal(elemVal: any) {
    elemVal.reset(elemVal.model);
  }

  public updateGroup(newValue: any, user: User, property: string) {
    if ((newValue != null) && (newValue !== user[property]) && this.groups.some(x => x.groupId == newValue)) {
      this.sharedService.setBusy(true, "MANAGEMENT.LOADING");
      user[property] = newValue;
      this.updateUser(user);

    }
  }

  public orderByFunction(prop: string) {
    if (prop) {
      if (this.orderByField == prop) {
        this.orderByDesc = !this.orderByDesc;
      }
      this.orderByField = prop;
    }
  }


  public updateUser(user: User) {
    this.adminApiService.updateUser(user).subscribe(
      (x) => {
        this.showAddUser = false;
        this.newUser = new User();
        this.sharedService.setSuccessAlert("MANAGEMENT.SAVED_SUCCESSFULY");
        if (user.id == this.currentUser.id) {
          this.store.dispatch(new appActions.FetchCurrentUserInfo({ needToFetch: true }));
        }
      },
      (error) => {
        let result = new Errors(error.error);
        this.translate.get(`SERVER_ERROR.${result.errorCode}`).subscribe(
          (msg) => {
            this.errorMsg = msg;
          });
      },
      () => {
        this.sharedService.setBusy(false);
        this.updateData(true);
      }
    );

  }

  trackByFn(index) {
    return index;
  }

  // is new group in list
  // is property is correct?
  // update

  public UpdateField(newValue: any, user: User, property: string) {
    if (newValue.valid && newValue.value !== user[property]) {
      user[property] = newValue.value;
      this.updateUser(user);


    }
  }



  public createUser(user: User) {
    this.sharedService.setBusy(true, "MANAGEMENT.LOADING");
    this.adminApiService.createNewUser(user).subscribe((res) => {
      this.sharedService.setSuccessAlert("MANAGEMENT.SAVED_SUCCESSFULY");
      this.cancel();
    }, (error) => {
      let result = new Errors(error.error);
      this.translate.get(`SERVER_ERROR.${result.errorCode}`).subscribe(
        (msg) => {
          this.errorMsg = msg;

        });
      this.sharedService.setBusy(false);
      this.activeUserId = "";
      this.updateData(true)

    });
  }


  public toggleNewUser() {
    this.errorMsg = "";
    this.isUpdateMode = false;
    this.showAddUser = !this.showAddUser;
  }
  public toggleManageGroup() {
    this.showManageGroup = !this.showManageGroup;
  }
  public GetCurrentGroupOption(groupId: string) {
    let group = this.groups.filter(item => {
      return item.groupId === groupId;
    });

    return group[0].name;
  }

  getGroupName(groupId) {
    if (this.groups) {
      var group = this.groups.find(x => x.groupId == groupId);
      if (group) {
        return group.name;
      }
      return "";
    }
  }

  getAllGroupNames(groupIds: string[]) {
    var groupNames: string[] = [];
    if (groupIds) {
      groupIds.forEach(id => {
        groupNames.push(this.getGroupName(id));
      });
    }
    return groupNames;
  }

  deleteUser(user) {
    this.adminApiService.deleteUser(user.id).subscribe(
      (x) => {
        this.updateData(true);
        this.sharedService.setSuccessAlert("Successfully delete user"); // need to translate ...        
      },
      (error) => {
        this.sharedService.setErrorAlert(new Errors(error.error));
      });

  }


  public deleteUserModal(user: User) {
    this.deleteModal = new Modal({ showModal: true });

    this.translate.get(['CONTACTS.MODALTITLE', 'CONTACTS.MODALTEXT', 'CONTACTS.MODALCANCELBTN', 'CONTACTS.MODALDELETEBTN']).subscribe((res: object) => {
      let keys = Object.keys(res);
      this.deleteModal.title = res[keys[0]];
      this.deleteModal.content = (res[keys[1]] as string).replace("{#*#}", user.name);

      this.deleteModal.rejectBtnText = res[keys[2]];
      this.deleteModal.confirmBtnText = res[keys[3]];

      let confirmAction = new Observable(ob => {
        ob.next(user.id);
        ob.complete();

        return { unsubscribe() { } };
      });
      this.deleteModal.confirmAction = confirmAction;

      this.modalService.showModal(this.deleteModal);
    });
  }

  editUser(user) {
    this.newUser = user;
    this.errorMsg = "";
    this.showAddUser = true;
    this.isUpdateMode = true;
    this.activeUserId = "";
  }

  cancel() {
    this.updateData(true);
    this.showAddUser = !this.showAddUser;
    this.newUser = new User();
    this.errorMsg = "";
    this.activeUserId = "";
  }

  public pageChanged(page: number) {
    this.currentPage = page;
    this.showSearchSpinner = true;
    this.updateData(false);
  }
}
