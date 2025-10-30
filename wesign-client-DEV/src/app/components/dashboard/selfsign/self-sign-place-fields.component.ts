import { Component, OnDestroy, OnInit, ChangeDetectorRef, ViewChild, ElementRef } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { SignatureField, TextField, CheckBoxField, ChoiceField, DocumentPagesRangeResponse, DocumentPageDataResult, RadioFieldGroup } from "@models/template-api/page-data-result.model";
import { Store } from "@ngrx/store";
import * as fieldActions from "@state/actions/fields.actions";
import { FLOW_STEP } from "@models/enums/flow-step.enum";
import { SignMode } from "@models/enums/sign-mode.enum";
import { Actions } from "@ngrx/effects";
import { DocumentApiService } from "@services/document-api.service";
import { SharedService } from "@services/shared.service";
import { IAppState, AlertLevel, AppState } from "@state/app-state.interface";
import { forkJoin, Observable, Subscription } from "rxjs";
import { map, switchMap } from "rxjs/operators";
import { Errors } from '@models/error/errors.model';
import * as styleActions from "@state/actions/style.actions";
import { UpdateSelfSignRequest } from '@models/self-sign-api/update-self-sign.model';
import { DocumentOperations } from '@models/enums/document-operations.enum';
import { StateProcessService } from '@services/state-process.service';
import { SelfSignApiService } from '@services/self-sign-api.service';
import { UserApiService } from '@services/user-api.service';
import { SmartCardSigningService } from '@services/smart-card-signing.service';
import { SignatureType } from '@models/enums/signature-type.enum';
import { TranslateService } from '@ngx-translate/core';
import { Signer } from '@models/document-api/signer.model';
import { SelfSignUpdateDocumentResult } from '@models/self-sign-api/self-sign-update-document-result.model';
import { DomSanitizer } from '@angular/platform-browser';


@Component({
    selector: "sgn-self-sign-place-fields",
    templateUrl: "self-sign-place-fields.component.html"
})
export class SelfSignPlaceFieldsComponent implements OnInit, OnDestroy {

    public signers: Signer[] = [];
    public pages$: Observable<number[]>;
    public documentPages: DocumentPageDataResult[] = [];
    public collectionId: string;
    public documentId: string;
    public currentDocumentName = "";
    public mode: SignMode = SignMode.SelfSign;
    @ViewChild("elem", { static: true })
    public elem: ElementRef;
    public FLOW_STEP = FLOW_STEP;
    public zmodel: { ZoomLevel: number, Bright: boolean } = { ZoomLevel: 1, Bright: false };
    public currentFlowStep: FLOW_STEP;
    public signatureColor: string;
    private storeSelectSub: Subscription;
    private userSub: Subscription;
    private getStateSnapshotSubscription: Subscription;
    private updateSelfSignDocumentSubscription: Subscription;
    subscription: any;
    public pagesCount: number;
    showServerSign: boolean = false;
    showSignErrorMessage: boolean;
    signErrorMessage: Observable<any>;
    isDark: boolean = true;
    showOpenLink: boolean = false;
    eidasShowAlert: boolean = false;
    eidasFlowActive: boolean = false;
    public currentPage: number = 1;
    public shouldSetImageToAllSigntures: boolean = false;

    public changePageNumber(pageNumer) {
        this.currentPage = pageNumer

    }
    constructor(
        private sanitizer: DomSanitizer,
        private documentApi: DocumentApiService, private translate: TranslateService, private selfSignApiService: SelfSignApiService,
        private router: Router, private route: ActivatedRoute, private store: Store<IAppState>,
        private sharedService: SharedService, private actions: Actions, private changeDetectorRef: ChangeDetectorRef,
        private stateProcess: StateProcessService, private userApi: UserApiService, private smartCardServiceApi: SmartCardSigningService) {

        this.store.dispatch(new fieldActions.ClearAllFieldsAction({}));

        this.storeSelectSub = this.store.select<any>('appstate').subscribe((state) => {
            if (
                state.FlowStep !== FLOW_STEP.SELF_SIGN_PLACE_FIELDS &&
                state.FlowStep !== FLOW_STEP.SELF_SIGN_SIGN) {
                this.sharedService.setFlowState("selfsign", FLOW_STEP.SELF_SIGN_PLACE_FIELDS);
            } else {
                this.currentFlowStep = state.FlowStep;
            }
        });

        this.userSub = this.userApi.getCurrentUser().subscribe(u => {
            this.signatureColor = u.userConfiguration.signatureColor;
        });
    }

