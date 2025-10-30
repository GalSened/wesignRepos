import { Component, ElementRef, EventEmitter, OnDestroy, OnInit, Output, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { Signer1FileSiging, SigningFileType, UploadRequest } from '@models/template-api/upload-request.model';
import { Store } from '@ngrx/store';
import { SharedService } from '@services/shared.service';
import { UserApiService } from '@services/user-api.service';
import { AlertLevel, IAppState } from '@state/app-state.interface';
import * as documentActions from "@state/actions/document.actions";
import { SelfSignApiService } from '@services/self-sign-api.service';
import { Subscription } from 'rxjs';
import { Errors } from '@models/error/errors.model';

@Component({
  selector: 'sgn-gov-sign-upload',
  templateUrl: './gov-sign-upload.component.html',
  styles: []
})
export class GovSignUploadComponent implements OnDestroy {

  @Output() public closeGovSign = new EventEmitter<any>();

  @ViewChild('fileInput', { static: false }) fileInput!: ElementRef<HTMLInputElement>;

  busy: any;
  file: any;
  showErrorMessage: boolean;
  errorMessage: string = "";
  private createSub: Subscription;

  constructor(private store: Store<IAppState>, private sharedService: SharedService, private userApiService: UserApiService,
    private router: Router, private selfSignApiService: SelfSignApiService) { }

  public ngOnDestroy(): void {
    if (this.createSub)
      this.createSub.unsubscribe();
  }

  cancel() {
    this.closeGovSign.emit();
  }

  async uploaded() {
    const input = this.fileInput.nativeElement;
    if (!this.busy && input.files.length > 0) {
      this.file = input.files[0];
      const userData = await this.userApiService.getUserProgramStatus();

      if (userData.isExpired) {
        this.sharedService.setTranslateAlert("SERVER_ERROR.20", AlertLevel.ERROR);
        return;
      }

      if (input) {
        this.busy = true;
        this.sharedService.setBusy(true, "DOCUMENT.UPLOADING");
        const reader = new FileReader();
        reader.readAsDataURL(this.file);
        reader.onload = () => {
          const signer1FileSiging = new Signer1FileSiging();
          const uploadRequest = new UploadRequest();
          uploadRequest.Name = signer1FileSiging.FileName = this.file.name;
          uploadRequest.Base64File = reader.result.toString();
          uploadRequest.IsOneTimeUseTemplate = true;
          signer1FileSiging.Base64File = uploadRequest.Base64File.split(",")[1];
          const re = /(?:\.([^.]+))?$/;
          let ext = re.exec(signer1FileSiging.FileName)[1];
          signer1FileSiging.SigingFileType = SigningFileType.NONE;

          this.showErrorMessage = false;

          signer1FileSiging.SigingFileType = SigningFileType.PDF;
          this.createSub = this.selfSignApiService.createSelfSignDocument(uploadRequest)
            .subscribe((doc) => {
              this.store.dispatch(new documentActions.SetCertificateFileUploadRequestAction({ signer1FileSiging: signer1FileSiging }));
              this.router.navigate(["dashboard", "govsign", `${doc.documentCollectionId}`, `${doc.documentId}`]);
              this.closeGovSign.emit();
            }, (error) => {
              this.sharedService.setBusy(false);
              this.sharedService.setErrorAlert(new Errors(error.message));
              this.busy = false;
            }, () => {
              this.busy = false;
              this.sharedService.setBusy(false);
            });
        }
      } else {
        this.busy = false;
        this.router.navigate(["dashboard"]);
        this.sharedService.setTranslateAlert("GLOBAL.PLEASE_SELECT_FILE", AlertLevel.ERROR);
      }
    }
  }
}