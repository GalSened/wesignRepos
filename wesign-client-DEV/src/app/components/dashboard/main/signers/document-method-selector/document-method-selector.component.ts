import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, EventEmitter, Input, OnInit, Output, Renderer2, ViewChild } from '@angular/core';
import { Signer } from '@models/document-api/signer.model';
import { SignMode } from '@models/enums/sign-mode.enum';
import { StateProcessService } from '@services/state-process.service';
import { Observable } from 'rxjs';
import * as documentActions from "@state/actions/document.actions";
import { AlertLevel, IAppState, AppState } from '@state/app-state.interface';
import { Store } from '@ngrx/store';
import { Contact, OtpContact, SendingMethod } from '@models/contacts/contact.model';
import { Router } from '@angular/router';
import { Tablet } from '@models/configuration/tablets-configuration.model';
import { SharedService } from '@services/shared.service';
import { NgForm } from '@angular/forms';
import { GroupAssignService } from '@services/group-assign.service';
import { ConfigurationApiService } from '@services/configuration-api.service';
import * as actions from "@state/actions/app.actions";
import { UiViewLicense, UserProgram } from '@models/program/user-program.model';
import { CompanySigner1Details } from '@models/account/user.model';
import * as appActions from "@state/actions/app.actions";
import { UploadRequest } from '@models/template-api/upload-request.model';
import { DistributionApiService } from '@services/distribution-api.service';
import { Errors } from '@models/error/errors.model';
import { BaseSigner } from '@models/distribution-api/read-signers-from-file-response.model';
import { CreateDistributionDocuments } from '@models/distribution-api/create-distribution-documents.model';
import { ContactsOriginMode } from '@models/enums/contacts-origin-mode.enum';
import validator from 'validator';
import { TranslateService } from '@ngx-translate/core';
import { CountriesPhoneData } from '@services/countries-phone-data.service';
import { ContactsGroup } from '@models/contacts/contacts-groups-result.model';

@Component({
  selector: 'sgn-document-method-selector',
  templateUrl: './document-method-selector.component.html',
})

export class DocumentMethodSelectorComponent implements OnInit, AfterViewInit {

  public selectedSignMode: SignMode = SignMode.SelfSign;
  public signModeEnum = SignMode;
  public state$: Observable<any>;
  public showContacts = false;
  public showContactsGroup = false;
  public showMultiContacts = false;
  public showTablets = false;
  private signerRef: HTMLElement;
  private selectedSigner: Signer;
  public shouldShowSigner1SigningLockOption: boolean = false;
  public isSigner1DistributionChecked: boolean = false;

  @ViewChild("wfForm") public wfForm: NgForm;
  @Output() public signAct = new EventEmitter<SignMode>();
  isAppSupportTablets: boolean;
  public signers: Signer[] = [];
  @Input() public isBusy: boolean;
  @Input() public documentName: string;


  public userProgram: UserProgram;
  public templatesCount: number;
  public companySigner1Details: CompanySigner1Details;

  public busy: boolean = false;
  public file: any = null;
  public acceptTypes: string = ".xlsx,.xls";
  public reader = new FileReader();
  public filename: string;
  @ViewChild("fileInput") fileInput: ElementRef;
  contactsFromExcel: OtpContact[] = [];
  public showExcelFileUpload: boolean = false;
  appState: AppState;
  contactsOriginMode: ContactsOriginMode = ContactsOriginMode.FromSystem;
  baseSignersFromExcel: BaseSigner[];
  phoneExt: string = "+972";
  useEmail: boolean = true;
  emailPhoneData: string;
  public telOptions = { initialCountry: 'il' };
  uiViewLicense: UiViewLicense;
  public enableMeaningOfSignature: boolean = false;
  public useMeaningOfSignature: boolean = false;;
  count: number;
  contacts: Contact[];
  @ViewChild('signatureGroup') groupSignatureButtonEl: ElementRef;
  @ViewChild('signatureSelfSign') selfSignSignatureButtonEl: ElementRef;
  @ViewChild('signersContainer') signersContainerEl: ElementRef;
  @Input() multidocs: boolean;

