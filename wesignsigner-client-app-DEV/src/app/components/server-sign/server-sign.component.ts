import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { Observable, Subscription } from 'rxjs';
import { signatureField } from 'src/app/models/pdffields/field.model';
import { AppState } from 'src/app/models/state/app-state.model';
import { StateService } from 'src/app/services/state.service';

@Component({
  selector: 'app-server-sign',
  templateUrl: './server-sign.component.html',
  styleUrls: ['./server-sign.component.scss']
})
export class ServerSignComponent implements OnInit {

  certId = "";
  password = "";
  signature = new signatureField();
  eyeIcon = "eye";
  appState: Observable<AppState>;
  state: AppState;
  isEmptyCertId: boolean;
  isEmptyPassword: boolean;
  stateSub: Subscription;
  @Input() public errorMessage = "";
  @Input() public showErrorMessage = false;
  @Output("parentFun") parentFun: EventEmitter<any> = new EventEmitter();

  constructor(private stateService: StateService, private translate: TranslateService) { }

  ngOnInit(): void {
    this.appState = this.stateService.state$;
    this.stateSub = this.appState.subscribe(x => {
      this.signature.signingType = x.selectedSignField.type;
      this.state = x;
      //this.showServerCredentialForm = x.
    });
    this.errorMessage = this.translate.instant('ERROR.INPUT.4');
  }

  showServerCredentialForm(): boolean {
    return this.stateService.showServerCredentialForm;
  }

  ngOnDestroy() {
    if (this.stateSub)
      this.stateSub.unsubscribe();
  }

  cancel() {
    this.stateService.showServerCredentialForm = false;
  }

  continue() {
    if (this.password == "" || this.certId == "") {
      this.isEmptyPassword = this.password == "";
      this.isEmptyCertId = this.certId == "";
      this.showErrorMessage = true;
      return;
    }
    this.stateService.setServerSignatureCredential({ certId: this.certId, password: this.password, authToken: "" });
    this.parentFun.emit();
    this.stateService.showServerCredentialForm = false;
  }
}