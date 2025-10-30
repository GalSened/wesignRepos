import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { DocumentSigner } from '@models/document-api/document-create-request.model';
import { AuthMode } from '@models/enums/auth-mode.enum';
import { OtpMode } from '@models/enums/otp-mode.enum';

@Component({
  selector: 'sgn-otp',
  templateUrl: './otp.component.html',
  styles: []
})
export class OtpComponent implements OnInit {

  @Input() signer: DocumentSigner;
  @Output() public hide = new EventEmitter<any>();
  @Output() public updateIdentificationMode = new EventEmitter<any>();
  submitted: boolean;
  shouldUsePassword: boolean;
  shouldUseOtpCode: boolean;
  otpPassword: string;
  hasError: boolean;

  constructor() { }

  ngOnInit() {
    if (this.signer.otpMode == OtpMode.CodeAndPasswordRequired) {
      this.shouldUseOtpCode = true;
      this.shouldUsePassword = true;
    }

    if (this.signer.otpMode == OtpMode.CodeRequired) {
      this.shouldUseOtpCode = true;
    }

    if (this.signer.otpMode == OtpMode.PasswordRequired) {
      this.shouldUsePassword = true;
    }

    if (this.signer.authenticationMode == AuthMode.None && (this.signer.otpMode == OtpMode.PasswordRequired || this.signer.otpMode == OtpMode.CodeAndPasswordRequired))
      this.otpPassword = this.signer.otpIdentification;
  }

  send() {
    this.submitted = true;

    if (this.shouldUsePassword || this.shouldUseOtpCode) {
      if (this.shouldUseOtpCode && this.shouldUsePassword) {

        if (!this.otpPassword || this.otpPassword == "") {
          return;
        }

        this.signer.otpMode = OtpMode.CodeAndPasswordRequired;
        this.signer.otpIdentification = this.otpPassword;
      }

      else if (this.shouldUsePassword) {
        if (!this.otpPassword || this.otpPassword == "") {
          return;
        }

        this.signer.otpMode = OtpMode.PasswordRequired;
        this.signer.otpIdentification = this.otpPassword;
      }

      else if (this.shouldUseOtpCode) {
        if (this.otpPassword != undefined && this.otpPassword != "") {
          this.hasError = true;
          return;
        }

        this.signer.otpMode = OtpMode.CodeRequired;
        this.signer.otpIdentification = "";
      }

      if (this.signer.otpMode != OtpMode.None && this.signer.authenticationMode == AuthMode.ComsignlIDP) {
        this.signer.authenticationMode = AuthMode.None;
        if (!this.shouldUsePassword) {
          this.signer.otpIdentification = "";
        }
      }

      this.updateIdentificationMode.emit();
      this.close();
    }

    else {
      this.signer.otpMode = OtpMode.None
      this.signer.otpIdentification = "";
      this.updateIdentificationMode.emit();
      this.close();
    }

    if (this.otpPassword != undefined && this.otpPassword != "") {
      this.hasError = true;
      return;
    }
  }

  close() {
    this.hide.emit();
    this.submitted = false;
    this.hasError = false;
  }
}