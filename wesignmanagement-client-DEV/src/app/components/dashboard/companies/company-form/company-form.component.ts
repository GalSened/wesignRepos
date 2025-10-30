import { filter } from 'rxjs/operators';
import { KeyValue } from '@angular/common';
import { Operation } from './../../../../enums/operaion.enum';
import { ProgramsApiService } from './../../../../services/programs-api.service';
import { Router } from '@angular/router';
import { Component, OnInit, Output, EventEmitter, ElementRef, ViewChild, Input, OnChanges, ChangeDetectorRef } from '@angular/core';
import { Company } from 'src/app/models/company.model';
import { CompaniesApiService } from 'src/app/services/companies-api.service';
import { Errors } from 'src/app/models/error/errors.model';
import { ProgramFilter } from 'src/app/models/program-filter.model';
import { ProgramsResult } from 'src/app/models/results/programs-result.model';
import { BannerAlertComponent } from 'src/app/components/shared/banner-alert/banner-alert.component';
import { SharedService } from 'src/app/services/shared.service';
import { ToolTip } from 'src/app/models/tool-tip.model';
import { MessageType } from 'src/app/enums/message-type.enum';
import { saveAs } from 'file-saver';
import { LicenseService } from 'src/app/services/license-api.service';
import { WeSignLicense } from 'src/app/models/wesign-license';
import { NgxSpinnerService } from 'ngx-spinner';
import { iif } from 'rxjs';


@Component({
  selector: 'app-company-form',
  templateUrl: './company-form.component.html',
  styleUrls: ['./company-form.component.css']
})
export class CompanyFormComponent implements OnInit {

  public programsOptions: {};
  public groupsOptions: {};
  public smsProvidersOptions = { 1: "Goldman", 2: "Twilio", 3: "InforUMobile (Shamir)", 4: "ClickSend", 5: "PayCall", 6: "Notify", 7: "Telemessage", 8: "Micropay", 9: "Notify G2", 12: "Center" };
  public portActiveDirectoryOptions = { 636: "Secured", 389: "Not Secured" };
  public languagesOptions = { 1: "English", 2: "עברית" };
  public signtureType = { 1: "Graphic", 2: "Smart Card", 3: "Server" };
  public newGroup: string = "";

  @ViewChild("emailBeforeHtmlTemplate") beforeEl: ElementRef;
  @ViewChild("emailAfterHtmlTemplate") afterEl: ElementRef;
  @ViewChild("companyLogo") logoEl: ElementRef;

  @Output() public removeCompanyForm = new EventEmitter<number>();

  public programsResult: ProgramsResult;
  public file: any = null;

  public submited: boolean = false;
  public isBusy: boolean = false;
  public isError: boolean = false;
  public showUpdateAlert: boolean = false;
  public errorMessage: string = "Error - Please correct the marked fields";
  readonly unlimited = -1;
  public toolTipInfo: ToolTip = new ToolTip();
  public isUnlimitedDeleteSignedDocuments: boolean = false;
  public isUnlimitedDeleteUnsignedDocuments: boolean = false;
  public isShownOptionalSetting: boolean = false;
  public isShownOptionalEmailSetting: boolean = false;
  public isShownOptionalSmsSetting: boolean = false;
  public isShownNotifications: boolean = false;
  public isShownOptionalDeleteSettings: boolean = false;
  public isShownActiveDirectorySettings: boolean = false;
  public isSigner1Details: boolean = false;
  public isFormShown: boolean = false;

  public isADSettingActiveInLicense: boolean = false;;
  showDeleteMappingAllert: Boolean = false;

  @Input() public operation: Operation;
  @Input() public company: Company = new Company();
  @Output() bannerAlertEvent = new EventEmitter<string>();



  constructor(private companiesApiService: CompaniesApiService,
    private programsApiService: ProgramsApiService,
    private sharedService: SharedService,
    private licenseApiService: LicenseService,
    private spinner: NgxSpinnerService) {

  }

