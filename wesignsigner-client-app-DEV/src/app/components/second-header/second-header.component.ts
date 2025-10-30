import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChange, SimpleChanges, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { DocumentOperation } from 'src/app/enums/document-operation.enum';
import { SignatureType } from 'src/app/enums/signature-type.enum';
import { TextFieldType } from 'src/app/enums/text-field-type.enum';
import { UpdateDocumentCollectionRequest } from 'src/app/models/requests/update-document-collection-request.model';
import { UpdateDocumentRequest } from 'src/app/models/requests/update-document-request.model';
import { SignerAuthentication } from 'src/app/models/responses/signer-authentication.model';
import { AppState } from 'src/app/models/state/app-state.model';
import { DocumentsService } from 'src/app/services/documents.service';
import { StateService } from 'src/app/services/state.service';
import { LiveEventsService } from 'src/app/services/live-events.service';
import { SmartCardService } from 'src/app/services/smart-card.service';
import { DocumentData } from 'src/app/models/state/document-data.model';
import { FieldRequest } from 'src/app/models/requests/fields-request.model';
import { WeSignFieldType } from 'src/app/enums/we-sign-field-type.enum';
import { Signer1Credential } from 'src/app/models/responses/signer1-credential.model';
import { Errors } from 'src/app/models/error/errors.model';
import { TranslateService } from '@ngx-translate/core';
import { Attachment } from 'src/app/models/responses/attachment.model';
import { AppConfigService } from 'src/app/services/app-config.service';
import { BaseField } from 'src/app/models/pdffields/basefield.model';
import { DatePipe } from '@angular/common'
import { SmartCardAlertComponent } from '../alert/smart-card-alert/smart-card-alert.component';
import { textField } from 'src/app/models/pdffields/field.model';
import { IedasSigningFlowComponent } from '../iedas-signing-flow/iedas-signing-flow.component';
import { IdentificationService } from 'src/app/services/identification.service';
import { CreateAuthFlowModel } from 'src/app/models/requests/identity-flow-create.model';

@Component({
  selector: 'app-second-header',
  templateUrl: './second-header.component.html',
  styleUrls: ['./second-header.component.scss']
})
export class SecondHeaderComponent implements OnInit, OnChanges {

  public shouldOpenMenu: boolean = false;
  public showErrorMessage: boolean = false;
  public isSenderLink: boolean = false;
  public currentPage: number = 1;
  public alertMessage: string = "";
  public state: AppState;
  public isBusy: boolean = false;
  private mainUrl: string = "";
  private hasAttachements: boolean = false;
  public subscription: any;
  private attachmentShown: boolean = false;
  public redirectTimeout: number;
  public inProcess: boolean = true;
  public postChangeInProcess: boolean = true;
  attachments: Attachment[];
  public showSmartCardAlert: boolean = false;
  public showiedasSigningFlow: boolean = false;
  public isFinishOnSigner: boolean = false;

  public showConfirmMessagePopup: boolean = false;
  public confirmMessagePopup: string = "";
  public confirmMessagePopupTitle: string = "";
  public confirmMessage2Popup: string = "";
  public confirmMessagePopupBold: string = "";
  // comment only for a commit

  private mandatoryEmptyFields: BaseField[];
  private mandatoryEmptyFieldsCount: number;
  private mandatoryEmptyFieldsCountOnInit: number;
  private currentJumpedFieldIndex: number = 0;

  private mandatoryEmptyFieldsOnInit: BaseField[];
  private dataChangesCounter: number = 0;

  private nameLocalCurrentJumpedField: string;
  private yLocationLocalCurrentJumpedField: number;
  private pageLocalCurrentJumpedField: number;

