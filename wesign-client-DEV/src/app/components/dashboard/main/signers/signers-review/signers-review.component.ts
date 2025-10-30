import { Component, OnInit } from '@angular/core';
import { Appendix, DocumentCollectionCreateRequest, DocumentSigner, SharedAppendix, SignerField } from '@models/document-api/document-create-request.model';
import { SignMode } from '@models/enums/sign-mode.enum';
import { Errors } from '@models/error/errors.model';
import { DocumentApiService } from '@services/document-api.service';
import { GroupAssignService } from '@services/group-assign.service';
import { SharedService } from '@services/shared.service';
import { StateProcessService } from '@services/state-process.service';
import { AppState } from '@state/app-state.interface';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { Location } from '@angular/common';
import { Router } from '@angular/router';
import { PageField, SignatureField, TextField } from '@models/template-api/page-data-result.model';
import { SendingMethod } from '@models/contacts/contact.model';
import validator from "validator";
import { SelectDistinctContactVideoConfrence } from '@models/contacts/select-distinct-contact-video-confrence.model';
import { DocumentCreateResult } from '@models/document-api/document-create-result.model';
import { OtpMode } from '@models/enums/otp-mode.enum';
import { AuthMode } from '@models/enums/auth-mode.enum';
import { Store } from '@ngrx/store';
import * as documentActions from '@state/actions/document.actions'; 


@Component({
  selector: 'sgn-signers-review',
  templateUrl: './signers-review.component.html',
  styles: []
})
export class SignersReviewComponent implements OnInit {

  state$: Observable<any>;
  appState: AppState;
  documentName = "document.pdf";
  documentMode: SignMode;
  isBusy: boolean;
  showOtp: boolean;
  showVisualIdentificationPopUp: boolean;
  showVideoConferencePopUp: boolean;
  showAppendices: boolean;
  showAttachments: boolean;
  showPersonalNote: boolean;
  request: DocumentCollectionCreateRequest;
  currSignerIndex: any;
  listenTocloseclickOutsideEvent = false;
  currSigner: DocumentSigner;
  generalNote: string;
  signersClassId: string[] = [];
  SendingMethod: typeof SendingMethod = SendingMethod;
  AuthMode = AuthMode;
  OtpMode = OtpMode;
  shouldShowSigner1SigningLockOption = false;
  signerAuthActive = false;
  signerAuthActiveByDefault = false;
  enableVisualIdentification = true;
  enableVideoConference = false;
  SignersIdentificationType: string[];
  IdentificationTypeRequierd = false;
  languageJsonPrefix = "SIGNERS.AUTH_MODES.";
  AppendixSigners = new Map<string, number[]>();
  dropDownTop = 0;
  dropDownLeft = 0;
  selectDistinctContactVideoConfrence: SelectDistinctContactVideoConfrence[] = [];

  constructor(
    private location: Location,
    private stateService: StateProcessService,
    private groupAssignService: GroupAssignService,
    private router: Router,
    private documentsApiService: DocumentApiService,
    private sharedService: SharedService,
    private store: Store 
  ) { }

