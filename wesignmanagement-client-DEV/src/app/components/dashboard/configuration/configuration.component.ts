import { Observable } from 'rxjs';
import { Configuration } from '../../../models/results/configuration.model';
import { Component, OnInit, ViewChild } from '@angular/core';
import { ConfigurationApiService } from 'src/app/services/configuration-api.service';
import { map, tap } from 'rxjs/operators';
import { Errors } from 'src/app/models/error/errors.model';
import { BannerAlertComponent } from '../../shared/banner-alert/banner-alert.component';
import { SharedService } from 'src/app/services/shared.service';
import { Company } from 'src/app/models/company.model';
import { AppState, IAppState } from 'src/app/state/app-state.interface';
import { Store } from '@ngrx/store';
import { ToolTip } from 'src/app/models/tool-tip.model';
import { LicenseService } from 'src/app/services/license-api.service';
import { SmsDetails } from 'src/app/models/sms-configuration.model';
import { SmtpDetails } from 'src/app/models/smtp-configuration.model';
import { NgxSpinnerService } from 'ngx-spinner';
import { config } from 'process';

@Component({
  selector: 'app-configuration',
  templateUrl: './configuration.component.html',
  styleUrls: ['./configuration.component.css']
})
export class ConfigurationComponent implements OnInit {
  
  private SUCCESS = 1;
  private FAILED = 2;
  public configuration$ : Observable<Configuration>;
  public isBusy : boolean = false;
  public isUnlimitedLogs : boolean = false;
  public isUnlimitedDeleteSignedDocuments : boolean = false;
  public isUnlimitedDeleteUnsignedDocuments : boolean = false;
  public showUpdateAlert : boolean = false;
  public isError : boolean = false;
  public smsProvidersOptions = { 1: "Goldman",2:"Twilio" , 3: "InforUMobile (Shamir)", 4 : "ClickSend", 5: "PayCall", 6:"Notify",7:"Telemessage",8:"Micropay", 9:"Notify G2", 12: "Center" };
  public portActiveDirectoryOptions= { 636: "Secured (port:636)",389: "Not Secured (port:389)" };
  public errorMessage : string = "Error - Please correct the marked fields";
  public showChangePassword: boolean = false;
  public toolTipInfo : ToolTip = new ToolTip();
  public appState: AppState;
  public isADSettingActiveInLicense : boolean = false;

  public isShownDefaultMessages :boolean= false;
  public isShownDefaultDelete :boolean= false;
  public isShownDefaultSMS :boolean= false;
  public isShownDefaultSMTP:boolean= false;
  public isShownDefaultActiveDirectory : boolean = false;
  public isShownDefaultVisualIdentity :boolean= false;

  public isShownPDFExternalService:boolean= false;
  public isShownHistoryIntegratorService:boolean= false;
  public isShownDefaultGraphicSignaturesSettings:boolean= false;
  public isSmsBusy: boolean = false;
  public smsMessage : string ;
  public smsPhone : string ;
  public isSmtpBusy: boolean = false;
  public smtpMessage : string ;
  public smtpEmail : string ; 
  
  @ViewChild('bannerAlert', { static: true }) bannerAlert: BannerAlertComponent;
  phoneExt: string = "972";
  
  constructor(private sharedService: SharedService,
              private configurationApiService : ConfigurationApiService,
              private licenseApiService: LicenseService,
              private store: Store<IAppState>,
              private spinner: NgxSpinnerService) { }

