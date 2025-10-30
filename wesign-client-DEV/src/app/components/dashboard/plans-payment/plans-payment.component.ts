import { AfterViewInit, Component, OnInit } from '@angular/core';
import { SharedService } from '@services/shared.service';
import { Observable } from 'rxjs';
import { UserApiService } from '@services/user-api.service';
import { User } from '@models/account/user.model';
import { environment } from "../../../../environments/environment"
import { Plan } from '@models/account/plan.model';
import { X } from 'angular-feather/icons';
import { HttpClient } from '@angular/common/http';
import { ProgramResetType, UserProgram } from '@models/program/user-program.model';
import { Modal } from '@models/modal/modal.model';
import { TranslateService } from '@ngx-translate/core';
import { Errors } from '@models/error/errors.model';
import { RedirectLinkResult } from '@models/Users/redirect-link-result.model';
import { stringToFileBuffer } from '@angular-devkit/core/src/virtual-fs/host';


@Component({
  selector: 'sgn-plans-payment',
  templateUrl: './plans-payment.component.html',
  styles: []
})
export class PlansPaymentComponent implements OnInit {

  public plans: Plan[];
  public user: User = new User();
  public tiny = environment.tiny;
  public remiderDocs: string;
  public remainingIdentifications: string;
  public remainVideoConference: string;
  public remiderTemplates: string;
  public remiderSMS: string;
  public diffDays: number;
  public showUpgradePlan: boolean = true;
  public showYearly: boolean = true;
  public showSubscription: boolean = false;
  public fromIsrael: boolean = false;
  public renewProgramDate: Date;
  public showRenewProgramDate: boolean;
  public remiderUsers: string;
  public usersLimit: string = "";
  public templatesLimit: string = "";
  public smsLimit: string = "";
  public documentsForMonth: string = "";
  public visualIdentificationsLimit: string = ""
  public unsubsribePopupData: Modal = new Modal();
  public okModel: Modal = new Modal();


  constructor(private userApiService: UserApiService,
    private httpClient: HttpClient,
    private sharedService: SharedService,
    private translate: TranslateService) {
    this.user.program = new UserProgram();

  }

  ngOnInit() {
    this.updateCurrentUser();
  }

  private updateCurrentUser() {
    this.userApiService.getCurrentUser().subscribe((result) => {
      this.user = result;
      this.httpClient.get("assets/Plans.json").subscribe(data => {
        this.plans = data as Plan[];
        if (this.showYearly) {
          this.plans = this.plans.filter(item => item.numberOfMonths == 12);
        }
        else {
          this.plans = this.plans.filter(item => item.numberOfMonths != 12);
        }
      })

      let unlimitedText: string = this.translate.currentLang == "en" ? "Unlimited" : "ללא הגבלה"
      this.remiderDocs = this.user.program.remainingDocumentsForMonth == -1 ? unlimitedText : String(this.user.program.remainingDocumentsForMonth);
      this.remainingIdentifications = this.user.program.remainingVisualIdentifications == -1? unlimitedText : String(this.user.program.remainingVisualIdentifications)
      this.remainVideoConference = this.user.program.remainingVideoConference == -1 ? unlimitedText : String(this.user.program.remainingVideoConference);
      this.documentsForMonth = this.user.program.documentsForMonth == -1 ? unlimitedText : String(this.user.program.documentsForMonth);
      this.remiderSMS = this.user.program.remainingSMS == -1 ? unlimitedText : String(this.user.program.remainingSMS);
      this.smsLimit = this.user.program.smsLimit == -1 ? unlimitedText : String(this.user.program.smsLimit);
      this.visualIdentificationsLimit = this.user.program.visualIdentificationsLimit == -1 ? unlimitedText: String(this.user.program.visualIdentificationsLimit)
      this.remiderTemplates = this.user.program.remainingTemplates == -1 ? unlimitedText : String(this.user.program.remainingTemplates);
      this.templatesLimit = this.user.program.templatesLimit == -1 ? unlimitedText : String(this.user.program.templatesLimit);
      this.remiderUsers = this.user.program.remainingUsers == -1 ? unlimitedText : String(this.user.program.remainingUsers);
      this.usersLimit = this.user.program.usersLimit == -1 ? unlimitedText : String(this.user.program.usersLimit);
      var date = new Date();
      const currentUTCTime = new Date(date.getTime() + date.getTimezoneOffset() * 60000);
      var date2 = new Date(this.user.program.expiredTime);
      this.diffDays = Math.floor((date2.getTime() - currentUTCTime.getTime()) / 1000 / 60 / 60 / 24);
      this.showRenewProgramDate = this.user.program.remainingDocumentsForMonth != -1 || this.user.program.remainingSMS != -1 || this.user.program.remainingVisualIdentifications != -1 ;

     
      this.renewProgramDate = new Date(this.user.program.lastResetDate);

      this.showRenewProgramDate  = this.showRenewProgramDate &&  date2 > this.renewProgramDate;
      if (this.user.program.programResetType == ProgramResetType.Yearly) {
        this.renewProgramDate.setDate(this.renewProgramDate.getDate() + 365)
      }
      else {
        this.renewProgramDate.setDate(this.renewProgramDate.getDate() + 30)
      }
      if (this.user.transactionId) {
        this.showSubscription = true;
      }
      else {
        this.showSubscription = false;
      }
      if (this.diffDays < 0) {
        this.diffDays = 0;
      }
    });
  }
  public changeSelection() {
    this.showYearly = !this.showYearly;
    this.httpClient.get("assets/Plans.json").subscribe(data => {
      this.plans = data as Plan[];
      if (this.showYearly) {
        this.plans = this.plans.filter(item => item.numberOfMonths == 12);
      }
      else {
        this.plans = this.plans.filter(item => item.numberOfMonths != 12);
      }
    })

  }
  public Select(isFromIsrael) {

    this.showUpgradePlan = false;
    this.fromIsrael = isFromIsrael;
  }