  ngOnInit(): void {
    let programFilter = new ProgramFilter();
    programFilter.limit = this.unlimited;
    this.programsApiService.Read(programFilter).subscribe(
      (data) => {
        this.programsResult = data.body;
        this.programsOptions = this.programsResult.programs
          .filter(x => x.id != '00000000-0000-0000-0000-000000000001')
          .reduce((prev, curr) => {
            prev[curr.id] = curr.name;
            return prev;
          }, {});

      },
      (err) => {
        let result = new Errors(err.error);
      }
    );



    this.licenseApiService.readSimpleInfo().subscribe(
      (data) => {
        this.isADSettingActiveInLicense = data.licenseLimits.licenseCounters.useActiveDirectory;

      },
      (err) => {
        let result = new Errors(err.error);
      }
    )

    this.sharedService.getToolTipsInfo().subscribe(
      (data) => {
        this.toolTipInfo.logo = (data as ToolTip).logo;
        this.toolTipInfo.messageBefore = (data as ToolTip).messageBefore;
        this.toolTipInfo.messageBeforeHebrew = (data as ToolTip).messageBeforeHebrew;
        this.toolTipInfo.messageAfter = (data as ToolTip).messageAfter;
        this.toolTipInfo.messageAfterHebrew = (data as ToolTip).messageAfterHebrew;
        this.toolTipInfo.htmlTemplateBeforeSigning = (data as ToolTip).htmlTemplateBeforeSigning;
        this.toolTipInfo.htmlTemplateAfterSigning = (data as ToolTip).htmlTemplateAfterSigning;
        this.toolTipInfo.defaultSMTPSSL = (data as ToolTip).defaultSMTPSSL;
        this.toolTipInfo.defaultSMTPPort = (data as ToolTip).defaultSMTPPort;
        this.toolTipInfo.shouldSendWithOTPByDefault = (data as ToolTip).shouldSendWithOTPByDefault;
        this.toolTipInfo.enablePersonalizedPFX = (data as ToolTip).enablePersonalizedPFX;
        this.toolTipInfo.enableVisualIdentityFlow = (data as ToolTip).enableVisualIdentityFlow;
        this.toolTipInfo.enableDisplaySignerNameInSignature = (data as ToolTip).enableDisplaySignerNameInSignature;
        this.toolTipInfo.signer1Endpoint = (data as ToolTip).signer1Endpoint;
        this.toolTipInfo.signer1User = (data as ToolTip).signer1User;
        this.toolTipInfo.signer1Password = (data as ToolTip).signer1Password;
        this.toolTipInfo.enableReminders = (data as ToolTip).enableReminders;
        this.toolTipInfo.disableUserReminderControl = (data as ToolTip).disableUserReminderControl;
        this.toolTipInfo.signReminderFrequency = (data as ToolTip).signReminderFrequency;
        this.toolTipInfo.enableDocumentNotifications = (data as ToolTip).enableDocumentNotifications;
        this.toolTipInfo.documentNotificationsEndpoint = (data as ToolTip).documentNotificationsEndpoint;
        this.toolTipInfo.shouldEnableMeaningOfSignatureOption = (data as ToolTip).shouldEnableMeaningOfSignatureOption;
        this.toolTipInfo.shouldForceOTPInLogin = (data as ToolTip).shouldForceOTPInLogin;
        this.toolTipInfo.shouldAddAppendicesAttachmentsToSendMail = (data as ToolTip).shouldAddAppendicesAttachmentsToSendMail;
        this.toolTipInfo.shouldEnableVideoConference = (data as ToolTip).shouldEnableVideoConference;
        this.toolTipInfo.enableTabletsSupport = (data as ToolTip).enableTabletsSupport;
      }
    )
  }

  public deleteGroupMap(groupMapper) {
    this.company.groupsADMapper = this.company.groupsADMapper.filter(x => x.groupName != groupMapper.groupName);
  }

  public onShownOptionalDeleteSettings() {
    this.isShownOptionalDeleteSettings = !this.isShownOptionalDeleteSettings;
    this.companiesApiService.ReadCompanyDeletionConfiguration(this.company.id).subscribe(
      (data) => {
        var deletion = data.body;
        if (deletion.deleteSignedDocumentAfterXDays == -1) {
          this.isUnlimitedDeleteSignedDocuments = true;
        }
        if (deletion.deleteUnsignedDocumentAfterXDays == -1) {
          this.isUnlimitedDeleteUnsignedDocuments = true;
        }
      }, (err) => {
        let result = new Errors(err.error);
      });
  }