  public showAttachmentsIsMain: boolean = false;
  @Input() public errorMessageToServerCredential: string = "Error - Certificate Id/Password is missing";
  @Input() public showErrorMsgInServerCredential: boolean = false;
  @Input() public totalPages: number = 1;
  @Input() public showPagenation: boolean = true;
  @Input() public token: string = "";
  @Input() public isSenderFinish: boolean = false;
  @Input() public shouldSetImageToAllSignatures: boolean = false;
  @Output() public moveToSuccessPage = new EventEmitter<string>();
  @Output() public changeLoaderEvent = new EventEmitter<{ show: boolean }>();
  @Output() public closeSpinner = new EventEmitter<any>();
  @Output() public showSpinner = new EventEmitter<any>();

  ADName: string;
  signerAuthToken: string;
  showSuccessMessage: boolean;
  invalidElements: textField[] = [];
  @ViewChild('smartCardAlert') smartCardAlert: SmartCardAlertComponent;
  @ViewChild('iedasSigningFlow') iedasSigningFlow: IedasSigningFlowComponent;

  constructor(private documentsService: DocumentsService, private router: Router, private liveEventsService: LiveEventsService,
    private stateService: StateService, private smartCardServiceApi: SmartCardService, private translate: TranslateService,
    private appConfigService: AppConfigService, private datePipe: DatePipe, private identificationService: IdentificationService) { }

  ngOnInit(): void {
    this.stateService.state$.subscribe((data) => {

      this.state = data;
      this.hasAttachements = data.attachments && data.attachments.length > 0;
      this.attachmentShown = data.isAttachmentshown;
      this.attachments = data.attachments;
      this.ADName = this.state.signerADName;
      this.signerAuthToken = this.state.signerAuthToken;

      if (data.documentsData && data.documentsData.length > 0) {

        this.mandatoryEmptyFields = this.getAllMandatoryEmptyFieldsAsBaseFields();
        if (this.mandatoryEmptyFields) {

          if (this.dataChangesCounter == 0) {
            this.mandatoryEmptyFieldsOnInit = this.mandatoryEmptyFields;
            this.dataChangesCounter++;
            if (this.mandatoryEmptyFieldsOnInit) {
              this.mandatoryEmptyFieldsCountOnInit = this.mandatoryEmptyFieldsOnInit.length
            }
          }
          if (this.mandatoryEmptyFields) {
            //    console.log(this.mandatoryEmptyFields)
            this.mandatoryEmptyFieldsCount = this.mandatoryEmptyFields.length
          }

          //    console.log(`data.currentJumpedField: `);
          //    console.log(data.currentJumpedField);

          this.nameLocalCurrentJumpedField = data.currentJumpedField.name;
          this.yLocationLocalCurrentJumpedField = data.currentJumpedField.yLocation;
          this.pageLocalCurrentJumpedField = data.currentJumpedField.page;

        }

        this.postChangeInProcess = !this.mandatoryEmptyFields ? false : true;

        if (this.inProcess && !this.postChangeInProcess) {
          this.showSuccessMessage = true;
          this.alertMessage = this.translate.instant('ERROR.OPERATION.4');
        }
        this.inProcess = this.postChangeInProcess
      }
    });

    this.mainUrl = this.router.url;
    this.isSenderLink = this.mainUrl.includes("sender");
    this.redirectTimeout = this.appConfigService.redirectTimeoutconfig;
  }

  ngOnDestroy() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  CloseMessageConfirmMessagePopup() {
    this.showConfirmMessagePopup = false;
  }