    public ngOnInit() {
        this.store.dispatch(new styleActions.StyleHeaderClassesAction({ Classes: "" }));
        this.route.params.pipe(
            switchMap((params) => {
                this.sharedService.setBusy(true, "GLOBAL.LOADING");
                this.collectionId = params.colid;
                this.documentId = params.docid;
                this.changeDetectorRef.detectChanges();
                return this.documentApi.pageCount(this.collectionId, this.documentId);
            }),
            map((res) => {
                this.sharedService.setBusy(false);
                this.pagesCount = res.pagesCount;
                this.currentDocumentName = res.documentName;
                let limit = res.pagesCount < 100 ? 10 : Math.ceil(res.pagesCount / 10)
                let arrSize = Math.ceil(res.pagesCount / limit);
                this.sharedService.setLoadingBanner(Math.ceil(Number(arrSize / 2)), "DOCUMENT.LOADING");
                let offset = 1;
                let arrtest = new Array(arrSize).fill(this.documentId).map(
                    (id) => {
                        let res = { id, offset, limit };
                        offset = offset + limit;
                        return res;
                    });
                let pagesRequests = arrtest.map(arra => this.documentApi.getPages(this.collectionId, this.documentId, arra.offset, arra.limit, false))

                return pagesRequests;
            }),
            switchMap(ar => forkJoin(ar))
        ).subscribe((data: DocumentPagesRangeResponse[]) => {
            data.forEach(element => {
                this.documentPages = this.documentPages.concat(element.documentPages);
                this.documentPages.forEach(page => {
                    if (page.ocrString) {
                        page.ocrHtml = this.sanitizer.bypassSecurityTrustHtml(page.ocrString || '');
                    }
                });
            });
            this.pages$ = new Observable(x => x.next(Array.from(Array(this.pagesCount).keys()).map((v) => v + 1)));
        });
    }


    public next() {
        if (!this.currentDocumentName || this.currentDocumentName.length === 0) {
            this.sharedService.setTranslateAlert("DOCUMENT.NAME_EMPTY", AlertLevel.ERROR);
            return;
        }
        // validate data, done
        let isValid = this.isValidInputs();

        if (isValid) {
            let signer1DetailsExist = false;
            let smartCardSignatureExist = this.isContainSignaturesByType(SignatureType.SmartCard);
            let isContainServerSignatures = this.isContainSignaturesByType(SignatureType.Server);
            this.stateProcess.getStateSnapshot().subscribe(
                (x: AppState) => {
                    signer1DetailsExist = x.SelfSignSignerAuth != null && ((x.SelfSignSignerAuth.certificateId != null && x.SelfSignSignerAuth.password != null) ||
                        (x.SelfSignSignerAuth.signerToken != null && x.SelfSignSignerAuth.signerToken != ""));
                    this.eidasFlowActive = x.shouldSignEidasSignatureFlow;
                }
            );

            if (isContainServerSignatures && this.eidasFlowActive) {
                this.eidasShowAlert = true;
                return;
            }

            if (isContainServerSignatures && !signer1DetailsExist) {
                this.showServerSign = true;
                return;
            }

            if (smartCardSignatureExist) {
                this.showOpenLink = true;
                return;
            }
            this.saveDocument(DocumentOperations.Close);
        }
    }

    closePopUp() {
        this.eidasShowAlert = false;
        this.showOpenLink = false;
    }

    onShouldSetImageToAllSignturesChanged(value: boolean): void {
        this.shouldSetImageToAllSigntures = value;
    }

    smartCardSigning() {
        this.showOpenLink = false;
        this.eidasShowAlert = false;
        this.saveDocument(DocumentOperations.Close);
    }