  public changePaymentRule()
  {
    this.sharedService.setBusy(true);
    this.userApiService.ChangePaymentRole().subscribe(
      (redirectLinkResult :RedirectLinkResult) =>{
        

        if(redirectLinkResult.link)
        {
          window.location.href=redirectLinkResult.link;
        }
        else
        {
          this.sharedService.setErrorAlert("GLOBAL.ERROR_CHANGE_PAYMENT");      
        }
        this.sharedService.setBusy(false);
      // show message of cancel subsccription
      },
      (err)=>{
        this.sharedService.setErrorAlert(new Errors(err.error));
        this.sharedService.setBusy(false);
      },
      () =>{
        this.sharedService.setBusy(false);
      }
    );

    
  }

  public unsubsribePaymetProgram() {

    this.unsubsribePopupData.showModal = true;
    this.translate.get(['BUTTONS.UNSELECT_ALL', 'GLOBAL.ARE_YOU_SURE_CANCEL_SUBSCRIPTION', 'REGISTER.CANCEL', 'REGISTER.SUBMIT'])
      .subscribe((res: object) => {
        let keys = Object.keys(res);
        this.unsubsribePopupData.title = res[keys[0]];
        this.unsubsribePopupData.content = res[keys[1]];
        this.unsubsribePopupData.rejectBtnText = res[keys[2]];
        this.unsubsribePopupData.confirmBtnText = res[keys[3]];

      });
  }
  public doDUnsubsribeEvent() {

    this.unsubsribePopupData.showModal = false;
    this.sharedService.setBusy(true);
    this.userApiService.unSubscribeUser().subscribe(
      () => {
        this.updateCurrentUser();


        this.okModel.showModal = true;
        this.translate.get(['BUTTONS.CANCEL_SUBSCRIPTION', 'GLOBAL.SUBSCRIPTION_CANCELLATION_DONE',  'BUTTONS.CLOSE'])
          .subscribe((res: object) => {
            let keys = Object.keys(res);
            this.okModel.title = res[keys[0]];
            this.okModel.content = res[keys[1]];            
            this.okModel.confirmBtnText = res[keys[2]];
    
          });

        
        this.sharedService.setBusy(false);
        
      },
      (err) => {
        this.sharedService.setErrorAlert(new Errors(err.error));
        this.sharedService.setBusy(false);
      },
      () => {
        this.sharedService.setBusy(false);
      }


    );

  }
}