  ngOnInit() {
    this.request = new DocumentCollectionCreateRequest();
    this.state$ = this.stateService.getState();
    this.documentMode = this.groupAssignService.isOrderedWorkflow ? SignMode.OrderedWorkflow : SignMode.Workflow;
    this.request.shouldEnableMeaningOfSignature = this.groupAssignService.useMeaningOfSignature;
    (<HTMLInputElement>document.getElementById("ignoreSigningOrder")).checked = this.documentMode == SignMode.Workflow;
    this.state$.pipe(take(1)).subscribe(
      (state: AppState) => {
        this.appState = state;

        if (this.appState.companySigner1Details != null) {
          this.shouldShowSigner1SigningLockOption = this.appState.companySigner1Details.shouldShowInUI;
          this.request.shouldSignUsingSigner1AfterDocumentSigningFlow = this.appState.companySigner1Details.shouldSignAsDefaultValue;
        }

        this.enableVideoConference = state.enableVideoConferenceFlow;
        this.signerAuthActive = state.EnableUseSignerAuth;
        this.signerAuthActiveByDefault = this.signerAuthActive && state.UseSignerAuthByDefault;
        this.enableVisualIdentification = true;
        this.documentName = state.CurrentDocumentName;
        this.signersClassId = state.Signers.map(x => x.ClassId);
        this.request.signers = state.Signers.map(
          (x) => {
            let docSigner = new DocumentSigner();
            docSigner.contactName = x.FullName;
            docSigner.contactMeans = x.DeliveryMeans;
            docSigner.sendingMethod = x.DeliveryMethod == SendingMethod.TABLET ? SendingMethod.TABLET :
              validator.isEmail(x.DeliveryMeans) ? SendingMethod.EMAIL :
                SendingMethod.SMS;
            docSigner.phoneExtension = x.DeliveryExtention;
            docSigner.otpMode = state.ShouldSendWithOTPByDefault ? OtpMode.CodeRequired : OtpMode.None;
            docSigner.authenticationMode = AuthMode.None;
            return docSigner;
          });

        this.SignersIdentificationType = new Array(this.request.signers.length).fill("");

        if (this.signerAuthActiveByDefault) {
          this.request.signers.forEach(x => x.authenticationMode = AuthMode.IDP)
        }

        this.request.signers.forEach(signer => {
          this.currSigner = signer;
          this.updateIdentificationMode();
        });

        this.updateSignersFields();
      });
  }

  isAuthSelected(signerIndex) {
    return this.request.signers[signerIndex].authenticationMode == AuthMode.IDP;
  }

  selectedAuthForSigner(event, signerIndex) {
    if (this.request.signers[signerIndex].authenticationMode == AuthMode.IDP) {
      this.request.signers[signerIndex].authenticationMode = AuthMode.None;
    }

    else {
      this.request.signers[signerIndex].authenticationMode = AuthMode.IDP;
    }

    console.log(this.request.signers[signerIndex].authenticationMode)
  }

  updateSignersFields() {
    this.groupAssignService.groupFieldsObserv.subscribe(
      (data: Map<string, PageField[]>) => {
        data.forEach((val, key) => {
          let index = this.signersClassId.findIndex(x => x == key);
          if (index > -1) {
            this.request.signers[index].signerFields = val.map(a => {
              const sf = new SignerField();
              sf.templateId = (a as PageField).templateId;
              sf.fieldName = (a as PageField).name;
              if (a instanceof SignatureField) {
                sf.fieldValue = (a as SignatureField).image;
              }
              if (a instanceof TextField) {
                sf.fieldValue = (a as TextField).value;
              }

              return sf;
            });

            this.request.signers[index].signerFields = Array.from(new Set(this.request.signers[index].signerFields.map(a => a.fieldName)))
              .map(fieldName => {
                return this.request.signers[index].signerFields.find(a => a.fieldName === fieldName)
              });
          }
        });
      });
  }

  sendDocument() {
    this.request.documentName = this.documentName;
    this.request.documentMode = this.documentMode;
    this.request.senderNote = this.generalNote;
    this.request.templates = this.appState.SelectedTemplates.map(x => x.templateId);
    const previousSigners: DocumentSigner[] = JSON.parse(JSON.stringify(this.request.signers))
    this.createSharedFromDuplication();

    this.isBusy = true;
    this.sharedService.setBusy(true, "DOCUMENT.SENDING");
    this.documentsApiService.documentCreate(this.request).subscribe(
      (res: DocumentCreateResult) => {
        this.isBusy = false;
        this.sharedService.setBusy(false);
        this.groupAssignService.updateFieldsMap(new Map<string, PageField[]>());
        this.groupAssignService.useMeaningOfSignature = false;
        this.cleanSessionStorage();

        // Set file name to empty string in the state
        this.store.dispatch(new documentActions.SetDocumentName({ CurrentDocumentName: "" }));

        this.router.navigate(["/dashboard", "success"]);
      },
      (err) => {
        this.request.signers = previousSigners; // mechanism to prevent data loss in case of errors 
        this.isBusy = false;
        this.sharedService.setBusy(false);
        this.sharedService.setErrorAlert(new Errors(err.error));
      });
  }