  constructor(
    private renderer: Renderer2,
    private router: Router,
    private stateService: StateProcessService,
    private store: Store<IAppState>,
    private sharedService: SharedService,
    private translate: TranslateService,
    private countriesPhoneData: CountriesPhoneData,
    private cdr: ChangeDetectorRef,
    private assignService: GroupAssignService,
    private configurationApiService: ConfigurationApiService,
    private distributionApiService: DistributionApiService
  ) { }

  ngAfterViewInit(): void {
    this.useMeaningOfSignature = this.assignService.useMeaningOfSignature;
    let contactsArray = sessionStorage.getItem("CONTACTS")
    if (contactsArray != null) {
      this.contacts = JSON.parse(contactsArray);
      sessionStorage.removeItem("CONTACTS");
      let signersArray = sessionStorage.getItem("SIGNERS")
      if (signersArray != null) {
        this.signers = [];
        this.signers = JSON.parse(signersArray);
        sessionStorage.removeItem("SIGNERS");
      }

      if (this.contacts.length > 0) {
        this.changeToOthersSignMode(this.groupSignatureButtonEl.nativeElement, this.selfSignSignatureButtonEl.nativeElement);
        if (this.signers.length == 0) {
          for (let index = 0; index < this.contacts.length; index++) {
            this.updateSigner(this.contacts[index], true, index);
          }
        }
      }
    }
  }
  //this.store.dispatch(new documentActions.SetSigners({ Signers: [...this.appState.Signers] }));

  ngOnInit() {
    this.contacts = [];
    this.state$ = this.stateService.getState();
    if (!this.signers || this.signers.length < 1) {
      this.signers = [this.addEmptySigner(1)];
    }

    this.configurationApiService.readInitConfiguration().subscribe(
      (data) => {


        if (data.shouldUseSignerAuth) {
          this.store.dispatch(new actions.SignerAuthAction());
        }
        if (data.shouldUseSignerAuthDefault) {
          this.store.dispatch(new actions.SignerAuthDefaultAction());
        }
      },
      (error) => {

      }
    );
    this.state$.subscribe((x: AppState) => {
      this.isAppSupportTablets = x.EnableTabletSupport;
      this.userProgram = x.program;
      this.templatesCount = x.SelectedTemplates.length;
      this.companySigner1Details = x.companySigner1Details;
      if (this.companySigner1Details.shouldSignAsDefaultValue == true) {
        this.isSigner1DistributionChecked = true;
      }
      if (this.companySigner1Details.shouldSignAsDefaultValue == false) {
        this.isSigner1DistributionChecked = false;
      }
      this.shouldShowSigner1SigningLockOption = this.companySigner1Details.shouldShowInUI;
      this.appState = x;
      if (x.program) {
        this.uiViewLicense = x.program.uiViewLicense;
      }
      this.enableMeaningOfSignature = x.enableMeaningOfSignature;
    });
    if (this.multidocs) {
      this.selectedSignMode = SignMode.OrderedWorkflow;
    }
  }

  public onSigner1DistributtonChange() {
    this.isSigner1DistributionChecked = !this.isSigner1DistributionChecked;
  }

  private changeToOthersSignMode(target: ElementRef, sibling: ElementRef) {
    this.selectedSignMode = SignMode.Workflow;

    this.renderer.removeClass(sibling, "is-active");
    this.renderer.addClass(target, "is-active");

    this.isSigner1DistributionChecked = false;
  }