  ngOnInit(): void {
    this.store.select<any>('appstate').subscribe((state: any) => {
      this.appState = state;
    });
    this.updateData();
    this.sharedService.getToolTipsInfo().subscribe(
      (data)=>{
        this.toolTipInfo.messageBefore = (data as ToolTip).messageBefore;
        this.toolTipInfo.messageAfter = (data as ToolTip).messageAfter;    
        this.toolTipInfo.messageBeforeHebrew = (data as ToolTip).messageBeforeHebrew; 
        this.toolTipInfo.messageAfterHebrew = (data as ToolTip).messageAfterHebrew; 
        this.toolTipInfo.adHost = (data as ToolTip).adHost;     
        this.toolTipInfo.adPort = (data as ToolTip).adPort;     
        this.toolTipInfo.adContainer = (data as ToolTip).adContainer;     
        this.toolTipInfo.adUser = (data as ToolTip).adUser;     
        this.toolTipInfo.adDomain = (data as ToolTip).adDomain;     
        this.toolTipInfo.adPassword = (data as ToolTip).adPassword;     
        this.toolTipInfo.daysToDeleteSignedDoc = (data as ToolTip).daysToDeleteSignedDoc;     
        this.toolTipInfo.daysToDeleteUnsinedDoc = (data as ToolTip).daysToDeleteUnsinedDoc;     
        this.toolTipInfo.defaultSMSFrom = (data as ToolTip).defaultSMSFrom;     
        this.toolTipInfo.defaultSMSFromPassword = (data as ToolTip).defaultSMSFromPassword;     
        this.toolTipInfo.defaultSMSFromProvider = (data as ToolTip).defaultSMSFromProvider;     
        this.toolTipInfo.defaultSMSFromUser = (data as ToolTip).defaultSMSFromUser;     
        this.toolTipInfo.defaultSMTPSSL = (data as ToolTip).defaultSMTPSSL;     
        this.toolTipInfo.defaultSMTPFrom = (data as ToolTip).defaultSMTPFrom;     
        this.toolTipInfo.defaultSMTPServer = (data as ToolTip).defaultSMTPServer;     
        this.toolTipInfo.defaultSMTPPort = (data as ToolTip).defaultSMTPPort;     
        this.toolTipInfo.defaultSMTPUser = (data as ToolTip).defaultSMTPUser;     
        this.toolTipInfo.defaultSMTPPassword = (data as ToolTip).defaultSMTPPassword;     
        this.toolTipInfo.defaultSMTPMaxAttachmentSize = (data as ToolTip).defaultSMTPMaxAttachmentSize;     
        this.toolTipInfo.logsDeleteInterval = (data as ToolTip).logsDeleteInterval;     
        this.toolTipInfo.useOTPForManagmentSite = (data as ToolTip).useOTPForManagmentSite;     
        this.toolTipInfo.enableFreeTrileUsers = (data as ToolTip).enableFreeTrileUsers;     
        this.toolTipInfo.enableTabletsSupport = (data as ToolTip).enableTabletsSupport;
        this.toolTipInfo.signer1Endpoint = (data as ToolTip).signer1Endpoint;
        this.toolTipInfo.signer1User = (data as ToolTip).signer1User;
        this.toolTipInfo.signer1Password = (data as ToolTip).signer1Password;
        this.toolTipInfo.shouldUseReCaptchaInRegistration = (data as ToolTip).shouldUseReCaptchaInRegistration;
        this.toolTipInfo.shouldUseSignerAuth = (data as ToolTip).shouldUseSignerAuth;
        this.toolTipInfo.enableSigner1ExtraSigningTypes = (data as ToolTip).enableSigner1ExtraSigningTypes;
        this.toolTipInfo.shouldSendWithOTPByDefault = (data as ToolTip).shouldSendWithOTPByDefault;        
        this.toolTipInfo.enableVisualIdentityFlow = (data as ToolTip).enableVisualIdentityFlow;
        this.toolTipInfo.enableDisplaySignerNameInSignature = (data as ToolTip).enableDisplaySignerNameInSignature;
        this.toolTipInfo.externalPDFServiceURL = (data as ToolTip).externalPDFServiceURL;
        this.toolTipInfo.externalPDFServiceAPIKey = (data as ToolTip).externalPDFServiceAPIKey;
        this.toolTipInfo.historyIntegratorServiceURL = (data as ToolTip).historyIntegratorServiceURL;
        this.toolTipInfo.historyIntegratorServiceAPIKey = (data as ToolTip).historyIntegratorServiceAPIKey;
        this.toolTipInfo.shouldUseSignerAuthDefault = (data as ToolTip).shouldUseSignerAuthDefault;
        this.toolTipInfo.enableShowSSOOnlyInUserUI = (data as ToolTip).enableShowSSOOnlyInUserUI;
        
      }
    );

    this.licenseApiService.readSimpleInfo().subscribe(
      (res) => {       
          this.isADSettingActiveInLicense = res.licenseLimits.licenseCounters.useActiveDirectory;
       
      },
      (err) => {
        
      },
      () => {
      }
    );
  }

  public showAlert(){
    this.showUpdateAlert = true;
  }

  public hideAlert(){
    this.showUpdateAlert = false;
    this.isBusy = false;
  }

