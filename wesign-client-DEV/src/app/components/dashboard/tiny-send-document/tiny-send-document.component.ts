import { Component, OnInit, ElementRef, ViewChild, OnDestroy } from '@angular/core';
import { FileType, UploadRequest } from '@models/template-api/upload-request.model';
import { Subscription, Observable } from 'rxjs';
import { DocumentApiService } from '@services/document-api.service';
import { TemplateApiService } from '@services/template-api.service';
import { Router } from '@angular/router';
import { SharedService } from '@services/shared.service';
import { IAppState, AlertLevel } from '@state/app-state.interface';
import { Store } from '@ngrx/store';
import { FLOW_STEP } from '@models/enums/flow-step.enum';
import * as styleActions from "@state/actions/style.actions";
import { Errors } from '@models/error/errors.model';
import { UserApiService } from '@services/user-api.service';
import { User } from '@models/account/user.model';

@Component({
  selector: 'sgn-tiny-send-document',
  templateUrl: './tiny-send-document.component.html',
  styles: []
})

export class TinySendDocumentComponent implements OnInit, OnDestroy {
  private FREE_TRAIL_COMPANY_ID: string = "00000000-0000-0000-0000-000000000001";
  private canSendDocument: boolean = true;
  public user$: Observable<User>;
  constructor(private templateApiService: TemplateApiService,

    private router: Router,
    private sharedService: SharedService,
    private store: Store<IAppState>,
    private userApiService: UserApiService) {
    this.sharedService.setFlowState("tinysign", FLOW_STEP.TINY_SIGN_UPLOAD);

    this.userApiService.getCurrentUser(false).subscribe((data) => {
      this.canSendDocument = true;
      if (data.program.remainingDocumentsForMonth == 0) {
        this.canSendDocument = false;
        var date = new Date();
        const currentUTCTime = new Date(date.getTime() + date.getTimezoneOffset() * 60000);
        const expiredDate = new Date(data.program.expiredTime);
       // console.log(data.companyId);
        if ((data.companyId == this.FREE_TRAIL_COMPANY_ID) || (currentUTCTime.getTime() > expiredDate.getTime())) {
          this.router.navigate(["dashboard", "plan-payment"]);
        }
        else {
          this.sharedService.setTranslateAlert("TINY.PAYMENT.DOCUMENT_COUNT_EXPIERED", AlertLevel.ERROR);
          // this.router.navigate(["dashboard"]);
        }

      }
    });

  }

  @ViewChild("fileInput", { static: true })



  public el: ElementRef;

  public FileType = FileType;

  public fileType: FileType = FileType.NONE;

  public file: any = null;

  public busy: boolean = false;

  private createSub: Subscription;

  public ngOnInit() {
    this.store.dispatch(new styleActions.StyleHeaderClassesAction({ Classes: "ws_is-not-fixed" }));
  }


  public upload() {
    if (!this.busy) {
      if (!this.canSendDocument) {
        this.sharedService.setTranslateAlert("TINY.PAYMENT.DOCUMENT_COUNT_EXPIERED", AlertLevel.ERROR);
        return;
      }

      if (this.file) {
        this.busy = true;
        const reader = new FileReader();
        reader.readAsDataURL(this.file);
        reader.onload = () => {
          const uploadRequest = new UploadRequest();
          //uploadRequest.FileFormat = this.fileType;
          uploadRequest.Name = this.file.name;
          uploadRequest.Base64File = reader.result.toString();

          this.sharedService.setBusy(true, "DOCUMENT.UPLOADING");

          this.createSub = this.templateApiService.upload(uploadRequest)
            .subscribe((doc) => {
              this.sharedService.setFlowState("tinysign", FLOW_STEP.TINY_SIGN_PLACE_FIELDS);
              this.router.navigate(["dashboard", "tinysignfields", `${doc.templateId}`]);
              this.sharedService.setBusy(false);
            }, (error) => {
              this.sharedService.setBusy(false);
              this.sharedService.setErrorAlert(new Errors(error));
              this.busy = false;
            }, () => {
              this.busy = false;
              this.sharedService.setBusy(false);
            });
        };
      } else {
        this.sharedService.setTranslateAlert("GLOBAL.PLEASE_SELECT_FILE", AlertLevel.ERROR);
      }
    }
  }

  public cancel() {
    this.router.navigateByUrl("/dashboard");
  }

  public fileDropped() {
    if (this.el.nativeElement.files.length > 0) {
      this.file = this.el.nativeElement.files[0];
      if (this.file.type === "application/pdf") {
        this.fileType = FileType.PDF;
      } else if (this.file.type.startsWith("image")) {
        this.fileType = FileType.IMAGE;
      } else if (this.file.type === "application/vnd.openxmlformats-officedocument.wordprocessingml.document") {
        this.fileType = FileType.DOCX;
      }
    } else {
      this.file = null;
    }
  }

  public ngOnDestroy(): void {

    if (this.createSub)
      this.createSub.unsubscribe();
  }

}