  getBaseFields(documentsData: DocumentData[]): BaseField[] {
    let baseFields: BaseField[] = []
    if (!documentsData) {
      return;
    }
    for (let i = 0; i < documentsData.length; i++) {
      let documentBaseFields: BaseField[] = []
      let documentsDataPdfFields = documentsData[i].pdfFields;
      let choiceFields = documentsDataPdfFields.choiceFields.filter(x => x.mandatory && x.selectedOption == "") as BaseField[]
      let checkboxFields = documentsDataPdfFields.checkBoxFields.filter(x => x.mandatory && !x.isChecked) as BaseField[]
      let signatureFields = documentsDataPdfFields.signatureFields
        .filter(x => x.mandatory && x.image == null) as BaseField[];

      let textFields = documentsDataPdfFields.textFields.filter(x => x.mandatory && x.value == "") as BaseField[]

      let radioFields: BaseField[] = [];
      for (let y = 0; y < documentsDataPdfFields.radioGroupFields.length; y++) {
        if (documentsDataPdfFields.radioGroupFields[y].selectedRadioName == "") {
          let radioField = documentsDataPdfFields.radioGroupFields[y].radioFields[0] as BaseField;
          if (radioField.mandatory) {
            radioFields.push(radioField)
          }
        }
      }
      documentBaseFields = documentBaseFields.concat(choiceFields, signatureFields, textFields, radioFields, checkboxFields);
      documentBaseFields = this.sortFieldsByPageAndY(documentBaseFields);
      baseFields = baseFields.concat(documentBaseFields)
    }
    return baseFields;
  }

  sortFieldsByPageAndY(filteredBaseFields: BaseField[]): BaseField[] {
    filteredBaseFields = filteredBaseFields.sort((a, b) => {
      let aPage = a.page
      let bPage = b.page
      let aY = a.y
      let bY = b.y
      if (aPage == bPage) {
        return (aY < bY) ? -1 : (aY > bY) ? 1 : 0;
      } else {
        return (aPage < bPage) ? -1 : 1;
      }
    })
    return filteredBaseFields;
  }

  getAllMandatoryEmptyFieldsAsBaseFields(): BaseField[] {
    let documentsData = this.state.documentsData;
    let filteredBaseFields: BaseField[] = [];

    filteredBaseFields = this.getBaseFields(documentsData);

    if (filteredBaseFields.length == 0) {
      return;
    }

    return filteredBaseFields;
  }

  writeToErrorAlert(event) {
    this.alertMessage = event;
    this.showErrorMessage = true;
  }

  nextField() {
    this.mandatoryEmptyFields = this.getAllMandatoryEmptyFieldsAsBaseFields();
    if (!this.mandatoryEmptyFields) {
      return;
      // this.mandatoryEmptyFieldsCount = this.mandatoryEmptyFields.length;
    }
    let newIndex: number = this.calculateIndexOfNextJumpedField();

    let item = this.mandatoryEmptyFields[newIndex];

    this.stateService.SetCurrentJumpedField({ page: item.page, yLocation: item.y, name: item.name });
    this.currentJumpedFieldIndex++;

    var el: HTMLElement;
    try {
      el = document.getElementById(item.name);
      if (!el) {
        const elements = document.getElementsByTagName("input");
        var arr = [].slice.call(elements);
        arr = arr.filter(x => x.id.includes(item.name))[0] as HTMLElement;
        el = arr

      }
      el.classList.add("is-error");
      el.classList.remove("is-mandatory");
    }

    catch (error) {
      console.error("this is NOT signature")
    }
  }

  private calculateIndexOfNextJumpedField(): number {
    if (this.mandatoryEmptyFieldsOnInit.length == 1 || this.mandatoryEmptyFields.length == 1) {
      return 0;
    }
    const elementFromTheInitArray = this.mandatoryEmptyFieldsOnInit.find(x =>
      x.name == this.nameLocalCurrentJumpedField &&
      x.page == this.pageLocalCurrentJumpedField &&
      x.y == this.yLocationLocalCurrentJumpedField);

    var selectedElementIfNotModified = this.mandatoryEmptyFields.find(x =>
      x.name == this.nameLocalCurrentJumpedField &&
      x.page == this.pageLocalCurrentJumpedField &&
      x.y == this.yLocationLocalCurrentJumpedField);

    let newIndex: number;
    let indexOfSelectedItem = this.mandatoryEmptyFields.indexOf(elementFromTheInitArray);

    let indexInTheOriginalArray = this.mandatoryEmptyFieldsOnInit.indexOf(elementFromTheInitArray)

    if (indexInTheOriginalArray == this.mandatoryEmptyFieldsCountOnInit - 1 ||
      indexOfSelectedItem == this.mandatoryEmptyFieldsCount - 1) {
      newIndex = 0;
    }

    else {
      if (selectedElementIfNotModified) {
        newIndex = this.mandatoryEmptyFields.indexOf(elementFromTheInitArray) + 1;
      } else {
        newIndex = this.mandatoryEmptyFieldsOnInit.indexOf(elementFromTheInitArray) - indexInTheOriginalArray - this.mandatoryEmptyFields.indexOf(elementFromTheInitArray);
      }
      if (!newIndex || newIndex == -1) {
        newIndex = 0;
      }
    }
    return newIndex;
  }

