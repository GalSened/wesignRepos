import * as selectActions from "@state/actions/selection.actions";
import { AfterViewInit, Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { UploadRequest, UploadRequests } from '@models/template-api/upload-request.model';
import { Store } from '@ngrx/store';
import { SharedService } from '@services/shared.service';
import { StateProcessService } from '@services/state-process.service';
import { AlertLevel, AppState, IAppState } from '@state/app-state.interface';
import { forkJoin, Observable, of, Subscription } from 'rxjs';
import { environment } from "../../../../../environments/environment";
import * as documentActions from "@state/actions/document.actions";
import { NgModel } from '@angular/forms';
import { TemplateApiService } from '@services/template-api.service';
import { catchError, switchMap, take } from 'rxjs/operators';
import { Errors } from '@models/error/errors.model';
import { FLOW_STEP } from '@models/enums/flow-step.enum';
import { Signer } from '@models/document-api/signer.model';
import { TranslateService } from '@ngx-translate/core';
import { SelfSignApiService } from '@services/self-sign-api.service';
import { SignMode } from '@models/enums/sign-mode.enum';
import { DocumentApiService } from '@services/document-api.service';
import { TemplateInfo } from '@models/template-api/template-info.model';
import { UploadResult } from '@models/template-api/upload-result.model';
import { DuplicateTemplateResult } from '@models/template-api/duplicate-template-result-model';
import { PageField } from '@models/template-api/page-data-result.model';
import { GroupAssignService } from '@services/group-assign.service';
import * as fieldsActions from "@state/actions/fields.actions";

@Component({
    selector: 'sgn-signers',
    templateUrl: 'signers.component.html'
})

export class SignersComponent implements OnInit, OnDestroy, AfterViewInit {

    state$: Observable<AppState>;
    tiny = environment.tiny;
    @ViewChild("fileInput")
    fileInput: ElementRef;
    @ViewChild("fullName")
    fullName: NgModel;
    @ViewChild("email")
    email: NgModel;
    @ViewChild("phone")
    phone: NgModel;
    telOptions: any;
    phoneExt = "1";
    file: any = null;
    busy = false;
    fullNameData = "";
    emailPhoneData = "";
    useEmail = true;
    private _fileName = '';
    createSub: Subscription;
    isTemplate: boolean;
    validName = true;
    validEmailPhone = true;
    isDocNameSetBefore = false;
    templateInfo: TemplateInfo[];
    multidocs = false;

    constructor(
        private stateService: StateProcessService,
        private store: Store<IAppState>,
        private sharedService: SharedService,
        private router: Router,
        private templateApiService: TemplateApiService,
        private translate: TranslateService,
        private selfSignApiService: SelfSignApiService,
        private documentApi: DocumentApiService,
        private groupAssignService: GroupAssignService,
    ) {
        // Check if there is a renamed file name to use
        this.state$ = this.stateService.getState();
        this.state$.subscribe(state => {
            if (state.CurrentDocumentName) {
                this._fileName = state.CurrentDocumentName;
            }
        });
    }

    get fileName(): string {
        return this._fileName;
    }
      
    set fileName(value: string) {
        this._fileName = value;
        this.store.dispatch(new documentActions.SetDocumentName({ CurrentDocumentName: value }));
    }

    ngAfterViewInit(): void {
        this.store.dispatch(new documentActions.SetDocumentName({ CurrentDocumentName: this.fileName }));
    }

    ngOnInit() {

        this.sharedService.getSupportedCountries().subscribe((x) => {
            this.telOptions = x;
        }
        )
        this.state$ = this.stateService.getState();
        this.state$.subscribe(
            state => {
            if (!this.fileName && !this.isDocNameSetBefore) {
                    this.fileName = state.SelectedTemplates.length > 0 ? state.SelectedTemplates[0].name : state.FileUploadRequests ? state.FileUploadRequests.Name : "";
                this.isDocNameSetBefore = true;
            }
            this.isTemplate = state.SelectedTemplates.length > 0;
                this.multidocs = state.FileUploadRequests.Requests.length > 1
            }
        )
    }

    public onSignerMethodChanged(event) {
        this.useEmail = event.target.selectedIndex == 0;
        this.emailPhoneData = "";
    }
    public fileDropped() {
        if (!this.busy && this.fileInput.nativeElement.files.length > 0) {
            this.file = this.fileInput.nativeElement.files[0];

            if (this.file) {
                this.sharedService.setBusy(true, "DOCUMENT.UPLOADING");
                this.busy = true;
                const reader = new FileReader();
                reader.readAsDataURL(this.file);
                this.sharedService.setBusy(true, "DOCUMENT.UPLOADING");
                reader.onload = () => {
                    const uploadRequest = new UploadRequest();
                    this.fileName = this.file.name.slice(0, 50);
                    uploadRequest.Name = this.file.name.split(".")[0];
                    uploadRequest.Base64File = reader.result.toString();
                    this.store.dispatch(new documentActions.ClearFileUploadRequestAction({}));
                    this.store.dispatch(new documentActions.SetFileUploadRequestAction({ fileUploadRequest: uploadRequest }));
                    let uploadRequests = new UploadRequests()
                    uploadRequest.Name = uploadRequest.Name
                    uploadRequests.Requests = [uploadRequest]
                    this.store.dispatch(new documentActions.SetMultipleFilesUploadRequestAction({ fileUploadRequests : uploadRequests})
                    )

                    this.store.dispatch(new documentActions.SetDocumentName({ CurrentDocumentName: this.fileName }));
                    this.busy = false;
                    this.sharedService.setBusy(false, "DOCUMENT.UPLOADING");
                };
            } else {
                this.sharedService.setTranslateAlert("GLOBAL.PLEASE_SELECT_FILE", AlertLevel.ERROR);
                this.busy = false;
            }
        }
    }

    public nameChanged($event) {
        this.store.dispatch(new documentActions.SetDocumentName({ CurrentDocumentName: this.fileName }));
    }

    onCountryChange(obj) {
        this.phoneExt = obj.dialCode;

    }

    public ngOnDestroy(): void {
        if (this.createSub)
            this.createSub.unsubscribe();
        }

    public sign() { // Wesign lite mode
        if (!this.busy) {
            this.validName = this.validEmailPhone = true;
            if (this.fullName.control.invalid || (this.useEmail && this.email.control.invalid) || (!this.useEmail && this.phone.control.invalid)) {

                if (this.fullName.control.invalid) {
                    this.validName = false;
                }
                if ((this.useEmail && this.email.control.invalid) || (!this.useEmail && this.phone.control.invalid)) {
                    this.validEmailPhone = false;
                }

                this.sharedService.setTranslateAlert("TINY.ILLEGAL_RECIPIENT", AlertLevel.ERROR);
            }
            else {
                this.busy = true;

                this.sharedService.setBusy(true, "DOCUMENT.UPLOADING");

                let signer = new Signer();
                signer.DeliveryMeans = this.useEmail ? this.email.control.value : "+" + this.phoneExt + this.phone.control.value;
                signer.FullName = this.fullName.control.value;

                this.store.dispatch(new documentActions.SetSigners({ Signers: [signer] }));

                this.createSub = this.state$.pipe(
                    take(1),
                    switchMap(state => {
                        if (this.fileName) {
                            state.FileUploadRequests.Name = this.fileName;
                            this.store.dispatch(new documentActions.SetMultipleFilesUploadRequestAction({ fileUploadRequests: state.FileUploadRequests }));
                        }
                        state.FileUploadRequest.IsOneTimeUseTemplate = true;
                        return this.templateApiService.upload(state.FileUploadRequest);
                    })
                ).subscribe(doc => {
                    this.sharedService.setFlowState("tinysign", FLOW_STEP.TINY_SIGN_PLACE_FIELDS);
                    this.router.navigate(["dashboard", "tinysignfields", `${doc.templateId}`]); // TODO - change url
                }, (error) => {
                    this.sharedService.setBusy(false);
                    this.sharedService.setErrorAlert(new Errors(error));
                    this.busy = false;
                }, () => {
                    this.busy = false;
                    this.sharedService.setBusy(false);
                });
            }
        }
    }

    telInputObject(obj) {
       // console.log(obj);
        obj.setCountry('in');
    }
    public signAction(mode) {
        switch (mode as SignMode) {
            case SignMode.SelfSign:
                this.selfSignAction();
                break;
            default:
                this.internalSignAction(mode);
                break;
        }
    }

    private selfSignAction() {
        this.sharedService.setBusy(true, "DOCUMENT.UPLOADING");
        let fileUploadRequest: UploadRequest = new UploadRequest();
        let templateInfo;
        let templateID = ""
        this.state$.pipe(
            take(1),
            switchMap(state => {
                if (this.isTemplate) {
                    templateInfo = state.SelectedTemplates[0];
                    templateID = state.SelectedTemplates[0].templateId;
                    return this.templateApiService.downloadTemplate(state.SelectedTemplates[0].templateId);
                }
                else {
                    state.FileUploadRequest.Name = this.fileName; // ??? 
                    fileUploadRequest = state.FileUploadRequest;
                    return of(null);
                }
            }),
            catchError(error => {
                this.sharedService.setBusy(false);
                this.sharedService.setErrorAlert(new Errors(error));
                this.busy = false;
                return of({ results: null });
            }),
            switchMap(template => {
                if (template && this.isTemplate) {
                    fileUploadRequest.Name = this.fileName;
                    let TYPED_ARRAY = new Uint8Array(template.body);
                    fileUploadRequest.SourceTemplateId = templateID;
                    const STRING_CHAR = TYPED_ARRAY.reduce((data, byte) => {
                        return data + String.fromCharCode(byte);
                    }, '');
                    let base64String = btoa(STRING_CHAR);
                    fileUploadRequest.Base64File = `data:application/pdf;base64,${base64String}`;
                }
                else {
                    this.store.dispatch(new documentActions.SetFileUploadRequestAction({ fileUploadRequest: fileUploadRequest }));
                }

                return this.selfSignApiService.createSelfSignDocument(fileUploadRequest)
            })
        ).subscribe(doc => {
            this.sharedService.setFlowState("selfsign", FLOW_STEP.SELF_SIGN_PLACE_FIELDS);
            this.router.navigate(["dashboard", "selfsignfields", `${doc.documentCollectionId}`, `${doc.documentId}`]);
            this.sharedService.setBusy(false);
        }, (error) => {
            this.sharedService.setBusy(false);
            this.sharedService.setErrorAlert(new Errors(error));
            this.busy = false;
        }, () => {
            this.busy = false;
        });
    }

    private internalSignAction(signMode: SignMode) {
        this.sharedService.setBusy(true, "DOCUMENT.UPLOADING");
        let templateInfo: TemplateInfo;
        this.state$.pipe(
            take(1),
            switchMap(state => {
                if (!this.isTemplate) {
                    if (state.FileUploadRequests.Requests.length == 1) {
                        let fileUploadRequest: UploadRequest = new UploadRequest();
                        fileUploadRequest = state.FileUploadRequests.Requests[0];
                        fileUploadRequest.Name = this.fileName;
                        fileUploadRequest.IsOneTimeUseTemplate = true;
                        return this.templateApiService.upload(fileUploadRequest);
                    }
                    else {

                        let fileUploadRequests: UploadRequests = new UploadRequests();
                        fileUploadRequests = state.FileUploadRequests;
                        fileUploadRequests.Name = this.fileName;


                        let results = fileUploadRequests.Requests.map((request) => {
                            return this.templateApiService.upload(request)
                        });
                        return forkJoin(results);

                    }
                }
                else {
                    let isOneTimeUseTemplate = true;
                    let id: string;
                    let arrtest = state.SelectedTemplates.map((templateInfo) => {
                        let res = { id, isOneTimeUseTemplate };
                        res.id = templateInfo.templateId;
                        res.isOneTimeUseTemplate = true;
                        return res;
                    });
                    let duplicateRequests = arrtest.map(arra => this.templateApiService.duplicate(arra.id, arra.isOneTimeUseTemplate));
                    let responses = forkJoin(duplicateRequests);
                    return responses;
                }
            }),
            switchMap(res => {
                if (res && !this.isTemplate) {
                    if (!Array.isArray(res)) {
                        this.store.dispatch(new documentActions.ClearFileUploadRequestAction({}));
                        this.store.dispatch(new fieldsActions.ClearAllFieldsAction({}))
                        this.store.dispatch(new selectActions.ClearTemplateSelectionAction({}))
                        templateInfo = new TemplateInfo();
                        templateInfo.templateId = (<UploadResult>res).templateId.toString();
                        templateInfo.name = (<UploadResult>res).templateName;
                        this.store.dispatch(new selectActions.SelectTemplateAction({ templateInfo }));
                    }
                    else {
                        for (let i = 0; i < res.length; i++) {
                            templateInfo = new TemplateInfo();
                            templateInfo.templateId = (<UploadResult>res[i]).templateId.toString();
                            templateInfo.name = (<UploadResult>res[i]).templateName;
                            this.store.dispatch(new selectActions.SelectTemplateAction({ templateInfo }));
                        }
                    }
                } else if (res && this.isTemplate) {
                    this.groupAssignService.updateFieldsMap(new Map<string, PageField[]>());
                    this.store.dispatch(new documentActions.ClearFileUploadRequestAction({}));
                    this.store.dispatch(new fieldsActions.ClearAllFieldsAction({}))
                    this.store.dispatch(new selectActions.ClearTemplateSelectionAction({}))
                    for (let index = 0; index < (<DuplicateTemplateResult[]>res).length; index++) {
                        const element = (<DuplicateTemplateResult[]>res)[index];
                        templateInfo = new TemplateInfo();
                        templateInfo.templateId = element.newTemplateId;
                        templateInfo.name = element.name;
                        this.store.dispatch(new selectActions.SelectTemplateAction({ templateInfo }));
                    }
                }
                return of(null);
            })
        ).subscribe(doc => {
            if (signMode == SignMode.Online) {
                this.sharedService.setFlowState("onlinesign", FLOW_STEP.ONLINE_SEND_GUIDE);
            }
            if (signMode == SignMode.OrderedWorkflow || signMode == SignMode.Workflow) {
                this.sharedService.setFlowState("workflowsign", FLOW_STEP.MULTISIGN_ASSIGN);
            }

            this.sharedService.setBusy(false);
            this.router.navigate(["dashboard", "groupsign"]);
        }, (error) => {
            this.sharedService.setBusy(false);
            if (error.status == 0)
            {
                this.sharedService.setErrorAlert(new Errors(error));
            }
            else
            {
                this.sharedService.setErrorAlert(new Errors(error.error));
            }
            this.busy = false;
        }, () => {
            this.busy = false;
        });
    }


    public moveTemplate(event) {
        this.store.dispatch(new selectActions.MoveTemplateAction({ index: event.index, direction: event.direction }));
    }
}