  public change($event, mode: SignMode) {
    this.selectedSignMode = mode;
    let siblings = [];
    let sib = $event.target.parentElement.firstChild;
    let a = $event.target;
    while (sib) {
      if (sib.nodeType == 1 && sib !== $event.target)
        siblings.push(sib);

      sib = sib.nextSibling;
    }

    siblings.forEach(s => this.renderer.removeClass(s, "is-active"));
    this.renderer.addClass($event.target, "is-active");

    if (this.selectedSignMode == SignMode.Distribution) {
      if (this.companySigner1Details.shouldSignAsDefaultValue) {
        this.isSigner1DistributionChecked = true;
      } else {
        this.isSigner1DistributionChecked = false;
      }

    }

    if (this.selectedSignMode == SignMode.Distribution && this.templatesCount > 1) {
      this.sharedService.setTranslateAlert("SIGNERS.SELFSIGN_LIMITION", AlertLevel.ERROR);
    }
  }

  public sign() {
    this.isBusy = true;
    let valid = false;

    if (!this.documentName) {
      this.sharedService.setTranslateAlert("DOCUMENT.NAME_EMPTY", AlertLevel.ERROR);
      this.isBusy = false;
      return;
    }
    if (this.userProgram.smsLimit != -1 && this.signers.filter(x => x.DeliveryMethod == SendingMethod.SMS).length > this.userProgram.remainingSMS) {
      this.sharedService.setTranslateAlert("GLOBAL.INSUFFICIENT_SMS", AlertLevel.ERROR);
      this.isBusy = false;
      return;
    }
    if (this.selectedSignMode == SignMode.SelfSign) {
      if (this.templatesCount > 1) {
        this.sharedService.setTranslateAlert("SIGNERS.SELFSIGN_LIMITION", AlertLevel.ERROR);
        this.isBusy = false;
        return;
      }
      valid = true;
    }
    else {
      if (!this.wfForm.valid) {
        this.sharedService.setTranslateAlert("TINY.ILLEGAL_RECIPIENT", AlertLevel.ERROR);
        this.isBusy = false;
        window.scroll(0, 0);
        return;
      }

      let errorsCount = 0;
      this.signers.forEach(signer => {
        if ((signer.DeliveryMethod != SendingMethod.TABLET && (!signer.FullName || !signer.DeliveryMeans)) ||
          (signer.DeliveryMethod == SendingMethod.TABLET && !signer.FullName)) {
          errorsCount++;
        }
        // if(signer.DeliveryMethod == SendingMethod.SMS){
        //   signer.DeliveryMeans = signer.DeliveryMeans.startsWith("+") ? signer.DeliveryMeans: `+${signer.DeliveryExtention}${signer.DeliveryMeans}`;
        // }

        // TODO add regex validation? 
      });

      if (errorsCount == 0) {
        valid = true;
        let contacts_json = JSON.stringify(this.contacts);
        let signers_json = JSON.stringify(this.signers);
        sessionStorage.setItem("CONTACTS", contacts_json)
        sessionStorage.setItem("SIGNERS", signers_json)
        this.store.dispatch(new documentActions.SetSigners({ Signers: [...this.signers] }));
      }
      if (this.selectedSignMode == SignMode.Online && this.companySigner1Details) {
        this.store.dispatch(new appActions.SetShouldSignUsingSigner1AfterDocumentSigningFlow({ shouldSignUsingSigner1AfterDocumentSigningFlow: this.companySigner1Details.shouldSignAsDefaultValue }));
      }
    }

    if (valid)
      this.signAct.emit(this.selectedSignMode);
  }

  public openContactsBook($event, signer: Signer) {
    this.selectedSigner = signer;
    this.signerRef = $event.target.closest("button").parentElement;
    this.showContacts = true;
  }

  public openShowMultiContactsBook($event) {
    this.signerRef = $event.target.closest("button").parentElement;
    this.showMultiContacts = true;
    this.contactsOriginMode = ContactsOriginMode.FromSystem;
  }

  public openExcelFileUpload() {
    this.showExcelFileUpload = !this.showExcelFileUpload;
    setTimeout(() => {
      //scroll to bottom of page
      window.scrollTo(0, document.body.scrollHeight);
    }, 300);
  }

