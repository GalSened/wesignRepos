import { UsersApiService } from './../../../services/users-api.service';
import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import { User } from 'src/app/models/user.model';
import { Observable, fromEvent } from 'rxjs';
import { Filter } from 'src/app/models/filter.model';
import { tap, map, filter, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { PagerService } from 'src/app/services/pager.service';
import { BannerAlertComponent } from '../../shared/banner-alert/banner-alert.component';
import { SharedService } from 'src/app/services/shared.service';
import { Errors } from 'src/app/models/error/errors.model';
import { IAppState, AppState } from 'src/app/state/app-state.interface';
import { Store } from '@ngrx/store';
import { userType } from 'src/app/models/user-type.enum';
import { UpdateUserType } from 'src/app/models/update-user-type.model';
import { PaginatorComponent } from '../../shared/paginator.component';
import { Operation } from 'src/app/enums/operaion.enum';
import { UserFormComponent } from './user-form/user-form.component';
import { HtmlTemplateFormComponent } from './html-template-form/html-template-form.component';
import { UserType } from 'src/app/enums/user-type.enum';


@Component({
  selector: 'app-users',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.css']
})
export class UsersComponent implements OnInit {


  public users$: Observable<User[]>;
  public userId: string = "";
  public userName: string = "";
  public currentPage = 1;
  private PAGE_SIZE = 10;
  private SUCCESS = 1;
  private FAILED = 2;
  public pageCalc: any;
  public isFormShown: boolean = false;
  public isHtmlTemplateFormShown: boolean = false;
  public filter: Filter = new Filter();
  public languagesOptions = { 1: "English", 2: "עברית" };
  public usertypes = userType;



  @ViewChild('searchInput', { static: true }) searchInput: ElementRef;
  @ViewChild('bannerAlert', { static: true }) bannerAlert: BannerAlertComponent;


  @ViewChild(PaginatorComponent) paginator: PaginatorComponent;
  @ViewChild('appUserForm', { static: true }) appUserForm: UserFormComponent;
  @ViewChild('appHtmlTemplateForm', { static: true }) appHtmlTemplateForm: HtmlTemplateFormComponent;

  public showDeletionAlert: boolean = false;
  public showUpdateFormAlert: boolean = false;
  public appState: AppState;
  public user: User = new User();
  public userNewType: number;
  public showResendAlert: boolean;
  public isSorted: boolean = false;
  resendPasswordEmailUser: User;

  public get UsersTypeOptions() {
    return SharedService.EnumToArrayHelper<userType>(userType)
  }

  constructor(private pager: PagerService,
    private usersApiService: UsersApiService,
    private sharedService: SharedService,
    private store: Store<IAppState>) { }



  ngOnInit(): void {
    this.store.select<any>('appstate').subscribe((state: any) => {
      this.appState = state;
      this.isFormShown = this.appState.ShouldShowUserForm;
      this.isHtmlTemplateFormShown = this.appState.ShouldShowUserForm;
    });
    this.updateData();
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
      this.currentPage = 1;
      this.updateData(true);
      this.paginator.reset();

    });
  }

  public pageChanged(page: number) {
    this.currentPage = page;
    this.updateData();
  }

  public updateData(shouldReset = false) {
    this.filter.limit = this.PAGE_SIZE;
    this.filter.offset = shouldReset ? 0 : (this.currentPage - 1) * this.PAGE_SIZE;
    this.users$ = this.usersApiService.Read(this.filter).pipe(
      tap((data) => {
        const total = +data.headers.get("x-total-count");
        this.pageCalc = this.pager.getPager(total, this.currentPage, this.PAGE_SIZE);
      }),
      map((res) => res.body.users),
    );

  }

  public openDeletionAlert(user: User) {
    this.userName = user.name;
    this.userId = user.id;
    this.showDeletionAlert = true;
  }

  public hideAlert() {
    this.showDeletionAlert = false;
  }

  public hideForm($event) {
    if ($event == 1) {
      this.updateData();
    }
    this.isFormShown = false;
    this.isHtmlTemplateFormShown = false;
  }

  public createUser() {
    this.isFormShown = !this.isFormShown;
    this.appUserForm.UpdateFormUI(Operation.Create, null);
  }

  public openUpdateUserForm(user: User) {
    this.isFormShown = !this.isFormShown;
    this.appUserForm.UpdateFormUI(Operation.Update, user);
  }

  public openHtmlTemplateForm(user: User) {
    this.isHtmlTemplateFormShown = !this.isHtmlTemplateFormShown;
    this.appHtmlTemplateForm.LoadUser(user);
  }

  openResendAlert(user: User) {
    this.resendPasswordEmailUser = user;
    this.showResendAlert = true;
  }

  deleteUser() {
    this.usersApiService.delete(this.userId).subscribe
      (
        () => {
          this.bannerAlert.showBannerAlert("User " + this.userName + "Successfully Removed", this.SUCCESS);
          this.updateData();
        }, (err) => {
          let errorMessage = this.sharedService.getErrorMessage(new Errors(err.error));
          this.bannerAlert.showBannerAlert(errorMessage, this.FAILED);
        }
      );
    this.hideAlert();
  }

  public showBannerAlert(event) {
    this.bannerAlert.showBannerAlert("user " + this.user.name + " Successfully Updated", 1);
  }


  public closeResendAlert() {
    this.showResendAlert = false;
  }

  public resendResetPassword() {
    this.showResendAlert = false;

    this.usersApiService.resendResetPassword(this.resendPasswordEmailUser.id).subscribe(
      (data) => {
        this.bannerAlert.showBannerAlert("Successfully Send ResetPassword Mail", this.SUCCESS);
      },
      (error) => {
        this.bannerAlert.showBannerAlert("Operation Failed", this.FAILED);
      }
    );
  }

  public nameSort() {
    this.users$ = this.users$.pipe(map((data) => {
      data.sort((a, b) => {
        var nameA = a.name.toLocaleLowerCase(), nameB = b.name.toLocaleLowerCase()
        return nameA < nameB ? this.isSorted ? 1 : -1 : this.isSorted ? -1 : 1;
      });
      return data;
    }))
    this.isSorted = !this.isSorted;
  }

  public emailSort() {
    this.users$ = this.users$.pipe(map((data) => {
      data.sort((a, b) => {
        var emailA = a.email.toLocaleLowerCase(), emailB = b.email.toLocaleLowerCase()
        return emailA < emailB ? this.isSorted ? 1 : -1 : this.isSorted ? -1 : 1;
      });
      return data;
    }))
    this.isSorted = !this.isSorted;
  }

  public creationTimeSort() {
    this.users$ = this.users$.pipe(map((data) => {
      data.sort((a, b) => {
        return new Date(a.creationTime).getTime() < new Date(b.creationTime).getTime() ? this.isSorted ? 1 : -1 : this.isSorted ? -1 : 1;
      });
      return data;
    }))
    this.isSorted = !this.isSorted;
  }

  public languageSort() {
    this.users$ = this.users$.pipe(map((data) => {
      data.sort((a, b) => {
        var languageA = a.language, languageB = b.language
        return languageA < languageB ? this.isSorted ? 1 : -1 : this.isSorted ? -1 : 1;
      });
      return data;
    }))
    this.isSorted = !this.isSorted;
  }

  public typeSort() {
    this.users$ = this.users$.pipe(map((data) => {
      data.sort((a, b) => {
        var typeA = a.type, typeB = b.type
        return typeA < typeB ? this.isSorted ? 1 : -1 : this.isSorted ? -1 : 1;
      });
      return data;
    }))
    this.isSorted = !this.isSorted;
  }

  public companySort() {
    this.users$ = this.users$.pipe(map((data) => {
      data.sort((a, b) => {
        var companyA = a.companyName.toLocaleLowerCase(), companyB = b.companyName.toLocaleLowerCase()
        return companyA < companyB ? this.isSorted ? 1 : -1 : this.isSorted ? -1 : 1;
      });
      return data;
    }))
    this.isSorted = !this.isSorted;
  }

  public groupSort() {
    this.users$ = this.users$.pipe(map((data) => {
      data.sort((a, b) => {
        var groupA = a.groupName, groupB = b.groupName
        return groupA < groupB ? this.isSorted ? 1 : -1 : this.isSorted ? -1 : 1;
      });
      return data;
    }))
    this.isSorted = !this.isSorted;
  }

  public programSort() {
    this.users$ = this.users$.pipe(map((data) => {
      data.sort((a, b) => {
        var programA = a.programName, programB = b.programName
        return programA < programB ? this.isSorted ? 1 : -1 : this.isSorted ? -1 : 1;
      });
      return data;
    }))
    this.isSorted = !this.isSorted;
  }


}
