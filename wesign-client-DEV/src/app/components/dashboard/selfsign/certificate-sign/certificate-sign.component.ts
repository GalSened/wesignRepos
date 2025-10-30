import { AfterViewInit, Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Errors } from '@models/error/errors.model';
import { Signer1FileSiging } from '@models/template-api/upload-request.model';
import { Store } from '@ngrx/store';
import { SelfSignApiService } from '@services/self-sign-api.service';
import { SharedService } from '@services/shared.service';
import { StateProcessService } from '@services/state-process.service';
import { AppState, IAppState } from '@state/app-state.interface';
import { Observable, of } from 'rxjs';
import * as documentActions from "@state/actions/document.actions";
import { Signer1Credential, SignerAuthentication } from '@models/self-sign-api/signer-authentication.model';
import * as appActions from "@state/actions/app.actions";
import { TranslateService } from '@ngx-translate/core';
import { catchError, map, switchMap } from 'rxjs/operators';
import { UserApiService } from '@services/user-api.service';

@Component({
  selector: 'sgn-certificate-sign',
  templateUrl: './certificate-sign.component.html',
  styles: []
})
export class CertificatesignComponent implements OnInit, AfterViewInit {

  public name: string;
  public state$: Observable<AppState>;
  private signer1FileSiging: Signer1FileSiging;
  public isBusy: boolean = false;
  public showCertInput: boolean = false;
  public certId: string = "";
  public password: string = "";
  public isEmptyCertId: boolean = false;
  public isEmptyPassword: boolean = false;
  public eyeIcon: string = "eye";

  constructor(private store: Store<IAppState>, private translate: TranslateService, private selfSignApiService: SelfSignApiService,
    private userApiService: UserApiService, private sharedService: SharedService, private stateService: StateProcessService, private router: Router) { }

  ngOnInit() {
    this.state$ = this.stateService.getState();
    this.state$.subscribe(
      (state: AppState) => {
        this.signer1FileSiging = state.signer1FileSiging;
        this.name = this.limitFileName(this.signer1FileSiging.FileName);
        this.signer1FileSiging.Signer1Credential = state.SelfSignSignerAuth;
        //this.showCertInput = true;
        this.showCertInput = this.userApiService.authToken.trim() == '';
      });
  }

  ngAfterViewInit(): void {
    this.password = "";
    this.certId = "";
  }

  public SignFile() {
    if ((this.password == "" || this.certId == "") && (this.signer1FileSiging.Signer1Credential == null)) {
      this.isEmptyPassword = this.password == "";
      this.isEmptyCertId = this.certId == "";
      return
    }

    if (this.password != "" && this.certId != "") {
      this.signer1FileSiging.Signer1Credential = new Signer1Credential();
      this.signer1FileSiging.Signer1Credential.certificateId = this.certId;
      this.signer1FileSiging.Signer1Credential.password = this.password;
    }

    this.isBusy = true;
    this.sharedService.setBusy(true, "DOCUMENT.SIGNING");

    let auth = new Signer1Credential();
    auth.certificateId = this.certId;
    auth.password = this.password;
    auth.signerToken = this.signer1FileSiging.Signer1Credential.signerToken

    let request = new SignerAuthentication();
    request.signer1Credential = auth;

    this.selfSignApiService.verifySigner1Credential(request).pipe(
      map(response1 => response1)
      , switchMap((editedResponse1) => {
        return this.selfSignApiService.SignUsingSigner1(this.signer1FileSiging)
      }),
      catchError(error => {
        this.isBusy = false;
        this.sharedService.setBusy(false);
        let ex = new Errors(error.error);
        let errorMessage = this.translate.instant(`SERVER_ERROR.${ex.errorCode}`);
        this.sharedService.setErrorAlert(errorMessage);
        return of({ results: null });
      })
    ).subscribe(
      (data: any) => {
        if (data.status != 200 && data.results == null) {
          return;
        }
        let fn = data.headers.get("x-file-name");
        const filename = decodeURIComponent(fn) ? decodeURIComponent(fn) : "setup";
        const blob = new Blob([data.body], { type: "application/octet-stream" });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.target = "_blank";
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        this.isBusy = false;
        this.sharedService.setBusy(false);
        this.store.dispatch(new documentActions.SetCertificateFileUploadRequestAction({ signer1FileSiging: null }));
        if (this.password != "" && this.certId != "") {
          let auth = new Signer1Credential();
          auth.certificateId = this.certId;
          auth.password = this.password;
          this.store.dispatch(new appActions.SetSignerAuthAction({ signerAuth: auth }));
        }
        this.router.navigate(["dashboard"]);
      },
      (err) => {
        this.isBusy = false;
        let ex = this.sharedService.convertArrayBufferToErrorsObject(err.error);
        this.sharedService.setErrorAlert(ex);
        this.sharedService.setBusy(false);
      });
  }

  private limitFileName(fileName: string, maxLength: number = 50) {
    // Extract the file extension
    const fileExtension = fileName.substring(fileName.lastIndexOf('.'));

    // Ensure there's an extension and that it's valid
    if (fileExtension && fileExtension.length > 0) {
      // Get the part of the file name before the extension
      const baseFileName = fileName.substring(0, fileName.lastIndexOf('.'));

      // If the base file name is longer than the max length minus the extension length
      if (baseFileName.length > maxLength - fileExtension.length) {
        // Truncate the base file name to fit within the maxLength
        return baseFileName.substring(0, maxLength - fileExtension.length) + fileExtension;
      }
    }

    // If no truncation needed, return the original file name
    return fileName;
  }
}