  cleanSessionStorage() {
    sessionStorage.removeItem("FIELDS_ARRAY");
    sessionStorage.removeItem("CONTACTS");
    sessionStorage.removeItem("SIGNERS");
  }

  backToEditDocument() {
    this.location.back();
  }

  back() {
    this.location.back();
  }

  showDrop(i, event) {
    this.fixDropDownPositionForSignerAdditionalActions(event);

    if (this.currSignerIndex == i) {
      this.currSignerIndex = -1;
      this.listenTocloseclickOutsideEvent = false;
    }

    else {
      this.listenTocloseclickOutsideEvent = false;
      this.currSignerIndex = i;
      this.currSigner = this.request.signers[this.currSignerIndex];
      setTimeout(() => this.listenTocloseclickOutsideEvent = true, 300);
    }
  }

  fixDropDownPositionForSignerAdditionalActions(event) {
    let btnRect = (event.target as HTMLElement).getBoundingClientRect();
    this.dropDownTop = btnRect.top + btnRect.height + 5;
    this.dropDownLeft = btnRect.left;
  }

  showVideoConferencePopUpProcess() {
    if (this.selectDistinctContactVideoConfrence.length === 0) {
      this.request.signers.forEach(element => {

        if (!this.selectDistinctContactVideoConfrence.some(contact =>
          contact.contactMeans === element.contactMeans)) {
          let distinctContact = new SelectDistinctContactVideoConfrence();
          distinctContact.contactId = element.contactId;
          distinctContact.contactMeans = element.contactMeans;
          distinctContact.contactName = element.contactName;
          distinctContact.sendingMethod = element.sendingMethod;
          distinctContact.phoneExtension = element.phoneExtension;
          distinctContact.selected = false;
          this.selectDistinctContactVideoConfrence.push(distinctContact);
        }
      });
    }
    
    this.showVideoConferencePopUp = true;
  }

  hide() {
    this.showOtp = false;
    this.showAppendices = false;
    this.showAttachments = false;
    this.showPersonalNote = false;
    this.showVisualIdentificationPopUp = false;
    this.showVideoConferencePopUp = false;
    this.currSignerIndex = -1;
    this.listenTocloseclickOutsideEvent = false;
  }

  updateIdentificationMode() {
    if (this.currSigner.authenticationMode == AuthMode.ComsignlIDP) {
      this.SignersIdentificationType[this.request.signers.indexOf(this.currSigner)] = this.languageJsonPrefix + "FACIALIDENTFICIATION"

    }

    else if (this.currSigner.otpMode == OtpMode.None) {
      this.SignersIdentificationType[this.request.signers.indexOf(this.currSigner)] = "";
      this.IdentificationTypeRequierd = !this.SignersIdentificationType.every((x) => x == "");
      return;
    }

    else {
      this.SignersIdentificationType[this.request.signers.indexOf(this.currSigner)] = this.languageJsonPrefix + String((Object.keys(OtpMode).filter((x) => Number.isNaN(Number(x)))[this.currSigner.otpMode])).toUpperCase();
    }

    this.IdentificationTypeRequierd = true;
  }

  getDocumentAppendices(appendices) {
    this.request.senderAppendices = this.request.senderAppendices.concat(appendices);
    this.request.senderAppendices = this.removeDuplication(this.request.senderAppendices);
  }