  public onFinish() {
    if (this.state.documentCollectionData.mode == 3 && !this.state.documentsData) {
      this.liveEventsService.finishAsSender(this.state.Token);
    }
    else {
      this.next();
    }
  }

  public next() {
    if (this.state.documentCollectionData.mode == 3 && !this.state.documentsData)
      return;
    this.isSenderFinish = false;
    this.stateService.setSubmittion(true);
    if (this.hasAttachements) {
      if (!this.isValidAttachments()) {
        this.alertMessage = this.translate.instant('ERROR.INPUT.5');
        this.showErrorMessage = true;
        this.showAttachmentsIsMain = !this.showAttachmentsIsMain;

        this.stateService.setAttachmentshown();
        return;
      }
      else if (!this.attachmentShown) {
        this.showAttachmentsIsMain = !this.showAttachmentsIsMain;
        this.stateService.setAttachmentshown();
        return;
      }
    }

    if (this.hasAttachements && !this.isValidAttachments()) {
      this.alertMessage = this.translate.instant('ERROR.INPUT.5');
      this.showErrorMessage = true;
      return;
    }
    if (this.isValidDocument(true)) {
      this.handleDoneProcess();
    }
  }

  isValidAttachments() {
    let result: boolean = true;
    this.state.attachments.forEach(element => {
      if (element.isMandatory && element.base64File == null) {
        result = false;
      }
    });
    return result;
  }

  private handleDoneProcess() {
    this.stateService.setSubmittion(true);
    this.stateService.showLoader = true;
    this.showErrorMessage = false;
    let isValid = this.isValidDocument(true);

    if (!isValid) {
      this.stateService.showLoader = false;

      return;
    }

    if (!this.InEIdasSigningFlow()) {
      if (this.shouldAutomateSigner1Details()) {
        this.stateService.setServerSignatureCredential({ certId: this.ADName, password: "", authToken: this.signerAuthToken });
      }
      else if (this.state.signingType == SignatureType.Server && !this.isValidServerSignatureCredentials()) {
        this.stateService.showServerCredentialForm = true;
        this.stateService.showLoader = false;

        return;
      }
    }

    this.sendRequestToServer(false);
  }

  shouldAutomateSigner1Details() {
    return this.ADName != undefined && this.ADName != "";
  }

  smartCardAlertClose() {
    this.stateService.showLoader = false;
    this.isBusy = false;
  }

  eidasAlertClose() {
    this.stateService.showLoader = false;
    this.isBusy = false;
  }

  IsInSaveModeOperationNeeded() {
    if (this.state.operation == DocumentOperation.Save || this.state.signingType == SignatureType.SmartCard) {
      return true;
    }

    return this.InEIdasSigningFlow();
  }

  InEIdasSigningFlow() {
    if (this.state.signingType == SignatureType.Server && this.state.inEIDASSigngingFlow) {
      return true;
    }
    return false
  }