  public openTablets($event) {
    this.signerRef = $event.target.closest("button").parentElement;
    this.showTablets = true;
  }

  public SetSelectedContactGroup(contactsGroup: ContactsGroup) {
    this.showContactsGroup = false;
    if (contactsGroup) {

      contactsGroup?.contactsGroupMembers?.forEach(selectedSigner => {


        let lastUsedSigner = this.signers.findIndex(x => !x.ContactId);
        let signer = null;
        if (lastUsedSigner == -1) {
          signer = this.signers[this.signers.length - 1];
        }
        else {
          signer = this.signers[lastUsedSigner];
        }

        if (signer.ContactId) {
          signer = this.addSigner();
          if (signer == null) {
            return;
          }
        }
        signer.FullName = selectedSigner.contact.name;
        signer.DeliveryMeans = selectedSigner.contact.defaultSendingMethod == SendingMethod.EMAIL ? selectedSigner.contact.email : selectedSigner.contact.phone;
        signer.DeliveryMethod = selectedSigner.contact.defaultSendingMethod;
        signer.DeliveryExtention = selectedSigner.contact.phoneExtension;
        signer.ContactId = selectedSigner.contact.id;
        this.contacts.push(selectedSigner.contact);
      });
    }
  }

  public updateSigner(contact: Contact, isFromSession: boolean, index: number) {
    this.showContacts = false;
    if (contact) {
      if (this.signerRef && !isFromSession) {
        if (this.selectedSigner.ContactId != null) {
          this.contacts = this.contacts.filter(c => c.id !== this.selectedSigner.ContactId);
        }
        this.contacts.push(contact);
        const id = this.signerRef.dataset.signerid;
        let si = this.signers.find(s => s.ClassId == id);
        if (si) {
          si.FullName = contact.name;
          si.DeliveryMeans = contact.defaultSendingMethod == SendingMethod.EMAIL ? contact.email : contact.phone;
          si.DeliveryMethod = contact.defaultSendingMethod;
          si.DeliveryExtention = contact.phoneExtension;
          si.ContactId = contact.id;
          this.useEmail = contact.defaultSendingMethod == SendingMethod.EMAIL;
        }
        let iso2 = this.countriesPhoneData.getIso2CodeByDialCode(contact.phoneExtension.replace("+", ""), contact.phone);

        this.telOptions = { initialCountry: <string>iso2 };
      }
      else if (isFromSession) {
        let si = this.signers[index];
        if (si) {
          si.FullName = contact.name;
          si.DeliveryMeans = contact.defaultSendingMethod == SendingMethod.EMAIL ? contact.email : contact.phone;
          si.DeliveryMethod = contact.defaultSendingMethod;
          si.DeliveryExtention = contact.phoneExtension;
          si.ContactId = contact.id;
          this.useEmail = contact.defaultSendingMethod == SendingMethod.EMAIL;
        }
        let iso2 = this.countriesPhoneData.getIso2CodeByDialCode(contact.phoneExtension.replace("+", ""), contact.phone);

        this.telOptions = { initialCountry: <string>iso2 };
      }
    }
  }

  public updateSignerAsTablet(tablet: Tablet) {
    this.showTablets = false;
    if (tablet && this.signerRef) {
      const id = this.signerRef.dataset.signerid;
      let si = this.signers.find(s => s.ClassId == id);
      if (si) {
        si.DeliveryMeans = "";
        si.FullName = tablet.name;
        si.DeliveryMethod = SendingMethod.TABLET;
      }
    }
  }

  public addSigner() {
    if (this.signers.length < 25) {
      var lastSigner = this.addEmptySigner(this.signers.length + 1)
      this.signers.push(lastSigner);
      return lastSigner;
    }
    return null;
  }

  public removeSigner(index) {
    this.contacts.splice(index, 1);
    this.signers.splice(index, 1);
  }