  public update() {

    this.submited = true;

    this.company.deletionDetails.deleteSignedDocumentAfterXDays = this.isUnlimitedDeleteSignedDocuments ? -1 :
      this.company.deletionDetails.deleteSignedDocumentAfterXDays;
    this.company.deletionDetails.deleteUnsignedDocumentAfterXDays = this.isUnlimitedDeleteUnsignedDocuments ? -1 :
      this.company.deletionDetails.deleteUnsignedDocumentAfterXDays;

    if (this.operation == Operation.Create) {
      this.company.user.groupName = this.newGroup;
      this.createCompany();
    }

    if (this.operation == Operation.Update) {
      this.showUpdateAlert = true;
    }

  }

  public hideAlert() {
    this.showUpdateAlert = false;
    this.isBusy = false;
    this.spinner.hide();
  }


  public updateCompany() {


    if (!this.isBusy) {
      this.showUpdateAlert = false;
      this.isBusy = true;
      this.spinner.show();
      this.companiesApiService.updateCompany(this.company).subscribe(
        () => {
          this.hideTheForm();
          this.bannerAlertEvent.emit('eventDesc');
          this.isError = false;
          this.company = new Company();
          this.isBusy = false;
          this.spinner.hide();
        }, (err) => {
          this.isError = true;
          this.errorMessage = this.sharedService.getErrorMessage(new Errors(err.error));
          this.hideAlert();
        },
        () => {
          this.hideAlert();
        });
    }
  }

  private createCompany() {

    if (!this.isBusy) {
      this.isBusy = true;
      this.spinner.show();
      this.companiesApiService.createCompany(this.company).subscribe(
        () => {

          this.hideTheForm();
          this.bannerAlertEvent.emit('eventDesc');
          this.isError = false;
          this.company = new Company();
        }, (err) => {
          this.isError = true;
          this.errorMessage = this.sharedService.getErrorMessage(new Errors(err.error));
          this.isBusy = false;
          this.spinner.hide();
        },
        () => {
          this.isBusy = false;
          this.spinner.hide();
        }
      );
    }
  }

  public cancel() {
    this.company = new Company();
    this.hideTheForm();
  }

  public dropDownUpdate(newValue, company: Company, property: string) {
    if (newValue && property == 'language' && newValue !== company.language) {
      company.language = newValue;
    }
    if (newValue && property == 'program') {
      company.programId = newValue;
    }
    if (newValue && property == 'smsProvider') {
      company.smsConfiguration.provider = newValue;
    }
    if (newValue && property == 'groups') {
      let x1 = this.company.groups.find(x => x["0"] == newValue);
      let x2 = this.company.groups.find(x => x["item1"] == newValue);

      company.user.groupName = x1 ? x1["1"] : x2 ? x2["item2"] : "";
    }
    if (newValue && property == 'signtureType') {
      company.defaultSigningType = newValue;
    }

  }

  public hideTheForm() {
    this.isBusy = false;
    this.clearChildsUI();
    this.removeCompanyForm.emit()
    window.scroll(0, 0);
  }

  public fileDropped(messageType: MessageType) {
    if (this.beforeEl.nativeElement.files.length > 0 || this.afterEl.nativeElement.files.length > 0) {
      this.file = this.beforeEl.nativeElement.files.length > 0 && messageType == MessageType.Before ? this.beforeEl.nativeElement.files[0] :
        this.afterEl.nativeElement.files.length > 0 && messageType == MessageType.After ? this.afterEl.nativeElement.files[0] : "";
      if (this.file.type == "text/html") {
        const reader = new FileReader();
        reader.readAsDataURL(this.file);
        reader.onload = () => {
          if (messageType == MessageType.Before) {
            this.company.smtpConfiguration.beforeSigningHtmlTemplateBase64String = reader.result.toString();
          }
          if (messageType == MessageType.After) {
            this.company.smtpConfiguration.afterSigningHtmlTemplateBase64String = reader.result.toString();
          }
        };
      }
    } else {
      this.file = null;
    }
  }

