import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from "@angular/core";
import { Router } from "@angular/router";
import { FLOW_STEP } from "@models/enums/flow-step.enum";
import { FileType, UploadRequest } from "@models/template-api/upload-request.model";
import { SharedService } from "@services/shared.service";
import { SelfSignApiService } from '@services/self-sign-api.service';
import { Errors } from '@models/error/errors.model';
import { AlertLevel, IAppState } from '@state/app-state.interface';
import { Store } from '@ngrx/store';
import * as styleActions from "@state/actions/style.actions";
import { Subscription } from 'rxjs';

@Component({
    selector: "sgn-self-sign-upload",
    templateUrl: "self-sign-upload.component.html",
})
export class SelfSignUploadComponent implements OnInit, OnDestroy {

    @ViewChild("fileInput", { static: true })
    public el: ElementRef;
    public FileType = FileType;
    public fileType: FileType = FileType.NONE;
    public file: any = null;
    public busy: boolean = false;
    private createSub: Subscription;

    constructor(private selfSignApiService: SelfSignApiService, private router: Router, private sharedService: SharedService, private store: Store<IAppState>) {
        this.sharedService.setFlowState("selfsign", FLOW_STEP.SELF_SIGN_UPLOAD);
    }

    public ngOnInit() {
        this.store.dispatch(new styleActions.StyleHeaderClassesAction({ Classes: "ws_is-not-fixed" }));
    }

    public fileDropped() {
        if (this.el.nativeElement.files.length > 0) {

            this.file = this.el.nativeElement.files[0];

            if (this.file.type === "application/pdf") {
                this.fileType = FileType.PDF;
            }
            else if (this.file.type.startsWith("image")) {
                this.fileType = FileType.IMAGE;
            }

            else if (this.file.type === "application/vnd.openxmlformats-officedocument.wordprocessingml.document") {
                this.fileType = FileType.DOCX;
            }
        }
        
        else {
            this.file = null;
        }
    }

    public upload() {
        if (this.file) {
            this.busy = true;
            const reader = new FileReader();
            reader.readAsDataURL(this.file);
            reader.onload = () => {
                const uploadRequest = new UploadRequest();

                uploadRequest.Name = this.file.name;
                uploadRequest.Base64File = reader.result.toString()
                this.sharedService.setBusy(true, "DOCUMENT.UPLOADING");

                this.createSub = this.selfSignApiService.createSelfSignDocument(uploadRequest)
                    .subscribe((doc) => {
                        this.sharedService.setFlowState("selfsign", FLOW_STEP.SELF_SIGN_PLACE_FIELDS);
                        this.router.navigate(["dashboard", "selfsignfields", `${doc.documentCollectionId}`, `${doc.documentId}`]);
                        this.sharedService.setBusy(false);
                    }, (error) => {
                        this.sharedService.setBusy(false);
                        this.sharedService.setErrorAlert(new Errors(error.message));
                        this.busy = false;
                    }, () => {
                        this.busy = false;
                        this.sharedService.setBusy(false);
                    });
            };
        } 
        
        else {
            this.sharedService.setTranslateAlert("Please select a file", AlertLevel.ERROR);
        }
    }

    public cancel() {
        this.router.navigateByUrl(this.sharedService.getBackUrl());
    }

    public ngOnDestroy() {
        if (this.createSub)
            this.createSub.unsubscribe();
    }
}