  public moveSigner(index, direction) {
    let deleted = this.signers.splice(index, 1);
    let contactDeleted = this.contacts.splice(index, 1);
    this.signers.splice(index + direction, 0, deleted.shift());
    this.contacts.splice(index + direction, 0, contactDeleted.shift());
  }

  public workFlowChange($event) {
    this.assignService.isOrderedWorkflow = !$event.target.checked;
  }

  public MeaningOfSignatureChange($event) {
    this.useMeaningOfSignature = $event.target.checked;
    this.assignService.useMeaningOfSignature = $event.target.checked;
  }

  private addEmptySigner(index: number): Signer {
    let nextIndex = this.getSignersIndexs();
    let signer = new Signer();
    signer.DeliveryMeans = "";
    signer.FullName = "";
    signer.ClassId = "ct-is-signer" + nextIndex;


    return signer;
  }

  getSignersIndexs() {
    let result = [];
    this.signers.forEach(element => {
      let index = element.ClassId.substring(12);
      result.push(parseInt(index));
    });

    let sortList = result.sort(function (a, b) {
      return a - b;
    });

    let res = sortList.length == 0 ? 1 : sortList[sortList.length - 1] + 1;
    for (let index = 0; index < sortList.length; index++) {
      if (index + 1 < sortList.length) {
        if (sortList[index + 1] - sortList[index] > 1) {
          res = sortList[index] + 1;
        }
      }
    }
    if (res > 25) {
      res = sortList[0] - 1;
    }
    return res;
  }

  public async distributionFileDropped(file) {
    this.contactsFromExcel = [];
    this.filename = file.name;
    this.reader.readAsDataURL(file);
    this.sharedService.setBusy(true, "DOCUMENT.LOADING");

    this.reader.onload = () => {
      const uploadRequest = new UploadRequest();
      uploadRequest.Base64File = this.reader.result.toString();
      this.distributionApiService.readSignersFromFile(uploadRequest)
        .subscribe(
          data => {
            window.scroll(0, 0);
            this.isBusy = false;
            this.showExcelFileUpload = false;
            this.sharedService.setBusy(false)
            if (data.signers.length == 0) {
              this.sharedService.setErrorAlert(this.translate.instant("ERROR.INPUT.EMPTY_LIST"));
              return;
            }
            this.sharedService.setSuccessAlert('ERROR.OPERATION.1');
            data.signers.forEach(baseSigner => {
              let email = baseSigner.signerMeans && validator.isEmail(baseSigner.signerMeans) ? baseSigner.signerMeans : baseSigner.signerSecondaryMeans && validator.isEmail(baseSigner.signerSecondaryMeans) ? baseSigner.signerSecondaryMeans : "";
              let phone = baseSigner.signerMeans && validator.isMobilePhone(baseSigner.signerMeans) ? baseSigner.signerMeans : baseSigner.signerSecondaryMeans && validator.isMobilePhone(baseSigner.signerSecondaryMeans) ? baseSigner.signerSecondaryMeans : "";
              let contact = new OtpContact(null);
              contact.name = baseSigner.fullName;
              contact.email = email;
              contact.phone = phone;
              contact.defaultSendingMethod = validator.isEmail(baseSigner.signerMeans) ? SendingMethod.EMAIL : SendingMethod.SMS;
              contact.id = this.sharedService.generateNewGuid();
              contact.shouldSendOTP = baseSigner.shouldSendOTP;
              contact.phoneExtension = baseSigner.phoneExtension;
              this.contactsFromExcel.push(contact);
            });
            this.baseSignersFromExcel = data.signers;

            this.contactsOriginMode = ContactsOriginMode.FromFile;
            this.showMultiContacts = true;

          }
          ,
          err => {
            window.scroll(0, 0);
            let result = new Errors(err.error);
            //this.sharedService.setErrorAlert(result);
            this.sharedService.setErrorAlert(result.errors.errors['Base64File'][0]);
            this.isBusy = false;
            this.sharedService.setBusy(false)
          });
    };
    this.isBusy = false;
  }

