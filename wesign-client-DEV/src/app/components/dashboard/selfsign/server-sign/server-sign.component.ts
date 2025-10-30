import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Errors } from '@models/error/errors.model';
import { Signer1Credential, SignerAuthentication } from '@models/self-sign-api/signer-authentication.model';
import { SignatureField } from '@models/template-api/page-data-result.model';
import { Store } from '@ngrx/store';
import { TranslateService } from '@ngx-translate/core';
import { SelfSignApiService } from '@services/self-sign-api.service';
import { SharedService } from '@services/shared.service';
import * as appActions from "@state/actions/app.actions";
import { IAppState } from '@state/app-state.interface';

@Component({
  selector: 'sgn-server-sign',
  templateUrl: './server-sign.component.html',
  styles: []
})
export class ServerSignComponent implements OnInit {

  public certId: string = "";
  public password: string = "";
  public signature: SignatureField = new SignatureField();
  public isEmptyCertId: boolean;
  public isEmptyPassword: boolean;
  public eyeIcon: string = "eye";
  @Input() public errorMessage: string = "";
  @Input() public showErrorMessage = false;
  @Output("parentFun") parentFun: EventEmitter<any> = new EventEmitter();
  @Output() public hide = new EventEmitter<any>();
  isBusy: boolean;

  constructor(private store: Store<IAppState>, private translate: TranslateService,
    private selfSignApiService: SelfSignApiService, private sharedService: SharedService) { }

  ngOnInit(): void {
    this.errorMessage = this.errorMessage != "" ? this.errorMessage : this.translate.instant('ERROR.INPUT.1');
  }

  showServerCredentialForm(): boolean {
    return true;
  }

  cancel() {
    this.hide.emit();
  }

  continue() {
    if (this.password == "" || this.certId == "") {
      this.isEmptyPassword = this.password == "";
      this.isEmptyCertId = this.certId == "";
      this.showErrorMessage = true;
      return;
    }

    let auth = new Signer1Credential();
    auth.certificateId = this.certId;
    auth.password = this.password;

    let request = new SignerAuthentication();
    request.signer1Credential = auth;

    this.sharedService.setBusy(true, "GLOBAL.LOADING")

    this.isBusy = true;

    this.selfSignApiService.verifySigner1Credential(request).subscribe(
      data => {
        this.store.dispatch(new appActions.SetSignerAuthAction({ signerAuth: auth }));
        this.hide.emit();
        this.isBusy = false;
        this.sharedService.setBusy(false);
        this.sharedService.setSuccessAlert("SIGNERS.VALID_CREDENTIAL");
      },
      error => {
        this.isBusy = false;
        this.sharedService.setBusy(false);
        let ex = new Errors(error.error);
        //if (ex.errorCode == 2 ) {            
        this.showErrorMessage = true;
        this.errorMessage = this.translate.instant(`SERVER_ERROR.${ex.errorCode}`);
        // } else {
        //     this.sharedService.scrollIntoInvalidField(ex);
        //     this.sharedService.setErrorAlert(ex);
        // }
      }
    )
  }
}