  public logoDropped() {
    if (this.logoEl.nativeElement.files.length > 0) {
      this.file = this.logoEl.nativeElement.files[0];
      if (String(this.file.type).startsWith("image/")) {
        const reader = new FileReader();
        reader.readAsDataURL(this.file);
        reader.onload = () => {
          this.company.logoBase64String = reader.result.toString();
        };
      }
    } else {
      this.file = null;
    }
  }
  public GetGroupsSelection(array: [string, string][]) {

    let groups = this.CollectionToOptions(array);
    return Object.keys(groups).find(key => groups[key] === this.company.user.groupName);


  }

  public CollectionToOptions(array: [string, string][]) {
    let options = array.reduce((prev, curr) => {
      if (curr["item1"]) {
        prev[curr["item1"]] = curr["item2"];
      }
      if (curr["0"]) {
        prev[curr["0"]] = curr["1"];
      }
      return prev;
    }, {});

    return options;

  }



  public removeLogo() {
    this.logoEl.nativeElement.value = "";
    this.company.logoBase64String = "";
  }

  public downloadLogo() {
    if (this.company.logoBase64String != null && this.company.logoBase64String != '') {
      fetch(this.company.logoBase64String)
        .then(res => res.blob())
        .then((blob) => {
          const fileName = 'logo.png';
          saveAs(blob, fileName);
        }
        );
    }
  }

  public removeEmailTemplate(messageType: MessageType) {
    if (this.company.smtpConfiguration != null) {
      if (messageType == MessageType.Before) {
        this.company.smtpConfiguration.beforeSigningHtmlTemplateBase64String = "";
        this.beforeEl.nativeElement.value = "";
      }
      if (messageType == MessageType.After) {
        this.company.smtpConfiguration.afterSigningHtmlTemplateBase64String = "";
        this.afterEl.nativeElement.value = "";
      }
    }
  }

  public downloadHtml(messageType: MessageType) {
    if (messageType == MessageType.Before &&
      this.company.smtpConfiguration.beforeSigningHtmlTemplateBase64String != null &&
      this.company.smtpConfiguration.beforeSigningHtmlTemplateBase64String != '') {
      fetch(this.company.smtpConfiguration.beforeSigningHtmlTemplateBase64String)
        .then(res => res.blob())
        .then((blob) => {
          const fileName = 'beforeSigning.html';
          saveAs(blob, fileName);
        }
        );
    }
    if (messageType == MessageType.After &&
      this.company.smtpConfiguration.afterSigningHtmlTemplateBase64String != null &&
      this.company.smtpConfiguration.afterSigningHtmlTemplateBase64String != '') {
      fetch(this.company.smtpConfiguration.afterSigningHtmlTemplateBase64String)
        .then(res => res.blob())
        .then((blob) => {
          const fileName = 'afterSigning.html';
          saveAs(blob, fileName);
        }
        );
    }
  }

  hideGroupForm($event) {
    this.isFormShown = false;

  }
  createGroupMapCompany() {
    if (this.company.groups.length == 0) {

      return;
    }
    window.scroll(0, 0);
    this.isFormShown = true;

  }

  clearChildsUI() {
    if (this.beforeEl != undefined) {
      this.beforeEl.nativeElement.value = "";
      this.afterEl.nativeElement.value = "";
    }
    if (this.logoEl != undefined) {
      this.logoEl.nativeElement.value = "";
    }
  }
  EnableRemindersChecked() {
    if (!this.company.notifications.shouldEnableSignReminders) {
      this.company.notifications.canUserControlReminderSettings = false;
      this.company.notifications.signReminderFrequencyInDays = 0;
    }
  }

  ShouldSendDocumentNotificationsChanged() {
    if (!this.company.notifications.shouldSendDocumentNotifications) {
      this.company.notifications.documentNotificationsEndpoint = ""
    }
  }

  TakeUserControlCheck() {
    if (!this.company.notifications.canUserControlReminderSettings) {
      this.company.notifications.signReminderFrequencyInDays = 1;
    }
    else {
      this.company.notifications.signReminderFrequencyInDays = 0;
    }
  }

  get invertedReminderSetting(): boolean {
    return !this.company.notifications.canUserControlReminderSettings;
  }

  set invertedReminderSetting(value: boolean) {
    this.company.notifications.canUserControlReminderSettings = !value;
  }
}