    public isValidInputs() {
        let result = true;
        this.stateProcess.getStateSnapshot().subscribe((state: AppState) => {
            let signatures = this.stateProcess.getPageFields<SignatureField>(state, SignatureField);

            signatures.forEach(element => {
                if (!element.image) {
                    let er = new Errors();
                    er.errorCode = 400;
                    this.sharedService.setErrorAlert(this.translate.instant("ERROR.INPUT.SELF_SIGN_SIGNATURE_MISSING"));
                    result = false;
                    this.scrollToElement(element);
                    return;
                }
            });

            if (!result) {
                return;
            }

            let fieldsArray = [];
            fieldsArray = fieldsArray.concat(signatures);
            fieldsArray = fieldsArray.concat(this.stateProcess.getPageFields<TextField>(state, TextField));
            fieldsArray = fieldsArray.concat(this.stateProcess.getPageFields<CheckBoxField>(state, CheckBoxField));
            fieldsArray = fieldsArray.concat(this.stateProcess.getPageFields<ChoiceField>(state, ChoiceField));
            fieldsArray = fieldsArray.concat(this.stateProcess.getPageFields<SignatureField>(state, SignatureField));
            fieldsArray = fieldsArray.concat(this.stateProcess.getRadioGroupFields(state));// state.RadioGroupNames;
            fieldsArray = this.sharedService.FixFieldWithDotsInFieldName(fieldsArray);
            let field = this.sharedService.isFieldsIntersect(fieldsArray);
            if (field != undefined) {
                result = false;
                this.store.dispatch(new fieldActions.FieldsIntersectAction({ Field: field }));
                this.sharedService.setTranslateAlert("GLOBAL.OVERLAYING_FIELDS", AlertLevel.ERROR);
                return;
            }
            if (!this.ValidateMandatoryFields(fieldsArray)) {
                result = false;
                return;
            }
        });

        return result;
    }

    public back() {
        this.documentApi.deleteDocument(this.collectionId).subscribe(
            x => {
                this.router.navigate(["dashboard", "selectsigners"]);

            },
            error => {
                this.router.navigate(["dashboard", "selectsigners"]);

            }
        );
    }

    private ValidateMandatoryFields(fieldsArray) {

        let result = true;
        if (fieldsArray === undefined || fieldsArray.length == 0) {
            return result;
        }

        let mandatory = fieldsArray.filter(x => x.mandatory == true);

        mandatory.forEach(element => {
            if (result && element instanceof CheckBoxField) {
                if (!element.isChecked) {
                    this.sharedService.setErrorAlert(this.translate.instant("SERVER_ERROR.43"));
                    this.scrollToElement(element);
                    result = false;
                }
            }

            else if (result && element instanceof ChoiceField) {
                if (element.selectedOption === undefined || element.selectedOption == "") {
                    this.sharedService.setErrorAlert(this.translate.instant("SERVER_ERROR.43"));
                    this.scrollToElement(element);
                    result = false;
                }
            }

            else if (result && element instanceof SignatureField) {
                if (element.image === undefined || element.image == "") {
                    this.sharedService.setErrorAlert(this.translate.instant("SERVER_ERROR.43"));
                    this.scrollToElement(element);
                    result = false;
                }
            }

            else if (result && (element.value === undefined || element.value == "")) {
                this.sharedService.setErrorAlert(this.translate.instant("SERVER_ERROR.43"));
                this.scrollToElement(element);
                result = false;
            }
        });

        if (result) {
            let radioGroupFields = fieldsArray.filter(x => x instanceof RadioFieldGroup);
            radioGroupFields.forEach(element => {
                if (element.radioFields !== undefined && element.radioFields.length > 0) {
                    if (result && element.radioFields[0].mandatory && (element.selectedRadioName === undefined || element.selectedRadioName == "")) {
                        this.sharedService.setErrorAlert(this.translate.instant("SERVER_ERROR.43"));
                        result = false;
                        this.scrollToElement(element);
                    }
                }
            });
        }

        return result;
    }

    private scrollToElement(element) {
        if (element instanceof RadioFieldGroup) {
            let htmlElements = document.getElementsByName(element.name);
            if (htmlElements.length > 0) {
                htmlElements[0].scrollIntoView();
                htmlElements.forEach(radioElement => {
                    radioElement.parentElement.classList.add("is-error");
                });
            }
        }

        else {
            let htmlElements = document.querySelectorAll('[data-fieldname="' + element.name + '"]');
            if (htmlElements.length > 0) {
                htmlElements[0].scrollIntoView();
                htmlElements[0].children[0].classList.add("is-error");
            }
        }
    }

    public ngOnDestroy() {
        if (this.storeSelectSub) {
            this.storeSelectSub.unsubscribe();
        }
        if (this.userSub) {
            this.userSub.unsubscribe();
        }
        if (this.subscription) {
            this.subscription.unsubscribe();
        }
        if (this.updateSelfSignDocumentSubscription) {
            this.updateSelfSignDocumentSubscription.unsubscribe();
        }
        if (this.getStateSnapshotSubscription) {
            this.getStateSnapshotSubscription.unsubscribe();
        }
    }