  sendRequestToServer(isSaveMode: boolean) {
    this.isBusy = true;
    //this.stateService.showLoader = true;
    this.showSpinner.emit();

    let request = new UpdateDocumentCollectionRequest();

    if (isSaveMode) {
      request.Operation = DocumentOperation.Save;
    }
    else {
      request.Operation = this.IsInSaveModeOperationNeeded() ? DocumentOperation.Save : this.state.operation;
    }

    request.SignerNote = this.state.signerNotes;
    request.SignerAttachments = this.state.attachments;

    if (!isSaveMode && this.state.signingType == SignatureType.Server && this.isValidServerSignatureCredentials()) {
      let signer1Credential = new Signer1Credential();
      signer1Credential.certificateId = this.state.serverSignatureCredential.certId;
      signer1Credential.password = this.state.serverSignatureCredential.password;
      signer1Credential.signerToken = this.state.serverSignatureCredential.authToken;
      let signerAuthentication = new SignerAuthentication();
      signerAuthentication.signer1Credential = signer1Credential;
      request.SignerAuthentication = signerAuthentication;
    }

    for (let documentData of this.state.documentsData) {
      let document = new UpdateDocumentRequest();
      document.DocumentId = documentData.documentId;
      document.Fields = this.GetFieldsRequest(documentData);
      request.Documents.push(document);
    }


    request.UseForAllFields = this.shouldSetImageToAllSignatures;

    this.documentsService.updateDocument(this.token, request).subscribe(
      (data) => {
        if (isSaveMode) {
          //TODO remove all is-error class from divs
          this.invalidElements.forEach(field => {

            let element = document.getElementById(field.name);
            element.parentElement.classList.add("is-optional");
            element.parentElement.classList.remove("is-error");
          });
          this.showErrorMessage = false;

          this.alertMessage = this.translate.instant('ERROR.OPERATION.3');

          this.confirmMessagePopupTitle = this.translate.instant('GLOBAL.SAVE_FILE_TITLE');
          this.confirmMessagePopup = this.translate.instant('GLOBAL.SAVE_FILE_MESSAGE_1');
          this.confirmMessagePopupBold = this.translate.instant('GLOBAL.SAVE_FILE_MESSAGE_BOLD');
          this.confirmMessage2Popup = this.translate.instant('GLOBAL.SAVE_FILE_MESSAGE_2');
          this.showConfirmMessagePopup = true;
          //this.stateService.showLoader = false;
          this.closeSpinner.emit();

          this.isBusy = false;
          return;
        }
        if (this.state.signingType == SignatureType.SmartCard) {
          this.showSmartCardAlert = true;

          this.closeSpinner.emit();
          //this.handleSmartCardSigningType();
        }
        else if (this.InEIdasSigningFlow()) {

          this.showiedasSigningFlow = true;
          this.closeSpinner.emit();
          // need to start EIDAS signing flow
        }
        else {
          this.afterSignProcess(data.downloadUrl, data.redirectUrl);
        }
      },
      (err) => {
        if (err.error.status == 2) {
          //  console.log(err.error.errors.error);
          this.stateService.setServerSignatureCredential({ certId: "", password: "", authToken: "" });
          this.stateService.setSignerADName("", "");
          this.showErrorMsgInServerCredential = true;
          let result = new Errors(err.error);
          this.alertMessage = this.translate.instant('SERVER_ERROR.' + result.errorCode);
          this.next();
        } else {
          this.stateService.showLoader = false;
          this.closeSpinner.emit();
          let result = new Errors(err.error);
          this.alertMessage = this.translate.instant('SERVER_ERROR.' + result.errorCode);
          this.showErrorMessage = true;
        }
        this.isBusy = false;
      });
  }

  handleEIDASSigningFlow() {
    this.showSpinner.emit();
    let authFlow = new CreateAuthFlowModel();
    authFlow.signerToken = this.token;
    this.identificationService.CreateidentityFlowEIDASSign(authFlow).subscribe(data => {
      window.location.href = data.identityFlowURL;
    },
      (err) => {
        this.closeSpinner.emit();
        let result = new Errors(err.error);
        this.alertMessage = this.translate.instant('SERVER_ERROR.' + result.errorCode);
        this.showErrorMessage = true;
      },
      () => {
        this.closeSpinner.emit();
      });
  }

