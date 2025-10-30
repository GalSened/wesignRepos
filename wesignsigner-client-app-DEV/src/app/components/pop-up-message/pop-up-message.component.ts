import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Errors } from 'src/app/models/error/errors.model';
import { SignaturesImagesModel } from 'src/app/models/responses/signatures-images.model';
import { ContactsApiService } from 'src/app/services/contacts-api.service';
import { StateService } from 'src/app/services/state.service';

@Component({
  selector: 'app-pop-up-message',
  templateUrl: './pop-up-message.component.html',
  styleUrls: ['./pop-up-message.component.scss']
})
export class PopUpMessageComponent implements OnInit {

  @Input() public token: string = "";
  @Input() public image: string = "";
  @Output("closePopup") closePopupFun: EventEmitter<any> = new EventEmitter();
  @Output() public closeSpinner = new EventEmitter<any>();
  @Output() public showSpinner = new EventEmitter<any>();

  showErrorMessage: boolean;
  errorMessage: any;

  constructor(private contactsApiService: ContactsApiService, private translate: TranslateService, private stateService: StateService) { }

  ngOnInit(): void {
  }

  cancel() {
    this.stateService.closeSaveSignatureForFutureUse();
  }

  saveSignature() {
    let signaturesImages = new SignaturesImagesModel();
    signaturesImages.signaturesImages = [this.image];
    this.showSpinner.emit();
    this.contactsApiService.updateSignaturesImages(this.token, signaturesImages)
      .subscribe(
        (res) => {
          this.closeSpinner.emit();
          this.stateService.closeSaveSignatureForFutureUse();
        },
        
        (err) => {
          this.closeSpinner.emit();
          this.showErrorMessage = true;
          let result = new Errors(err.error);
          this.errorMessage = this.translate.instant('SERVER_ERROR.' + result.errorCode);
        });
  }
}