  public update(configuration: Configuration){
    this.showUpdateAlert = false;
    this.spinner.show();    
    configuration.logArichveIntervalInDays = this.isUnlimitedLogs ? -1 : configuration.logArichveIntervalInDays;
    configuration.deleteSignedDocumentAfterXDays = this.isUnlimitedDeleteSignedDocuments ? -1 : configuration.deleteSignedDocumentAfterXDays;
    configuration.deleteUnsignedDocumentAfterXDays = this.isUnlimitedDeleteUnsignedDocuments ? -1 : configuration.deleteUnsignedDocumentAfterXDays;    
    this.configurationApiService.Update(configuration).subscribe(
      () => {           
          window.scroll(0,0);
          this.bannerAlert.showBannerAlert("App Configuration Successfully Updated", this.SUCCESS);
          this.updateData();    
          this.spinner.hide();
      }, (err) => {
          window.scroll(0,0);
          let errorMessage = this.sharedService.getErrorMessage(new Errors(err.error));
          this.bannerAlert.showBannerAlert(errorMessage, this.FAILED);
          this.spinner.hide();
      }
    );
  }
  
  private updateData() {
    this.spinner.show();
    this.configuration$ = this.configurationApiService.Read().pipe(
      tap((data) => {
        this.handleUnlimitedInputs(data.body);
        this.spinner.hide();
      }),
      map((res) => res.body),
    );
  }

  public showChangePasswordForm(){
    this.showChangePassword = true;
  }

  public hideChangePasswordForm(){
    this.showChangePassword = false;
  }

  public dropDownUpdate(newValue, configuration: Configuration, property: string) {
    if (newValue && property == 'smsProvider') {
      configuration.smsProvider = newValue;
    }
    if (newValue && property == 'activeDirectoryPort') {
      configuration.activeDirecrotyConfiguration.port = newValue;
    }

  }

  public handleUnlimitedInputs(configuration : Configuration) {
    if (configuration.logArichveIntervalInDays == -1) {
      this.isUnlimitedLogs = true;
    }
    if (configuration.deleteSignedDocumentAfterXDays == -1) {
      this.isUnlimitedDeleteSignedDocuments = true;
    }
    if (configuration.deleteUnsignedDocumentAfterXDays == -1) {
      this.isUnlimitedDeleteUnsignedDocuments = true;
    }    
  }

  public sendSmsTestMessage(configuration: Configuration){
    this.spinner.show();    
    let smsDetails = new SmsDetails();
    smsDetails.message = this.smsMessage;
    smsDetails.phoneNumber = `+${this.phoneExt}${this.smsPhone}`;
    smsDetails.from = configuration.smsFrom;
    smsDetails.user = configuration.smsUser;
    smsDetails.password = configuration.smsPassword;
    smsDetails.provider = configuration.smsProvider;
    this.configurationApiService.SendSmsTestMessage(smsDetails).subscribe(
      () => {           
        window.scroll(0,0);
        this.bannerAlert.showBannerAlert("Send SMS Successfully", this.SUCCESS);
        this.spinner.hide();
      }, (err) => {
          window.scroll(0,0);
          let errorMessage = this.sharedService.getErrorMessage(new Errors(err.error));
          this.bannerAlert.showBannerAlert(errorMessage, this.FAILED);
          this.spinner.hide();
      }
    );
  }

  public sendSmtpTestMessage(configuration: Configuration){
    this.spinner.show();
    let smtpDetails = new SmtpDetails();
    smtpDetails.message = this.smtpMessage;
    smtpDetails.email = this.smtpEmail;
    smtpDetails.from = configuration.smtpFrom;
    smtpDetails.user = configuration.smtpUser;
    smtpDetails.password = configuration.smtpPassword;
    smtpDetails.server = configuration.smtpServer;
    smtpDetails.port = configuration.smtpPort;
    smtpDetails.enableSsl = configuration.smtpEnableSsl;
    
    this.configurationApiService.SendSmtpTestMessage(smtpDetails).subscribe(
      () => {           
        window.scroll(0,0);
        this.bannerAlert.showBannerAlert("Send Email Successfully", this.SUCCESS);
        this.spinner.hide();
      }, (err) => {
          window.scroll(0,0);
          let errorMessage = this.sharedService.getErrorMessage(new Errors(err.error));
          this.bannerAlert.showBannerAlert(errorMessage, this.FAILED);
          this.spinner.hide();
      }
    );
  }

  onCountryChange(obj) {
    this.phoneExt = obj.dialCode;    
  }

  shouldUseSignerAuthChanged(configuration: any)
  {
    if (!configuration.shouldUseSignerAuth)
    {
      configuration.shouldUseSignerAuthDefault = false;
    }
  }

}
