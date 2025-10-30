import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Contact, SendingMethod } from '@models/contacts/contact.model';
import { DocumentSigner } from '@models/document-api/document-create-request.model';
import { Notes } from '@models/document-api/notes.model';
import { ReplaceSignerWithDetailsRequest } from '@models/document-api/replace-signer-with-details-request';
import { AuthMode } from '@models/enums/auth-mode.enum';
import { OtpMode } from '@models/enums/otp-mode.enum';
import { UserProgram } from '@models/program/user-program.model';
import { DocumentApiService } from '@services/document-api.service';
import { SharedService } from '@services/shared.service';
import { SignersApiService } from '@services/signers-api.service';
import { StateProcessService } from '@services/state-process.service';
import { AlertLevel, AppState } from '@state/app-state.interface';
import { Observable } from 'rxjs';
import validator from 'validator';

@Component({
  selector: 'sgn-replace-signer',
  templateUrl: './replace-signer.component.html',
  styles: []
})
export class ReplaceSignerComponent implements OnInit {

  @Output() public hide = new EventEmitter<any>();

  @Input() oldSignerId: string;
  @Input() docId: string;
  AuthMode = AuthMode;
  previousMeans: string;
  public previousSendingMethod: SendingMethod;
  public name: string;
  public means: string;
  public isBusy: boolean;
  public submitted: boolean;
  public hasError: boolean = false;
  public sameSignerError: boolean = false;
  public showPersonalNote: boolean;
  public showOtp: boolean;
  public showVisualIdentificationPopUp: boolean;
  public enableVisualIdentification: boolean = true;
  public signerAuthActive: boolean = false;
  public signerAuthActiveByDefault: boolean = false;
  public signer: DocumentSigner;
  public dropDownLeft: number = 0;
  public showExtraInfoDrop: boolean;
  public listenTocloseclickOutsideEvent = false;
  public deliveryMethod: SendingMethod = SendingMethod.EMAIL;
  public telOptions = { initialCountry: 'il' };
  public phoneExt: string = "+972";
  public deliveryExtention: string;
  public SendingMethod: typeof SendingMethod = SendingMethod;
  public userProgram: UserProgram;
  public state$: Observable<any>;
  public showContacts: boolean = false;
  public IdentificationTypeRequierd: boolean = false;
  public languageJsonPrefix: string = "SIGNERS.AUTH_MODES.";
  public signerIdentificationType: string;

  constructor(private documentApiService: DocumentApiService, private signersService: SignersApiService,
    private stateService: StateProcessService, private sharedService: SharedService) { }

  ngOnInit() {
    this.signer = new DocumentSigner();
    this.signerIdentificationType = "";
    this.state$ = this.stateService.getState();
    this.state$.subscribe((x: AppState) => {
      this.userProgram = x.program;
      this.signerAuthActive = x.EnableUseSignerAuth;
      this.signerAuthActiveByDefault = this.signerAuthActive && x.UseSignerAuthByDefault;
      if (this.signerAuthActiveByDefault) {
        this.signer.authenticationMode = AuthMode.IDP;
      }
    });

    this.documentApiService.getDocument(this.docId).subscribe(res => {
      this.enableVisualIdentification = true;
      const docSigner = res.signers.find(_ => _.id == this.oldSignerId);
      console.log(docSigner);
      this.signer.senderNote = docSigner.userNote;
      this.signer.phoneExtension = docSigner.contact.phoneExtension;
      this.signer.sendingMethod = docSigner.sendingMethod;
      this.signer.contactId = docSigner.contact.id;
      this.signer.contactName = docSigner.contact.name;
      this.signer.otpMode = docSigner.signerAuthentication.otpDetails.mode;
      this.signer.authenticationMode = docSigner.signerAuthentication.authenticationMode;
      this.signer.otpIdentification = docSigner.signerAuthentication.otpDetails.identification;
      this.updateIdentificationMode();

      if (docSigner.contact.seals.length > 0) {
        this.signer.sealId = docSigner.contact.seals[0].id;
      }

      if (docSigner.sendingMethod == SendingMethod.EMAIL) {
        this.signer.contactMeans = docSigner.contact.email;
        this.previousMeans = docSigner.contact.email;
        this.previousSendingMethod = SendingMethod.EMAIL;
      }

      else if (docSigner.sendingMethod == SendingMethod.SMS) {
        this.signer.contactMeans = docSigner.contact.phone;
        this.previousMeans = docSigner.contact.phone;
        this.previousSendingMethod = SendingMethod.SMS;
      }
    });
  }

  showDrop(event) {
    if (!this.showExtraInfoDrop) {
      setTimeout(() => this.listenTocloseclickOutsideEvent = true, 300);
    }
    this.showExtraInfoDrop = !this.showExtraInfoDrop;
    this.listenTocloseclickOutsideEvent = false;
  }