  handleSmartCardSigningType() {
    this.showSpinner.emit();
    this.smartCardServiceApi.alertError.subscribe(
      (msg: string) => {
        this.alertMessage = msg;
        this.showErrorMessage = true;
        this.stateService.showLoader = false;
        this.isBusy = false;
        this.showSmartCardAlert = false;
        this.smartCardAlert.releaseSubmitButton();
        this.closeSpinner.emit();
      });

    this.stateService.showLoader = true;

    this.subscription = this.smartCardServiceApi.getSmartCardSigningResultEvent()
      .subscribe(({ isSuccess: isSuccess, downloadLink: downloadLink }) => {
        this.isSmartCardSigningProcessSuccess(isSuccess, downloadLink)
      });

    this.smartCardSigning(this.state.documentCollectionData.id, true);
    //Cancel loading UI after 1 min
    setTimeout(() => {
      this.stateService.showLoader = false;
      this.isBusy = false;
      this.smartCardAlert.isBusy = false;
      this.closeSpinner.emit();
    }, 80000);
  }

  isSmartCardSigningProcessSuccess(isSuccess: boolean, downloadLink: string) {
    if (isSuccess) {
      this.showSmartCardAlert = false;
      this.smartCardAlert.releaseSubmitButton();
      this.afterSignProcess(downloadLink, "");
    }
    else {
      //show signing smart card error
    }
  }

  afterSignProcess(downloadUrl: string, redirectUrl: string) {
    this.closeSpinner.emit();
    if (this.state.documentCollectionData) {
      this.liveEventsService.notifySigningResult(this.token, true);
    }
    this.stateService.showLoader = false;

    this.moveToSuccessPage.emit(downloadUrl);
    if (redirectUrl != null && redirectUrl != "") {
      setTimeout(() => {

        window.location.href = redirectUrl.replace(/&amp;/g, '&');

      }, this.redirectTimeout);
    }
    this.isBusy = false;
  }

  smartCardSigning(documentCollectionId: string, isLastDocumentInCollection: boolean) {

    try {
      this.smartCardServiceApi.sign(documentCollectionId, this.token, isLastDocumentInCollection);
    } catch (error) {
      this.isBusy = false;
      this.closeSpinner.emit();
      return false;
    }
  }

  isValidServerSignatureCredentials() {
    return this.state.serverSignatureCredential &&
      (this.state.serverSignatureCredential.certId && this.state.serverSignatureCredential.password) || this.state.signerADName;
  }

  private GetFieldsRequest(documentData: DocumentData): FieldRequest[] {
    let result: FieldRequest[] = [];
    this.addTextFields(documentData, result);
    this.addSignatureFields(documentData, result);
    this.addRadioGroupFields(documentData, result);
    this.addChoiceFields(documentData, result);
    this.addCheckBoxFields(documentData, result);

    return result;
  }

  addCheckBoxFields(documentData: DocumentData, result: FieldRequest[]) {
    for (let field of documentData.pdfFields.checkBoxFields) {
      let fieldRequest = new FieldRequest();
      fieldRequest.fieldName = field.name;
      fieldRequest.fieldValue = String(this.isCheckboxIsChecked(field.name));
      fieldRequest.fieldType = WeSignFieldType.CheckBoxField;
      result.push(fieldRequest);
    }
  }

  addChoiceFields(documentData: DocumentData, result: FieldRequest[]) {
    for (let field of documentData.pdfFields.choiceFields) {
      let fieldRequest = new FieldRequest();
      fieldRequest.fieldName = field.name;
      fieldRequest.fieldValue = field.selectedOption;
      fieldRequest.fieldType = WeSignFieldType.ChoiceField;
      result.push(fieldRequest);
    }
  }

  addRadioGroupFields(documentData: DocumentData, result: FieldRequest[]) {
    for (let field of documentData.pdfFields.radioGroupFields) {
      let fieldRequest = new FieldRequest();
      fieldRequest.fieldName = field.name;

      fieldRequest.fieldValue = field.selectedRadioName;

      fieldRequest.fieldType = WeSignFieldType.RadioGroupField;
      result.push(fieldRequest);
    }
  }