  createSharedFromDuplication() {
    for (let i = 0; i < this.request.signers.length; i++) {
      let signer = this.request.signers[i];
      if (signer.senderAppendices !== undefined) {
        signer.senderAppendices.forEach(appendix => {
          let appendixJSON = JSON.stringify(appendix)
          if (this.AppendixSigners.has(appendixJSON)) {
            let signerIndexesList = this.AppendixSigners.get(appendixJSON)
            this.AppendixSigners.set(appendixJSON, [...signerIndexesList, i])
          }
          else {
            this.AppendixSigners.set(appendixJSON, [i])
          }
        });
      }
    }

    this.AppendixSigners.forEach((signerIndexes, appendixJson) => {
      if (signerIndexes.length > 1) {
        let appendix = JSON.parse(appendixJson)
        signerIndexes.forEach((signerIndex) => {
          let appendixIndex = this.request.signers[signerIndex].senderAppendices.findIndex(searchedAppendix => {
            return searchedAppendix.base64file == appendix.base64file && searchedAppendix.name == appendix.name;
          });

          if (appendixIndex != -1) {
            this.request.signers[signerIndex].senderAppendices.splice(appendixIndex, 1)
          }
        });

        let sharedAppendix = new SharedAppendix()
        sharedAppendix.signerIndexes = signerIndexes
        sharedAppendix.appendix = JSON.parse(appendixJson)
        this.request.SharedAppendices.push(sharedAppendix);
      }
    });
  }

  //Remove equals objects(all properties equals). and not remove duplication by a key.
  removeDuplication(fieldsArr: Appendix[]) {
    let newArr = [];

    fieldsArr.forEach(x => {

      let index = newArr.findIndex(a => a.name == x.name);
      if (index < 0) {
        newArr.push(x);
      }
      else {
        let item = newArr[index];
        if (x.name != item.name || x.base64file != item.base64file) {
          newArr.push(x);
        }
      }
    });

    return newArr;
  }

  updateSignMode() {
    if ((<HTMLInputElement>document.getElementById("ignoreSigningOrder")).checked) {
      this.documentMode = SignMode.Workflow;
    }

    else {
      this.documentMode = SignMode.OrderedWorkflow;
    }
  }

  updateSigner1SigningAfterDocumentSignedMode() {
    this.request.shouldSignUsingSigner1AfterDocumentSigningFlow = !this.appState.companySigner1Details.shouldSignAsDefaultValue;
  }


  closeclickOutside($event) {
    if (this.listenTocloseclickOutsideEvent && this.currSignerIndex != -1) {
      this.currSignerIndex = -1;
    }
  }
  removeSignerMode(signerIndex) {
    this.request.signers[signerIndex].authenticationMode = AuthMode.None;
    this.request.signers[signerIndex].otpMode = OtpMode.None;
    this.SignersIdentificationType[signerIndex] = ""
    this.IdentificationTypeRequierd = !this.SignersIdentificationType.every((x) => x == "");
  }

  upClick(signerIndex: number) {
    let higherSigner = this.request.signers[signerIndex - 1];
    let lowerSigner = this.request.signers[signerIndex];

    let higherAuthenthication = this.request.signers[signerIndex - 1].authenticationMode
    let lowerAuthenthication = this.request.signers[signerIndex].authenticationMode

    this.request.signers[signerIndex - 1] = lowerSigner;
    this.request.signers[signerIndex] = higherSigner;

    this.request.signers[signerIndex - 1].authenticationMode = lowerAuthenthication;
    this.request.signers[signerIndex].authenticationMode = higherAuthenthication;

    let temp = this.SignersIdentificationType[signerIndex - 1];
    this.SignersIdentificationType[signerIndex - 1] = this.SignersIdentificationType[signerIndex]
    this.SignersIdentificationType[signerIndex] = temp;
  }

  downClick(signerIndex: number) {
    let higherSigner = this.request.signers[signerIndex];
    let lowerSigner = this.request.signers[signerIndex + 1];

    let higherAuthenthication = this.request.signers[signerIndex].authenticationMode
    let lowerAuthenthication = this.request.signers[signerIndex + 1].authenticationMode

    this.request.signers[signerIndex] = lowerSigner;
    this.request.signers[signerIndex].authenticationMode = lowerAuthenthication;

    this.request.signers[signerIndex + 1] = higherSigner;
    this.request.signers[signerIndex + 1].authenticationMode = higherAuthenthication;

    let temp = this.SignersIdentificationType[signerIndex];
    this.SignersIdentificationType[signerIndex] = this.SignersIdentificationType[signerIndex + 1]
    this.SignersIdentificationType[signerIndex + 1] = temp;
  }
}