  closeclickOutside($event) {
    if (this.listenTocloseclickOutsideEvent && this.showExtraInfoDrop) {
      this.showExtraInfoDrop = false;
    }
  }

  hideAdditionalOptions() {
    this.showPersonalNote = false;
    this.showOtp = false;
    this.showVisualIdentificationPopUp = false;
  }

  onPersonalNoteClick() {
    if (this.signer) {
      this.showPersonalNote = true;
    }

    else {
      this.sharedService.setTranslateAlert("TINY.INVALID_SIGNER", AlertLevel.ERROR);
    }
  }

  onOtpClick() {
    if (this.signer) {
      this.showOtp = true;
    }

    else {
      this.sharedService.setTranslateAlert("TINY.INVALID_SIGNER", AlertLevel.ERROR);
    }
  }

  onFaceRecognitionClick() {
    if (this.signer) {
      this.showVisualIdentificationPopUp = true;
    }

    else {
      this.sharedService.setTranslateAlert("TINY.INVALID_SIGNER", AlertLevel.ERROR);
    }
  }

  isAuthSelected() {
    return this.signer.authenticationMode == AuthMode.IDP;
  }

  selectedAuthForSigner() {
    if (this.signer.authenticationMode == AuthMode.IDP) {
      this.signer.authenticationMode = AuthMode.None;
    }

    else {
      this.signer.authenticationMode = AuthMode.IDP;
    }
  }

  updateIdentificationMode() {
    if (this.signer.authenticationMode == AuthMode.ComsignlIDP) {
      this.signerIdentificationType = this.languageJsonPrefix + "FACIALIDENTFICIATION";
    }

    else if (this.signer.otpMode == OtpMode.None) {
      this.signerIdentificationType = "";
      this.IdentificationTypeRequierd = false;
      return;
    }

    else {
      this.signerIdentificationType = this.languageJsonPrefix + String((Object.keys(OtpMode).filter((x) => Number.isNaN(Number(x)))[this.signer.otpMode])).toUpperCase();
    }
    this.IdentificationTypeRequierd = true;
  }

  removeSignerMode() {
    this.signer.authenticationMode = AuthMode.None;
    this.signer.otpMode = OtpMode.None;
    this.signerIdentificationType = ""
    this.IdentificationTypeRequierd = false;
  }

  close() {
    this.hide.emit();
  }

  updateSigner(contact: Contact) {
    this.hasError = false;
    this.sameSignerError = false;
    this.showContacts = false;
    
    if (contact) {
      this.name = contact.name;
      this.means = contact.defaultSendingMethod == SendingMethod.EMAIL ? contact.email : contact.phone;
      this.deliveryMethod = contact.defaultSendingMethod;
      this.phoneExt = contact.phoneExtension;
    }
  }

  openContactsBook($event) {
    this.showContacts = true;
  }

  send() {
    this.submitted = true;
    this.hasError = false;
    this.sameSignerError = false;
    if (!this.name || this.name == "" || !this.means || this.means == "" ||
      (!validator.isEmail(this.means) && !validator.isMobilePhone(this.means)) ||
      (validator.isEmail(this.means) && this.deliveryMethod != SendingMethod.EMAIL) ||
      (validator.isMobilePhone(this.means) && this.deliveryMethod != SendingMethod.SMS)) {
      this.hasError = true;
      return;
    }

    if (this.previousSendingMethod === this.deliveryMethod && this.previousMeans === this.means) {
      this.sameSignerError = true;
      return;
    }

    this.isBusy = true;
    let signerNotes = new Notes();
    signerNotes.userNote = this.signer.senderNote;
    let input = new ReplaceSignerWithDetailsRequest();
    input.newSignerName = this.name;
    input.newSignerMeans = this.means;
    input.newNotes = signerNotes;
    input.newOtpMode = this.signer.otpMode;
    input.newOtpIdentification = this.signer.otpIdentification;
    input.newAuthenticationMode = this.signer.authenticationMode;

    this.signersService.replaceSignerWithDetails(this.docId, this.oldSignerId, input).subscribe(
      (data) => {
        this.submitted = false;
        this.isBusy = false;
        this.hasError = false;
        this.sameSignerError = false;
        this.sharedService.setSuccessAlert("ERROR.OPERATION.1");
        this.hide.emit();
      },
      (err) => {
        this.hasError = true;
        this.submitted = false;
        this.isBusy = false;
      }
    );
  }

  onSignerMethodChanged(event) {
    let useEmail = event.target.selectedIndex == 0;
    this.deliveryMethod = useEmail ? SendingMethod.EMAIL : SendingMethod.SMS;
  }

  onCountryChange(obj) {
    this.phoneExt = obj.dialCode;
    this.deliveryExtention = `+${this.phoneExt}`;
  }
}