  addSignatureFields(documentData: DocumentData, result: FieldRequest[]) {
    for (let field of documentData.pdfFields.signatureFields) {
      if (field.mandatory || field.image != null) {
      let fieldRequest = new FieldRequest();
      fieldRequest.fieldName = field.name;
      fieldRequest.fieldValue = field.image;
      fieldRequest.fieldType = WeSignFieldType.SignatureField;
      result.push(fieldRequest);
      }
    }
  }

  addTextFields(documentData: DocumentData, result: FieldRequest[]) {
    for (let field of documentData.pdfFields.textFields) {
      let fieldRequest = new FieldRequest();
      fieldRequest.fieldName = field.name;

      if (field.textFieldType == TextFieldType.Date) {
        let d = new Date(field.value);
        if (!isNaN(d.getDate())) {
          fieldRequest.fieldValue = this.datePipe.transform(d, 'MMM d, y');
        }
        else {
          fieldRequest.fieldValue = field.value;
        }
      }
      else {
        fieldRequest.fieldValue = field.value;
      }

      fieldRequest.fieldType = WeSignFieldType.TextField;
      result.push(fieldRequest);
    }
  }

  isValidDocument(showMsg: boolean) {
    let areAllRequiredFieldsFilled = this.areAllRequiredFieldsFilled();
    let areAllValidTextFields = this.areAllValidTextFields();
    if (!areAllRequiredFieldsFilled.isValid || !areAllValidTextFields) {
      let errorMessage = "";
      if (showMsg) {
        this.showErrorMessage = true;
        errorMessage = this.GetErrorMessage(areAllValidTextFields);
        let element = document.getElementById(areAllRequiredFieldsFilled.fieldName);
        if (element != undefined) {
          element.scrollIntoView(true);
        }
        if (this.invalidElements.length > 0) {
          document.getElementById(this.invalidElements[0].name).scrollIntoView(true);
        }
        window.scroll(0, 0);
        this.alertMessage = errorMessage;
      }
      return false;
    }
    return true;
  }

  GetErrorMessage(areAllValidTextFields) {
    var msg = this.translate.instant('ERROR.INPUT.1');
    if (!areAllValidTextFields) {
      if (this.invalidElements.length > 0) {
        if (this.invalidElements[0].textFieldType == TextFieldType.Email) {
          msg = this.translate.instant('ERROR.INPUT.EmailField');
        }
        if (this.invalidElements[0].textFieldType == TextFieldType.Phone) {
          msg = this.translate.instant('ERROR.INPUT.PhoneField');
        }
      }

    }
    return msg;

  }

  areAllRequiredFieldsFilled() {
    for (let documentData of this.state.documentsData) {
      for (let field of documentData.pdfFields.textFields) {
        if (field.mandatory && field.value == "") {
          //return false;
          return { isValid: false, fieldName: field.name };
        }
      }
      for (let field of documentData.pdfFields.choiceFields) {
        if (field.mandatory && field.selectedOption == "") {
          //return false;
          return { isValid: false, fieldName: field.name };
        }
      }
      for (let field of documentData.pdfFields.checkBoxFields) {
        if (field.mandatory && !field.isChecked) {
          //return false;
          return { isValid: false, fieldName: field.name };
        }
      }
      for (let field of documentData.pdfFields.signatureFields) {
        if (field.mandatory && field.image == null) {
          return { isValid: false, fieldName: field.name };
        }
      }

      let result = this.validateRadioGroups(documentData);
      if (!result.isValid) {
        return result

      }
    }

    return { isValid: true, fieldName: null };
  }


