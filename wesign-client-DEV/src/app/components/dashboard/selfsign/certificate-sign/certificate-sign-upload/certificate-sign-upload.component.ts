import { Component, ElementRef, EventEmitter, OnInit, Output, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { Signer1FileSiging, SigningFileType } from '@models/template-api/upload-request.model';
import { Store } from '@ngrx/store';
import { SharedService } from '@services/shared.service';
import { UserApiService } from '@services/user-api.service';
import { AlertLevel, IAppState } from '@state/app-state.interface';
import * as documentActions from "@state/actions/document.actions";

@Component({
  selector: 'sgn-certificate-sign-upload',
  templateUrl: './certificate-sign-upload.component.html',
  styles: []
})
export class CertificateSignUploadComponent implements OnInit {

  @Output() public closeCertificateSignUpload = new EventEmitter<any>();
  @ViewChild("certFileInputXML") certFileInputXML: ElementRef
  @ViewChild("certFileInputXLSX") certFileInputXLSX: ElementRef
  @ViewChild("certFileInputDOCX") certFileInputDOCX: ElementRef
  @ViewChild("certFileInputPDF") certFileInputPDF: ElementRef

  busy: any;
  file: any;
  showErrorMessage: boolean;
  errorMessage: string = "";

  constructor(private store: Store<IAppState>, private sharedService: SharedService, private userApiService: UserApiService, private router: Router) { }

  ngOnInit() {
    console.log("inside certificate-sign-upload");
  }

  cancel() {
    this.closeCertificateSignUpload.emit();
  }

  public async certFileDroppedBase(certFileInput: ElementRef, fileType: string) {
    if (!this.busy && certFileInput.nativeElement.files.length > 0) {
      this.file = certFileInput.nativeElement.files[0];
      const userData = await this.userApiService.getUserProgramStatus();

      if (userData.isExpired) {
        this.sharedService.setTranslateAlert("SERVER_ERROR.20", AlertLevel.ERROR);
        return;
      }

      if (certFileInput) {
        this.busy = true;
        this.sharedService.setBusy(true, "DOCUMENT.UPLOADING");
        const reader = new FileReader();
        reader.readAsDataURL(this.file);
        reader.onload = () => {
          const signer1FileSiging = new Signer1FileSiging();

          signer1FileSiging.FileName = this.file.name;
          signer1FileSiging.Base64File = reader.result.toString().split(",")[1];
          const re = /(?:\.([^.]+))?$/;
          let ext = re.exec(signer1FileSiging.FileName)[1];
          signer1FileSiging.SigingFileType = SigningFileType.NONE;

          let isValidFileType = this.isValidFileTypeUploaded(ext, fileType);

          if (!isValidFileType) {
            this.showErrorMessage = true;
            this.errorMessage = `ERROR.INPUT.I_FILE`;
            this.sharedService.setBusy(false);
            this.busy = false;
            return;
          }

          this.showErrorMessage = false;

          if (ext.toLocaleLowerCase() == "xml") {
            signer1FileSiging.SigingFileType = SigningFileType.XML;
          }
          else if (ext.toLocaleLowerCase() == "docx") {
            signer1FileSiging.SigingFileType = SigningFileType.WORD;
          }
          else if (ext.toLocaleLowerCase() == "xlsx" ) {
            signer1FileSiging.SigingFileType = SigningFileType.EXEL;
          }
          else if (ext.toLocaleLowerCase() == "pdf") {
            signer1FileSiging.SigingFileType = SigningFileType.PDF;
          }

          this.sharedService.setBusy(false);
          this.busy = false;

          this.store.dispatch(new documentActions.SetCertificateFileUploadRequestAction({ signer1FileSiging: signer1FileSiging }));
          this.router.navigate(["dashboard", "certsign"]);
          this.sharedService.setBusy(false);
        }

      } else {
        this.busy = false;
        this.router.navigate(["dashboard"]);
        this.sharedService.setTranslateAlert("GLOBAL.PLEASE_SELECT_FILE", AlertLevel.ERROR);
      }
    }
  }

  public async certFileDropped(fileType: string) {
    if (fileType == "XML") {
      this.certFileDroppedBase(this.certFileInputXML, fileType);
    }
    else if (fileType == "PDF") {
      this.certFileDroppedBase(this.certFileInputPDF, fileType);
    }
    else if (fileType == "XLSX") {
      this.certFileDroppedBase(this.certFileInputXLSX, fileType);
    }
    else if (fileType == "DOCX") {
      this.certFileDroppedBase(this.certFileInputDOCX, fileType);
    }
  }

  public isValidFileTypeUploaded(ext: string, fileType: string) {
    if (fileType == "XML" && ext.toLocaleLowerCase() != "xml") {
      return false;
    }
    if (fileType == "PDF" && ext.toLocaleLowerCase() != "pdf") {
      return false;
    }
    if (fileType == "XLSX" && ext.toLocaleLowerCase() != "xlsx") {
      return false;
    }
    if (fileType == "DOCX" && ext.toLocaleLowerCase() != "docx") {
      return false;
    }
    return true;
  }
}