  public selectContactsGroup() {
    this.showContactsGroup = true;
  }
  public distributionSending(contacts: OtpContact[]) {
    this.isBusy = true;
    this.showMultiContacts = false;
    window.scroll(0, 0);
    this.sharedService.setBusy(true, "DOCUMENT.SENDING");
    let request = this.GetRequestForDistributionCreateAPI(contacts, this.isSigner1DistributionChecked);
    setTimeout(() => {
      if (this.isBusy) {
        this.router.navigate(['/dashboard', 'documents', 'distribution'])
        this.sharedService.setSuccessAlert('GLOBAL.YOUR_REQUEST_HAS_BEEN_SENT');
        this.sharedService.setBusy(false);
        this.isBusy = false;
      }
    }, 10000);
    this.distributionApiService.distributionMechanism(request)
      .subscribe(
        data => {
          this.sharedService.setBusy(false)
          this.isBusy = false;
          this.router.navigate(['/dashboard', 'documents', 'distribution'])
          this.sharedService.setSuccessAlert('GLOBAL.SUCCESS_DISTRIBUTION_REQUEST', false);
        }
        ,
        err => {
          this.isBusy = false;
          this.sharedService.setBusy(false)
          if (err.status == 400) {
            let ex = new Errors(err.error);
            if (ex.errorCode == 86 || ex.errorCode == 87 || ex.errorCode == 20 || ex.errorCode == 10) {
              let ext = ex.errors.errors.error[1] == undefined ? "" : ex.errors.errors.error[1];
              let msg = `${this.translate.instant(`SERVER_ERROR.${ex.errorCode}`)} ${ext}`;
              setTimeout(() => {
                this.sharedService.setErrorAlert(msg, true);

              }, 1000);

            }
          }
          else {
            this.sharedService.setTranslateAlert("GLOBAL.FAILED_DISTRIBUTION_REQUEST", AlertLevel.ERROR, false);
          }
        });
  }

  private GetRequestForDistributionCreateAPI(contacts: OtpContact[], isSigner1DistributionChecked: boolean) {
    let request = new CreateDistributionDocuments();
    request.name = this.appState.CurrentDocumentName;
    request.templateId = this.appState.SelectedTemplates[0].templateId;
    request.signDocumentWithServerSigning = isSigner1DistributionChecked;

    contacts.forEach(contact => {
      let bs = new BaseSigner(contact);
      if (this.baseSignersFromExcel) {
        let baseSigner = this.baseSignersFromExcel
          .find(x => x.fullName == contact.name &&
            (x.signerMeans == contact.phone || x.signerMeans == contact.email));
        if (baseSigner) {
          bs.fields = baseSigner.fields;
        }
      }
      request.signers.push(bs);
    });
    return request;
  }

  onCountryChange(obj, signer: Signer) {
    this.phoneExt = obj.dialCode;
    let si = this.signers.find(s => s.ClassId == signer.ClassId);
    si.DeliveryExtention = `+${this.phoneExt}`;
  }

  public onSignerMethodChanged(event, signer: Signer) {
    this.useEmail = event.target.selectedIndex == 0;
    // this.emailPhoneData = "";
    let si = this.signers.find(s => s.ClassId == signer.ClassId);
    si.DeliveryMethod = this.useEmail ? SendingMethod.EMAIL : SendingMethod.SMS;
    let signerIndex = this.signers.findIndex(s => s.ClassId == signer.ClassId);
    if (signerIndex !== -1) {
      if (signer.DeliveryMethod === SendingMethod.EMAIL) {
        this.signers[signerIndex].DeliveryMeans = this.contacts[signerIndex].email;
      }
      else {
        this.signers[signerIndex].DeliveryMeans = this.contacts[signerIndex].phone;
      }
    }
  }
}