  validateRadioGroups(documentData: any): { isValid: boolean, fieldName?: string } {
    if (documentData.pdfFields.radioGroupFields && documentData.pdfFields.radioGroupFields.length > 0) {
      // Create a map to store merged radio groups by name
      let radioGroupMap = new Map<string, any>();

      for (let radioGroup of documentData.pdfFields.radioGroupFields) {
        if (radioGroupMap.has(radioGroup.name)) {
          // Merge radio fields if the group already exists
          let existingGroup = radioGroupMap.get(radioGroup.name);
          existingGroup.radioFields = [...existingGroup.radioFields, ...radioGroup.radioFields];
        } else {
          // Add new group to the map
          radioGroupMap.set(radioGroup.name, { ...radioGroup });
        }
      }

      // Convert the map back to an array
      let mergedRadioGroups = Array.from(radioGroupMap.values());

      for (let radioGroup of mergedRadioGroups) {
        let hasValueForMandatoryField = false;
        for (let field of radioGroup.radioFields) {
          if (!field.mandatory) {
            hasValueForMandatoryField = true;
          }
          let radioField = (<HTMLInputElement>document.getElementById(`${radioGroup.name}_${field.name}`));
          if (field.mandatory && radioField && radioField.checked) {
            hasValueForMandatoryField = true;
          }
        }
        if (!hasValueForMandatoryField) {
          return { isValid: false, fieldName: radioGroup.name };
        }
      }
    }
    return { isValid: true };
  }

  areAllValidTextFields() {
    this.invalidElements = [];
    for (let documentData of this.state.documentsData) {
      for (let field of documentData.pdfFields.textFields) {
        if ((field.textFieldType == TextFieldType.Phone) &&
          (field.value != "") && (!/^(\+\d{1,2}\s?)?1?\-?\.?\s?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{2,4}$/.test(field.value) || field.value.length < 6)) {
          this.invalidElements.push(field);

          return false;
        }

        if (field.textFieldType == TextFieldType.Email &&
          field.value != "" &&
          !/^[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}$/.test(field.value.toLocaleLowerCase())) {
          this.invalidElements.push(field);
          return false;
        }
        if (field.textFieldType == TextFieldType.Date &&
          field.value != "") {
          var timestamp = Date.parse(field.value);
          this.invalidElements.push(field);
          return !isNaN(timestamp);
        }
        if (field.textFieldType == TextFieldType.Time &&
          field.value != "" &&
          !/(((2[0-3])|(1[0-9])|0[1-9]):([0-5][0-9]))/.test(field.value)) {
          this.invalidElements.push(field);
          return false;
        }
        //TODO custom
      }
    }
    return true;
  }

  isCheckboxIsChecked(groupName: string) {
    let checkbox = document.getElementById(groupName);
    if (checkbox) {
      const element = <HTMLInputElement>checkbox;
      return element.checked;
    }
    return false;
  }

  onClickedOutside(e: Event) {
    if ((<HTMLElement>e.target).id != "menuButton" && (<HTMLElement>e.target).parentElement.id != "menuFeather"
      && (<HTMLElement>e.target).parentElement.parentElement.id != "menuFeather") {
      this.shouldOpenMenu = false;
    }
  }

  save() {
    this.invalidElements.forEach(field => {
      let el = document.getElementById(field.name)
      if (el != undefined) {
        el.scrollIntoView(true);
        el.parentElement.classList.remove("is-error");
        el.parentElement.classList.add("is-optional");
      }
    });
    let areAllValidTextFields = this.areAllValidTextFields();
    if (!areAllValidTextFields) {
      this.invalidElements.forEach(field => {
        let el = document.getElementById(field.name)
        if (el != undefined) {
          el.scrollIntoView(true);
          el.parentElement.classList.add("is-error");
          el.parentElement.classList.remove("is-optional");
        }
      });
      window.scroll(0, 0);
      this.alertMessage = this.translate.instant('ERROR.INPUT.0');
      this.showErrorMessage = true;
      return;
    }
    this.sendRequestToServer(true);
  }

  closeAlert() {
    this.showErrorMessage = false;
    this.showSuccessMessage = false;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes && changes.isSenderFinish && changes.isSenderFinish.currentValue)
      this.next();

  }
}