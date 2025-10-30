import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { DocumentSigner } from '@models/document-api/document-create-request.model';
import { AuthMode } from '@models/enums/auth-mode.enum';
import { OtpMode } from '@models/enums/otp-mode.enum';

@Component({
  selector: 'sgn-visual-identification',
  templateUrl: './visual-identification.component.html',
  styles: [
  ]
})
export class VisualIdentificationComponent implements OnInit {

  @Input() signer: DocumentSigner;
  @Output() public hide = new EventEmitter<any>();
  @Output() public updateIdentificationMode = new EventEmitter<any>();
  validId = true;
  submitted: boolean;
  hasError: boolean;
  shouldUseVisualIdentification: boolean;
  confirmId: string;
  isPassportSelected = false;

  constructor() { }

  ngOnInit(): void {

    if (this.signer.authenticationMode == AuthMode.ComsignlIDP) {
      this.confirmId = this.signer.otpIdentification;
      this.shouldUseVisualIdentification = true;
    }
  }

  isPassport(isPassport: boolean) {
    this.isPassportSelected = isPassport;
    this.confirmId = "";
  }

  send() {

    if (!this.isValidID(this.confirmId)) {
      this.validId = false;
      return;
    }

    this.validId = true;
    this.submitted = true;
    this.signer.authenticationMode = AuthMode.ComsignlIDP;
    this.signer.otpMode = OtpMode.None;
    this.signer.otpIdentification = this.confirmId;

    this.updateIdentificationMode.emit();
    this.close();
  }

  public close() {
    this.hide.emit();
    this.submitted = false;
    this.hasError = false;
  }

  public onKeyPress(e) {
    if (!this.isPassportSelected) {
      var characters = String.fromCharCode(e.which);
      if (!(/[0-9]/.test(characters))) {
        e.preventDefault();
      }
    }
  }

  public onPaste(e) {
    e.stopPropagation();
    e.preventDefault();
    if (!this.isPassportSelected) {
      let clipboardData = e.clipboardData
      let pastedData = clipboardData.getData('Text');
      if ((/^[0-9]*$/.test(e.target.value + pastedData)))
        e.target.value += pastedData;
    }
  }

  hasSpecialChar(str) {
    let regex = /[@!#$%^&*()_+\=\[\]{};':"\\|,.<>\/?]/;
    return regex.test(str);
  }

   isValidID(id: string): boolean {
    if (id == "" || id == null){
      return false;
    }

    if (this.isPassportSelected) {
      if (id.trim().length < 4 || id.trim().length > 15) {
        return false;
      }

      if (this.hasSpecialChar(id)) {
        return false;
      }

      return true;
    }

    id = String(id).trim();

    if (id.length > 9 || isNaN(Number(id))){
      return false;
    }

    id = id.length < 9 ? ("00000000" + id).slice(-9) : id;
    
    return Array.from(id, Number).reduce((counter, digit, i) => {
      const step = digit * ((i % 2) + 1);
      return counter + (step > 9 ? step - 9 : step);
    }) % 10 === 0;
  }
}