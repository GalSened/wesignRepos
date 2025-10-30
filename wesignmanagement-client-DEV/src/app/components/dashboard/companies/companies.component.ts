import { Operation } from './../../../enums/operaion.enum';
import { Company } from './../../../models/company.model';
import { CompanyResult } from './../../../models/results/companany-result.model';
import { Observable, fromEvent } from 'rxjs';
import { CompaniesApiService } from 'src/app/services/companies-api.service';
import { Router } from '@angular/router';
import { Component, OnInit, ElementRef, ViewChild, Input, ChangeDetectorRef } from '@angular/core';
import { Filter } from 'src/app/models/filter.model';
import { PagerService } from 'src/app/services/pager.service';
import { tap, map, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { Errors } from 'src/app/models/error/errors.model';
import { BannerAlertComponent } from '../../shared/banner-alert/banner-alert.component';
import { SharedService } from 'src/app/services/shared.service';
import { AppState, IAppState } from 'src/app/state/app-state.interface';
import { Store } from '@ngrx/store';
import { UsersResult } from 'src/app/models/results/users-result.model';
import { User } from 'src/app/models/user.model';
import { UsersApiService } from 'src/app/services/users-api.service';
import { UserType } from 'src/app/enums/user-type.enum';
import { LicenseService } from 'src/app/services/license-api.service';
import { ActivateLicenseAction } from 'src/app/state/app.action';
import { PaginatorComponent } from '../../shared/paginator.component';
import { NgxSpinnerService } from 'ngx-spinner';
import { ConfigurationApiService } from 'src/app/services/configuration-api.service';
import { Configuration } from 'src/app/models/results/configuration.model';
import { Notifications } from 'src/app/models/notifications.model';


@Component({
  selector: 'app-companies',
  templateUrl: './companies.component.html',
  styleUrls: ['./companies.component.css']
})
export class CompaniesComponent implements OnInit {

  private PAGE_SIZE = 10;
  private SUCCESS = 1;
  private FAILED = 2;
  public currentPage = 1;
  public operation: Operation;
  public company: Company = new Company();
  public pageCalc: any;
  public isFormShown: boolean = false;
  public showResendAlert: boolean = false;
  public isAlert: boolean = false;
  public showDeletionAlert: boolean = false;
  public filter: Filter = new Filter();
  public companies$: Observable<CompanyResult[]>;
  public users$: Observable<User[]>;
  public companyAdminUsers = {};
  public appState: AppState;
  public UserType = UserType;
  public selectedCompanyId: any;
  public serProfiles$: Observable<Configuration>;
  public isSorted: boolean = false;

  @ViewChild('searchInput', { static: true }) searchInput: ElementRef;
  @ViewChild('bannerAlert', { static: true }) bannerAlert: BannerAlertComponent;
  @ViewChild(PaginatorComponent) paginator: PaginatorComponent;

  public showUsers: boolean = false;

  public enableVisualIdentityFlow: boolean = false;

  public shouldSendWithOTPByDefault: boolean = false;
  public recentPasswordsAmount: number;


  constructor(private route: Router,
    private pager: PagerService,
    private licenseApiService: LicenseService,
    private companiesApi: CompaniesApiService,
    private sharedService: SharedService,
    private usersApiService: UsersApiService,
    private store: Store<IAppState>,
    private configurationApiService: ConfigurationApiService,
    private spinner: NgxSpinnerService) { }

  ngOnInit(): void {
    this.configurationApiService.Read().subscribe(x => {
      this.enableVisualIdentityFlow = x.body.enableVisualIdentityFlow;
      this.shouldSendWithOTPByDefault = x.body.shouldSendWithOTPByDefault;
      this.recentPasswordsAmount = x.body.recentPasswordsAmount;
    })
    this.store.select<any>('appstate').subscribe((state: any) => {
      this.appState = state;
      this.isFormShown = this.appState.ShouldShowCompanyForm;
      if (!this.appState.IsActivated) {
        this.licenseApiService.read().subscribe(
          () => {


            this.store.dispatch(new ActivateLicenseAction({ Token: this.appState.Token, RefreshToken: this.appState.RefreshToken }));

          });
      }
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
      this.updateData(true);
      this.paginator.reset();

    });
  }

  public pageChanged(page: number) {
    this.currentPage = page;
    this.updateData();
  }

  public createCompany() {
    this.isFormShown = true;
    this.operation = Operation.Create;

    this.company = new Company();
    this.company.enableVisualIdentityFlow = this.enableVisualIdentityFlow;
    this.company.shouldSendWithOTPByDefault = this.shouldSendWithOTPByDefault;
    this.company.recentPasswordsAmount = this.recentPasswordsAmount;
    this.company.notifications = new Notifications();
    this.company.passwordExpirationInDays = 0;
    this.company.minimumPasswordLength = 8;
  }

  public hideForm(operation: Operation) {
    this.isFormShown = false;
    this.updateData();
  }

  public hideAlert() {
    this.showDeletionAlert = false;
  }

  public openDeletionAlert(companyResult: CompanyResult) {
    this.company = new Company();
    this.company.id = companyResult.id;
    this.showDeletionAlert = true;
  }

  public showBannerAlert(event) {
    this.bannerAlert.showBannerAlert("Company " + this.company.companyName + " Successfully Updated", 1);
    this.updateData();
  }

  public deleteCompany() {
    this.companiesApi.deleteCompany(this.company).subscribe((res) => {
      this.bannerAlert.showBannerAlert("Company " + this.company.companyName + "Successfully Removed", this.SUCCESS);
      this.updateData();
    }, (err) => {
      let errorMessage = this.sharedService.getErrorMessage(new Errors(err.error));
      this.bannerAlert.showBannerAlert(errorMessage, this.FAILED);
    });
    this.hideAlert();
    this.company = new Company();
  }

  public openResendAlert() {
    this.showResendAlert = true;
  }

  public closeResendAlert() {
    this.showResendAlert = false;
  }

  public resendResetPassword(companyResult: CompanyResult) {
    this.showResendAlert = false;
    let userId = this.getUserIdForCurrentRow(companyResult);

    this.companiesApi.resendResetPassword(userId).subscribe(
      (data) => {
        this.bannerAlert.showBannerAlert("Successfully Send ResetPassword Mail", this.SUCCESS);
      },
      (error) => {
        this.bannerAlert.showBannerAlert("Operation Failed", this.FAILED);
      }
    );

  }

  public editCompany(companyResult: CompanyResult) {
    this.operation = Operation.Update;
    let userId = this.getUserIdForCurrentRow(companyResult);
    if (userId == '') {
      this.bannerAlert.showBannerAlert("Please Select Company Admin.", this.FAILED);
      return;
    }
    this.spinner.show();
    let res = this.companiesApi.ReadCompany(companyResult.id, userId).subscribe(
      (data) => {
        let expandedCompany = data.body;
        this.company = new Company();
        this.company.id = expandedCompany.id;
        this.company.companyName = expandedCompany.companyName;
        this.company.groups = expandedCompany.groups;
        this.company.logoBase64String = expandedCompany.logoBase64String;
        this.company.signatureColor = expandedCompany.signatureColor;
        this.company.language = expandedCompany.language;
        this.company.user = expandedCompany.user;
        this.company.programId = expandedCompany.programId;
        this.company.expirationTime = expandedCompany.expirationTime;
        this.company.messageAfter = expandedCompany.messageAfter;
        this.company.messageAfterHebrew = expandedCompany.messageAfterHebrew;
        this.company.messageBefore = expandedCompany.messageBefore;
        this.company.messageBeforeHebrew = expandedCompany.messageBeforeHebrew;
        this.company.smtpConfiguration = expandedCompany.smtpConfiguration;
        this.company.smsConfiguration = expandedCompany.smsConfiguration;
        this.company.notifications = expandedCompany.notifications;
        this.company.deletionDetails = expandedCompany.deletionDetails;
        this.company.groupsADMapper = expandedCompany.activeDirectoryGroups;
        this.company.shouldSendWithOTPByDefault = expandedCompany.shouldSendWithOTPByDefault;
        this.company.shouldForceOTPInLogin = expandedCompany.shouldForceOTPInLogin;
        this.company.enableVisualIdentityFlow = expandedCompany.enableVisualIdentityFlow;
        this.company.enableDisplaySignerNameInSignature = expandedCompany.enableDisplaySignerNameInSignature;
        this.company.enableTabletsSupport = expandedCompany.enableTabletsSupport;
        // this.company.shouldEnableGovernmentSignatureFormat = expandedCompany.shouldEnableGovernmentSignatureFormat;
        this.company.defaultSigningType = expandedCompany.defaultSigningType;
        this.company.transactionId = expandedCompany.transactionId;
        this.company.isPersonalizedPFX = expandedCompany.isPersonalizedPFX;
        this.company.recentPasswordsAmount = expandedCompany.recentPasswordsAmount;
        this.company.passwordExpirationInDays = expandedCompany.passwordExpirationInDays;
        this.company.minimumPasswordLength = expandedCompany.minimumPasswordLength;
        this.company.shouldEnableMeaningOfSignatureOption = expandedCompany.shouldEnableMeaningOfSignatureOption;
        this.company.shouldEnableVideoConference = expandedCompany.shouldEnableVideoConference;
        this.company.shouldAddAppendicesAttachmentsToSendMail = expandedCompany.shouldAddAppendicesAttachmentsToSendMail;
        if (expandedCompany.companySigner1Details != null) {
          this.company.companySigner1Details = expandedCompany.companySigner1Details;
        }
        this.isFormShown = true;

        if (this.company.logoBase64String != null) {
          var blob = new Blob([this.company.logoBase64String], { type: 'image/png' });
          const dT = new ClipboardEvent('').clipboardData || new DataTransfer();
          dT.items.add(new File([blob], 'exist_logo.png'));
          // var input: any = document.getElementsByName('companyLogo')[0];

          //    input.files = dT.files;
        }

        if (this.company.smtpConfiguration.beforeSigningHtmlTemplateBase64String != null && this.company.smtpConfiguration.beforeSigningHtmlTemplateBase64String != '') {
          var blob = new Blob([this.company.smtpConfiguration.beforeSigningHtmlTemplateBase64String], { type: 'application/html' });
          const dT2 = new ClipboardEvent('').clipboardData || new DataTransfer();
          dT2.items.add(new File([blob], 'exist_before_email_template.html'));
          // var input: any = document.getElementsByName('emailBeforeHtmlTemplate')[0];
          // input.files = dT2.files;
        }

        if (this.company.smtpConfiguration.afterSigningHtmlTemplateBase64String != null && this.company.smtpConfiguration.afterSigningHtmlTemplateBase64String != '') {
          var blob = new Blob([this.company.smtpConfiguration.afterSigningHtmlTemplateBase64String], { type: 'application/html' });
          const dT2 = new ClipboardEvent('').clipboardData || new DataTransfer();
          dT2.items.add(new File([blob], 'exist_after_email_template.html'));
          // var input: any = document.getElementsByName('emailAfterHtmlTemplate')[0];
          //  input.files = dT2.files;
        }
        this.spinner.hide();
      },
      (err) => {
        let result = new Errors(err.error);
        this.spinner.hide();
      }
    );
  }

  public CollectionToOptions(companyAdminUsers: [string, string][]) {
    return companyAdminUsers.reduce((prev, curr) => {
      prev[curr["item1"]] = curr["item2"];
      return prev;
    }, {});
  }

  private updateData(shouldReset = false) {
    this.filter.limit = this.PAGE_SIZE;
    this.filter.offset = shouldReset ? 0 : (this.currentPage - 1) * this.PAGE_SIZE;
    this.companies$ = this.companiesApi.Read(this.filter).pipe(
      tap((data) => {
        const total = +data.headers.get("x-total-count");
        this.pageCalc = this.pager.getPager(total, this.currentPage, this.PAGE_SIZE);
      }),
      map((res) => res.body.companies),
    );
  }

  private getUserIdForCurrentRow(companyResult: CompanyResult) {
    let selectedUserEmail = document.getElementById(companyResult.id).children[0].children[0].childNodes[1].textContent.trim();
    let userId = "";
    companyResult.companyAdminUsers.forEach((user) => {
      if (user["item2"] === selectedUserEmail) {
        userId = user["item1"];
      }
    });
    return userId;
  }

  public showCompanyUsers(company: CompanyResult) {
    this.showUsers = !this.showUsers;
    this.selectedCompanyId = company.id;
    let filter: Filter = new Filter();
    filter.limit = -1;
    this.users$ = this.usersApiService.ReadAddUsersInCompany(company.id).pipe(
      tap(),
      map((res) => res.body.users),
    );
  }

  public companyNameSort() {
    this.companies$ = this.companies$.pipe(map((data) => {
      data.sort((a, b) => {
        var nameA = a.name.toLocaleLowerCase(), nameB = b.name.toLocaleLowerCase()
        return nameA < nameB ? this.isSorted ? 1 : -1 : this.isSorted ? -1 : 1;
      });
      return data;
    }))
    this.isSorted = !this.isSorted;
  }

  public programSort() {
    this.companies$ = this.companies$.pipe(map((data) => {
      data.sort((a, b) => {
        var programA = a.programName.toLocaleLowerCase(), programB = b.programName.toLocaleLowerCase()
        return programA < programB ? this.isSorted ? 1 : -1 : this.isSorted ? -1 : 1;
      });
      return data;
    }))
    this.isSorted = !this.isSorted;
  }

  public expiredDateSort() {
    this.companies$ = this.companies$.pipe(map((data) => {
      data.sort((a, b) => {
        return new Date(a.exipredTime).getTime() < new Date(b.exipredTime).getTime() ? this.isSorted ? 1 : -1 : this.isSorted ? -1 : 1;
      });
      return data;
    }))
    this.isSorted = !this.isSorted;
  }

  public documentsUsagePerMonthSort() {
    this.companies$ = this.companies$.pipe(map((data) => {
      data.sort((a, b) => {
        let num = this.isSorted ? 1 : -1;
        return num * (a.documents - b.documents);
      });
      return data;
    }))
    this.isSorted = !this.isSorted;
  }

  public templatesUsagePerMonthSort() {
    this.companies$ = this.companies$.pipe(map((data) => {
      data.sort((a, b) => {
        let num = this.isSorted ? 1 : -1;
        return num * (a.templates - b.templates);
      });
      return data;
    }))
    this.isSorted = !this.isSorted;
  }

  public usersUsagePerMonthSort() {
    this.companies$ = this.companies$.pipe(map((data) => {
      data.sort((a, b) => {
        let num = this.isSorted ? 1 : -1;
        return num * (a.users - b.users);
      });
      return data;
    }))
    this.isSorted = !this.isSorted;
  }

  public smsUsagePerMonthSort() {
    this.companies$ = this.companies$.pipe(map((data) => {
      data.sort((a, b) => {
        let num = this.isSorted ? 1 : -1;
        return num * (a.sms - b.sms);
      });
      return data;
    }))
    this.isSorted = !this.isSorted;
  }

}
