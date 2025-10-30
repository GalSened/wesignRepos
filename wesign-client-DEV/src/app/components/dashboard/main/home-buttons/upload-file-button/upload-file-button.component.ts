import { Component, ElementRef, HostListener, Input, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { User } from '@models/account/user.model';
import { UploadRequest } from '@models/template-api/upload-request.model';
import { SharedService } from '@services/shared.service';
import { UserApiService } from '@services/user-api.service';
import { AlertLevel, IAppState } from '@state/app-state.interface';
import { environment } from "../../../../../../environments/environment"
import { Store } from "@ngrx/store";
import * as selectActions from "@state/actions/selection.actions";
import * as documentActions from "@state/actions/document.actions";

@Component({


  selector: 'sgn-upload-file-button',
  templateUrl: './upload-file-button.component.html',
  styles: []
})
export class UploadFileButtonComponent implements OnInit {

  public user: User;
  public tiny = environment.tiny;
  public busy: boolean = false;
  public files: any = null;

  @ViewChild("fileInput") fileInput: ElementRef;
  @Input() multipleFiles: boolean = true;

  constructor(
    private sharedService: SharedService,
    private store: Store<IAppState>,
    private userApiService: UserApiService,
    private router: Router,
  ) { }

  ngOnInit() {
  }

  public async fileDropped() {

    if (this.validateMultipleFileInput()) {
      if (!this.busy && this.fileInput.nativeElement.files.length > 0) {
        this.files = this.fileInput.nativeElement.files;
        this.uploadFile();
      }
    }
  }

  public validateMultipleFileInput(): boolean {
    if (this.fileInput.nativeElement.files.length > 1 && this.multipleFiles == false) {
      this.sharedService.setTranslateAlert("TINY.INVALID_FILE_PERMISSIONS", AlertLevel.ERROR);
      return false;
    }
    if (this.fileInput.nativeElement.files.length > 5) {
      this.sharedService.setTranslateAlert("TINY.INVALID_MULTIPLE_FILES", AlertLevel.ERROR);
      return false;
    }
    return true;
  }


  public async uploadFile() {
    sessionStorage.removeItem("CONTACTS");
    sessionStorage.removeItem("SIGNERS");
    const userData = await this.userApiService.getUserProgramStatus();
    if (userData.remeiningDocs == 0 || (userData.isFreeTrial && userData.isExpired)) {
      this.sharedService.setTranslateAlert("TINY.PAYMENT.DOCUMENT_COUNT_EXPIERED", AlertLevel.ERROR);
      if (this.tiny) {
        setTimeout(() => { this.router.navigate(["dashboard", "profile", "plans"]); }, 3000);
      }
      return;
    }
    if (this.files) {
      let fileCount = this.files.length;
      this.busy = true;
      this.sharedService.setBusy(true, "DOCUMENT.UPLOADING");
      let fileUploadRequests: UploadRequest[] = []
      let filesloaded = 0;
      for (let i = 0; i < fileCount; i++) {

        let reader = new FileReader();

      reader.onload = () => {
        const uploadRequest = new UploadRequest();
          let array = this.files[i].name.split(".");
        let name = "";
        for (let index = 0; index < array.length - 1; index++) {
          name += array[index];
          if (index != array.length - 2) {
            name += '.'
          }
        }
        uploadRequest.Name = name;
        uploadRequest.Base64File = reader.result.toString();
          if (this.files.length > 1)
            uploadRequest.IsOneTimeUseTemplate = true;
          fileUploadRequests.push(uploadRequest)
          filesloaded++;
          if (filesloaded == fileCount) {
            this.store.dispatch(new selectActions.ClearTemplateSelectionAction({}));
            this.store.dispatch(new documentActions.SetMultipleFilesUploadRequestAction({ fileUploadRequests: { Requests: fileUploadRequests, Name: fileUploadRequests[0].Name.slice(0, 50) } }));
            if (fileUploadRequests.length == 1) {
              this.store.dispatch(new documentActions.SetFileUploadRequestAction({ fileUploadRequest: fileUploadRequests[0] }))
            }
            this.sharedService.setBusy(false);
            this.busy = false;
            this.router.navigate(["dashboard", "selectsigners"]);
            this.sharedService.setBusy(false);
            this.store.dispatch(new documentActions.SetDocumentName({ CurrentDocumentName: name }));

          }
        }
        
        reader.readAsDataURL(this.files[i]);
      }



    } else {
      this.busy = false;
      this.router.navigate(["dashboard", "selectsigners"]);
      this.sharedService.setTranslateAlert("GLOBAL.PLEASE_SELECT_FILE", AlertLevel.ERROR);
    }
  }



  @HostListener('dragover', ['$event']) OnDragOver(evt) {
    evt.preventDefault();
    evt.stopPropagation();
    //console.log("Drag over")
  }

  @HostListener('dragleave', ['$event']) OnDragLeave(evt) {
    evt.preventDefault();
    evt.stopPropagation();
    //console.log("Drag Leave")
  }

  @HostListener('drop', ['$event']) OnDrop(evt) {
    evt.preventDefault();
    evt.stopPropagation();
    let files = evt.dataTransfer.files;
    if (files.length > 0) {
      this.files = files;
      this.uploadFile();
    }
  }
}