    private saveDocument(operation: DocumentOperations): Promise<any> { // TODO - convert to observable? maybe
        return new Promise((resolve, reject) => {
            this.getStateSnapshotSubscription = this.stateProcess.getStateSnapshot().subscribe((state: AppState) => {
                const request = new UpdateSelfSignRequest();
                request.useForAllFields = this.shouldSetImageToAllSigntures;
                request.fields.textFields = this.stateProcess.getPageFields<TextField>(state, TextField);
                request.fields.textFields = this.sharedService.FormatDateTextFromDOMDateFormat(request.fields.textFields);
                this.sharedService.removeErrorClassFromElements(request.fields.textFields);
                request.fields.checkBoxFields = this.stateProcess.getPageFields<CheckBoxField>(state, CheckBoxField);
                request.fields.choiceFields = this.stateProcess.getPageFields<ChoiceField>(state, ChoiceField);
                request.fields.radioGroupFields = this.stateProcess.getRadioGroupFields(state);// state.RadioGroupNames;
                request.fields.signatureFields = this.stateProcess.getPageFields<SignatureField>(state, SignatureField);
                request.signerAuthentication.signer1Credential = state.SelfSignSignerAuth;
                request.documentCollectionId = this.collectionId;
                request.documentId = this.documentId;
                request.operation = operation;
                request.name = this.currentDocumentName;
                this.sharedService.setBusy(true, "DOCUMENT.SIGNING")
                this.updateSelfSignDocumentSubscription = this.selfSignApiService.updateSelfSignDocument(request).subscribe(
                    (selfSignUpdateDocumentResult: SelfSignUpdateDocumentResult) => {
                        //smart card
                        if (operation == DocumentOperations.Close && request.fields.signatureFields.find(x => x.signingType == SignatureType.SmartCard)) {
                            this.documentStartSmartCardSigning(resolve, reject, selfSignUpdateDocumentResult.token, request);
                        }

                        else if (this.eidasFlowActive && operation == DocumentOperations.Close && request.fields.signatureFields.find(x => x.signingType == SignatureType.Server)) {
                            if (selfSignUpdateDocumentResult.redirectUrl) {
                                window.location.href = selfSignUpdateDocumentResult.redirectUrl;
                            }
                        }

                        else {
                            this.documentSignedSuccessfully(resolve);
                        }

                    }, (error) => {
                        this.sharedService.setBusy(false);
                        let ex = new Errors(error.error);
                        if (ex.errorCode == 2) {
                            this.showServerSign = true;
                            this.showSignErrorMessage = true;
                            this.signErrorMessage = this.translate.instant(`SERVER_ERROR.${ex.errorCode}`);
                        } else {

                            this.sharedService.scrollIntoInvalidField(ex);
                            this.sharedService.setErrorAlert(ex);
                        }
                        reject();
                    });
            });
        });
    }

    private documentStartSmartCardSigning(resolve: (value: any) => void, reject: (reason?: any) => void, token: string, request: UpdateSelfSignRequest) {
        this.subscription = this.smartCardServiceApi.getSmartCardSigningResultEvent()
            .subscribe(({ isSuccess: isSuccess, downloadLink: downloadLink }) => {
                if (isSuccess) {
                    this.documentSignedSuccessfully(resolve);
                }
                else {
                    this.sharedService.setBusy(false);
                    this.sharedService.setErrorAlert(new Errors());
                    reject();
                }
            },
                (error) => {
                    console.error('Error occurred:', error);
                    this.sharedService.setBusy(false);
                    this.sharedService.setErrorAlert(new Errors());
                    reject(error);
                });

        var fieldNameToImageCollection: { [fieldName: string]: string; } = {};
        request.fields.signatureFields.forEach((e) => {
            fieldNameToImageCollection[e.name] = e.image;
        });
        this.sharedService.setBusy(true, "DOCUMENT.SIGNING");
        this.smartCardServiceApi.sign(request.documentId, token, true);
        setTimeout(() => {
            this.sharedService.setBusy(false);
        }, 180000);
    }

    private documentSignedSuccessfully(resolve: (value: any) => void) {
        this.sharedService.setSuccessAlert("DOCUMENT.SAVED_SUCCESSFULY");
        this.sharedService.setBusy(false);
        resolve(undefined);
        this.router.navigate(["dashboard", "success", "selfsign"]);
    }

    public isContainSignaturesByType(signatureSigningType: SignatureType): boolean {
        let res = false;
        this.stateProcess.getStateSnapshot().subscribe((state: AppState) => {
            let signatureFields = this.stateProcess.getPageFields<SignatureField>(state, SignatureField);
            let serverSignature = signatureFields.find(x => x.signingType == signatureSigningType);

            res = serverSignature != null;
        });
        return res;
    }
}