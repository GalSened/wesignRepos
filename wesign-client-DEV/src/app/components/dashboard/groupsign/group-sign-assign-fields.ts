import { AfterViewInit, Component, ElementRef, OnInit, Renderer2 } from '@angular/core';
import { Store } from '@ngrx/store';
import { IAppState, AppState, AlertLevel } from '@state/app-state.interface';
import * as styleActions from "@state/actions/style.actions";
import { Observable, of } from 'rxjs';
import { TemplateInfo } from '@models/template-api/template-info.model';
import { switchMap, map, mergeMap, combineAll, mergeAll, tap, take, concatMap, toArray } from 'rxjs/operators';
import { TemplateApiService } from '@services/template-api.service';
import { TemplatePagesResult } from '@models/template-api/template-pages-result.model';
import { SharedService } from '@services/shared.service';
import { Router } from '@angular/router';
import { FLOW_STEP } from '@models/enums/flow-step.enum';
import { DocumentApiService } from '@services/document-api.service';
import { GroupAssignService } from '@services/group-assign.service';
import { Signer } from '@models/document-api/signer.model';
import { DocumentCollectionCreateRequest, DocumentSigner } from '@models/document-api/document-create-request.model';
import { SignMode } from '@models/enums/sign-mode.enum';
import { StateProcessService } from '@services/state-process.service';
import { PDFFields, UpdateTemplateRequest } from '@models/template-api/update-template-request.model';
import { CheckBoxField, ChoiceField, DocumentFields, PageDataResult, PageField, RadioField, RadioFieldGroup, SignatureField, TemplatePagesRangeResponse, TextField } from '@models/template-api/page-data-result.model';
import { Errors } from '@models/error/errors.model';
import * as fieldActions from "@state/actions/fields.actions";
import { TranslateService } from '@ngx-translate/core';
import { Modal } from '@models/modal/modal.model';
import { DomSanitizer } from '@angular/platform-browser';

@Component({
    selector: 'sgn-group-sign-assign-fields',
    templateUrl: 'group-sign-assign-fields.html'
})

export class GroupSignAssignFieldsComponent implements OnInit, AfterViewInit {

    public pages$: Observable<number[]>;
    public pagesData: TemplatePagesResult[] = [];
    public zmodel: { ZoomLevel: number, Bright: boolean } = { ZoomLevel: 1, Bright: false };
    public documentName: string = "";
    public signers: Signer[] = [];
    nextButtonText: string;
    private flowStep: FLOW_STEP;
    public showOpenLink = false;
    public isLinkClicked = false;
    public liveLink: string;
    atLeastOneFieldExist: boolean = false;
    atLeastOneFieldAssignToEachSigner: boolean = false;
    public onlineLink = "http://localhost:4201/terms";
    pagesCount: number = 0;
    pageFields: PageField[];

    pageFieldFromStore: PageField[];
    public templatePages: PageDataResult[] = [];
    atLeastOneSigFieldAssignToEachSigner: boolean = false;

    private ABSTRICTION_TOKEN = "Signer"
    public ascribeFieldsToSignersModal: Modal = new Modal()
    public showConfirmWhenUsingMeaningOfSignature: Modal = new Modal()
    public shouldAscribeFields: boolean = false;
    private appState: AppState;

    isDark: boolean = true;
    public radioGroupFields: RadioFieldGroup[] = [];
    public showServerSign: boolean = false;
    public showSignErrorMessage: boolean;
    public signErrorMessage: Observable<any>;

    constructor(private store: Store<IAppState>, public el: ElementRef, private renderer: Renderer2, private templateApi: TemplateApiService,
        private sharedService: SharedService, private stateService: StateProcessService, private documentApi: DocumentApiService,
        private router: Router, private translate: TranslateService, private groupAssignService: GroupAssignService,
        private sanitizer: DomSanitizer) { }

    ngAfterViewInit(): void {
        if (this.groupAssignService.useMeaningOfSignature) {
            this.showConfirmWhenUsingMeaningOfSignature.showModal = true;
            this.translate.get(['SIGNERS.MEANING_OF_SIGNATURE', 'SIGNERS.MEANING_OF_SIGNATURE_POPUP_CONTENT', 'BUTTONS.CLOSE'])
                .subscribe((res: object) => {
                    let keys = Object.keys(res);
                    this.showConfirmWhenUsingMeaningOfSignature.title = res[keys[0]];
                    this.showConfirmWhenUsingMeaningOfSignature.content = res[keys[1]];
                    this.showConfirmWhenUsingMeaningOfSignature.confirmBtnText = res[keys[2]];
                });
        }
    }

    public currentPage: number = 1;
    public changePageNumber(pageNumer) {
        this.currentPage = pageNumer

    }


    ngOnInit() {
        this.translate.get(['DOCUMENT.SHOULD_ASCRIBE_FIELDS_TITLE', 'DOCUMENT.SHOULD_ASCRIBE_FIELDS_MESSAGE', 'BUTTONS.NO', 'BUTTONS.YES'])
            .subscribe((res: object) => {
                let keys = Object.keys(res);
                this.ascribeFieldsToSignersModal.title = res[keys[0]];
                this.ascribeFieldsToSignersModal.content = res[keys[1]];
                this.ascribeFieldsToSignersModal.rejectBtnText = res[keys[2]];
                this.ascribeFieldsToSignersModal.confirmBtnText = res[keys[3]];
            });


        this.store.dispatch(new styleActions.StyleHeaderClassesAction({ Classes: "" }));

        this.store.select<any>('appstate').subscribe(state => {
            this.appState = state;
            this.signers = state.Signers;
            this.nextButtonText = state.FlowStep == FLOW_STEP.ONLINE_SEND_GUIDE ? 'FORGET.SEND' : 'SIGNERS.REVIEW_DOCUMENT';
            this.flowStep = state.FlowStep;
            this.pageFieldFromStore = state.PageFields;
        });

        this.store.select<any>('appstate').pipe(

            take(1),
            map((s: AppState) =>
                s.SelectedTemplates
            ),
            mergeAll(),
            concatMap((template) => {
                this.sharedService.setBusy(true, "GLOBAL.LOADING");
                return this.templateApi.pageCount(template.templateId)
            }),
            switchMap((res) => {
                this.sharedService.setBusy(true);
                this.pagesCount += res.pagesCount;
                let limit = this.pagesCount < 100 ? 10 : Math.ceil(this.pagesCount / 10)
                let arrSize = Math.ceil(res.pagesCount / limit);
                this.sharedService.setLoadingBanner(Math.ceil(Number(arrSize / 2)), "DOCUMENT.LOADING");
                let offset = 1;
                let arrtest = new Array(arrSize).fill(res.templateId).map(
                    (id) => {
                        let res = { id, offset, limit };
                        offset = offset + limit;
                        return res;
                    });
                let pagesRequests = arrtest.map(arra => this.templateApi.getPages(arra.id, arra.offset, arra.limit))

                return pagesRequests;
            }),
            combineAll()

        ).subscribe((data: TemplatePagesRangeResponse[]) => {
            data.forEach(element => {
                this.templatePages = this.templatePages.concat(element.templatePages);
                this.templatePages.forEach(templatePage => {
                    if (templatePage.ocrString) {
                        templatePage.ocrHtml = this.sanitizer.bypassSecurityTrustHtml(templatePage.ocrString || '');
                    }
                });
                if (!this.shouldAscribeFields) {
                    element.templatePages.forEach(templatePage => {
                        this.FindAscribtionFields(templatePage.pdfFields)
                    }
                );
                }
            });

            this.pages$ = new Observable(x => x.next(Array.from(Array(this.pagesCount).keys()).map((v) => v + 1)));
            if (this.shouldAscribeFields) {
                this.ascribeFieldsToSignersModal.showModal = true
            }

        });

        let fieldsArray = sessionStorage.getItem("FIELDS_ARRAY");
        sessionStorage.removeItem("FIELDS_ARRAY");
        if (fieldsArray != null) {
            if (fieldsArray.length > 0) {
                this.store.dispatch(new fieldActions.ShouldLoadFields({ shouldLoad: false }));
                this.loadSessionStorageFields(fieldsArray);
            }
        }


    }

    private loadSessionStorageFields(fieldsJson: string) {
        this.pageFields = JSON.parse(fieldsJson);
        let count = 0;
        this.store.dispatch(new fieldActions.ClearAllFieldsAction({}))

        this.pageFields.forEach(x => {
            this.addField(x, count);
            count++;
        })
    }

    private addField(x: PageField, count: number) {
        if (x.description.startsWith("Choice")) {
            this.store.dispatch(
                new fieldActions.AddPageFieldAction({ PageField: Object.assign(new ChoiceField(), x as ChoiceField) }));
            this.groupAssignService.addFieldFromStorage(Object.assign(new ChoiceField(), x as ChoiceField), count == 0 ? true : false);
        }
        else if (x.description.startsWith("Signature")) {
            x.mandatory = true;
            this.store.dispatch(
                new fieldActions.AddPageFieldAction({ PageField: Object.assign(new SignatureField(), x as SignatureField) }));
            this.groupAssignService.addFieldFromStorage(Object.assign(new SignatureField(), x as SignatureField), count == 0 ? true : false);
        }
        else if (x.description.startsWith("Checkbox")) {
            this.store.dispatch(
                new fieldActions.AddPageFieldAction({ PageField: Object.assign(new CheckBoxField(), x as CheckBoxField) }));
            this.groupAssignService.addFieldFromStorage(Object.assign(new CheckBoxField(), x as CheckBoxField), count == 0 ? true : false);
        }
        else if (x.description.startsWith("Radio")) {
            this.store.dispatch(
                new fieldActions.AddPageFieldAction({ PageField: Object.assign(new RadioField(), x as RadioField) }));
            this.groupAssignService.addFieldFromStorage(Object.assign(new RadioField(), x as RadioField), count == 0 ? true : false);
        }
        else { // Text Field
            this.store.dispatch(
                new fieldActions.AddPageFieldAction({ PageField: Object.assign(new TextField(), x as TextField) }));
            this.groupAssignService.addFieldFromStorage(Object.assign(new TextField(), x as TextField), count == 0 ? true : false);
        }
    }

    public back() { // from edit document
        this.router.navigate(["/dashboard", "selectsigners"])
    }

    public next($event) {
        this.stateService.getStateSnapshot().subscribe((store: AppState) => {
            let fieldsArray = []
            fieldsArray = fieldsArray.concat(this.stateService.getPageFields<TextField>(store, TextField));
            if (!this.isTextFieldsGroupsBelongToSignleSigner(fieldsArray)) {
                return;
            }
            fieldsArray = fieldsArray.concat(this.stateService.getPageFields<CheckBoxField>(store, CheckBoxField));
            fieldsArray = fieldsArray.concat(this.stateService.getPageFields<ChoiceField>(store, ChoiceField));
            let radioGroupFields = this.stateService.getRadioGroupFields(store);// state.RadioGroupNames;
            fieldsArray = fieldsArray.concat(this.stateService.getPageFields<SignatureField>(store, SignatureField))

            radioGroupFields.forEach(x => {
                fieldsArray = fieldsArray.concat(x.radioFields)
            }
            );

            fieldsArray = this.sharedService.FixFieldWithDotsInFieldName(fieldsArray);
            let field = this.sharedService.isFieldsIntersect(fieldsArray);
            if (field != undefined) {
                this.store.dispatch(new fieldActions.FieldsIntersectAction({ Field: field }));
                this.sharedService.setTranslateAlert("GLOBAL.OVERLAYING_FIELDS", AlertLevel.ERROR);
                return;

            }

            let fields_json = JSON.stringify(fieldsArray);
            sessionStorage.setItem("FIELDS_ARRAY", fields_json);

            this.sendDoument();
        });
    }


    private isTextFieldsGroupsBelongToSignleSigner(fieldsArray) {
        let fieldsDes = [];
        fieldsArray.forEach(element => {
            if (!fieldsDes.includes(element.description)) {
                fieldsDes.push(element.description);
            }
        });
        let textFields = new Map<string, TextField[]>();
        fieldsArray.forEach(element => {
            if (textFields[(<TextField>element).description] == undefined) {
                textFields[(<TextField>element).description] = [element];
            }
            else {
                textFields[(<TextField>element).description].push(element);
            }
        });
        let isTextFieldGroupBelongToSignleSigner = true;
        let fieldName = "";
        fieldsDes.forEach((description) => {
            let fields = textFields[description];
            let signerId = fields[0].signerId;
            fields.forEach(textField => {
                if (textField.signerId != signerId) {
                    isTextFieldGroupBelongToSignleSigner = false;
                    fieldName = textField.description;
                    return;
                }
            });
        });
        if (!isTextFieldGroupBelongToSignleSigner) {
            let msg: string = this.translate.instant(`ERROR.INPUT.FIELDS_GROUP_TO_SINGLE_SIGNER`);
            msg = msg.replace("FIELD_DESCRIPTION", fieldName);
            this.sharedService.setErrorAlert(msg, true);
            return false;
        }
        return true;
    }

    private sendDoument() {
        this.sharedService.setBusy(true, "GLOBAL.LOADING");
        // * Save templates
        const fields: PDFFields = new PDFFields();
        const collectionRequest = new DocumentCollectionCreateRequest();
        let signersWithFieldsCount = 0;
        let signersWithSigFieldsCount = 0;

        this.groupAssignService.groupFieldsObserv.subscribe(
            (data: Map<string, PageField[]>) => {
                data.forEach((val, key) => {
                    if (key != undefined && val.length > 0) {
                        this.atLeastOneFieldExist = true;
                        signersWithFieldsCount++;
                        for (let index = 0; index < val.length; index++) {
                            const field = val[index];
                            if (field instanceof SignatureField) {
                                signersWithSigFieldsCount++;
                                break;
                            }
                        }
                    }
                }
                );
            }
        );

        this.stateService.getState()
            .pipe(
                take(1),
                tap((state: AppState) => {
                    this.atLeastOneFieldAssignToEachSigner = state.Signers.length == signersWithFieldsCount;
                    this.atLeastOneSigFieldAssignToEachSigner = state.Signers.length == signersWithSigFieldsCount;
                    fields.textFields = this.stateService.getPageFields<TextField>(state, TextField);
                    this.sharedService.removeErrorClassFromElements(fields.textFields);
                    fields.checkBoxFields = this.stateService.getPageFields<CheckBoxField>(state, CheckBoxField);
                    fields.choiceFields = this.stateService.getPageFields<ChoiceField>(state, ChoiceField);
                    fields.radioGroupFields = this.stateService.getRadioGroupFields(state);// state.RadioGroupNames;
                    fields.signatureFields = this.stateService.getPageFields<SignatureField>(state, SignatureField);
                    this.radioGroupFields = fields.radioGroupFields;
                    if (state.FlowStep == FLOW_STEP.ONLINE_SEND_GUIDE) {
                        collectionRequest.documentMode = SignMode.Online;
                        collectionRequest.documentName = state.CurrentDocumentName;
                        collectionRequest.shouldSignUsingSigner1AfterDocumentSigningFlow = state.shouldSignUsingSigner1AfterDocumentSigningFlow;
                        collectionRequest.shouldEnableMeaningOfSignature = false;
                        collectionRequest.templates = (<TemplateInfo[]>state.SelectedTemplates).map(
                            (templateInfo) => templateInfo.templateId
                        );
                        collectionRequest.signers = state.Signers.reduce((acc, val) => {
                            let ds = new DocumentSigner();
                            ds.contactName = val.FullName;
                            ds.contactMeans = val.DeliveryMeans;
                            ds.sendingMethod = val.DeliveryMethod;
                            ds.phoneExtension = val.DeliveryExtention;
                            return acc.concat(ds);
                        }, []);
                    }
                }),
                map((s: AppState) => s.SelectedTemplates),
                mergeAll(),
                mergeMap(template => {
                    const request = new UpdateTemplateRequest();
                    request.id = template.templateId;
                    request.name = template.name;
                    request.fields.textFields = fields.textFields.filter(f => f.templateId == template.templateId);
                    request.fields.checkBoxFields = fields.checkBoxFields.filter(f => f.templateId == template.templateId);
                    request.fields.choiceFields = fields.choiceFields.filter(f => f.templateId == template.templateId);
                    fields.radioGroupFields.forEach(x => {
                        var items = x.radioFields.filter(item => item.templateId == template.templateId)

                        if (items && items.length > 0) {
                            let group = new RadioFieldGroup();
                            group.name = x.name;
                            group.selectedRadioName = x.selectedRadioName;
                            group.radioFields = items;
                            request.fields.radioGroupFields.push(group);
                        }

                    });

                    request.fields.signatureFields = fields.signatureFields.filter(f => f.templateId == template.templateId);

                    return this.templateApi.update(request);
                }),
                toArray(),
                switchMap(_ => {
                    if (this.flowStep == FLOW_STEP.ONLINE_SEND_GUIDE)
                        return this.documentApi.documentCreate(collectionRequest);

                    return of(null);
                }),
            )
            .subscribe(result => {
                this.sharedService.setBusy(false);

                if (this.flowStep == FLOW_STEP.ONLINE_SEND_GUIDE) {
                    if (!result) {
                        throw 'no data';
                    }
                    this.showOpenLink = true;
                    let senderLink = result.signerLinks.find(x => x.link.includes("sender"));
                    this.liveLink = senderLink.link;
                } else if (this.flowStep == FLOW_STEP.MULTISIGN_ASSIGN) {
                    // if (!this.atLeastOneFieldExist || !this.atLeastOneFieldAssignToEachSigner) {
                    //     this.sharedService.setTranslateAlert("DOCUMENT.NO_FIELDS", AlertLevel.ERROR);
                    // }else {
                    //     this.router.navigate(["/dashboard", "selectsigners", "review"]);
                    // }
                    this.router.navigate(["/dashboard", "selectsigners", "review"]);
                }
            }, (error) => {
                let ligalRadioGroup = true;
                if (this.radioGroupFields) {
                    // && this.radioGroupFields != []
                    this.radioGroupFields.forEach(x => {
                        if (x.radioFields.length == 1) {
                            ligalRadioGroup = false;
                        }
                    });
                }
                this.sharedService.setBusy(false);
                if (!ligalRadioGroup) {
                    this.sharedService.setTranslateAlert("GLOBAL.SHOULD_CONTAIN_TWO_ELEMENTS", AlertLevel.ERROR);
                }
                else {
                    let ex = new Errors(error.error);
                    this.sharedService.scrollIntoInvalidField(ex);
                    this.sharedService.setErrorAlert(ex);
                }
            }, () => { this.sharedService.setBusy(false); });
    }



    public openLiveLink() {
        window.open(this.liveLink);
        this.GoToDeshboard();
    }
    public GoToDeshboard() {
        this.showOpenLink = false;
        this.clearSession();
        this.router.navigate(["/dashboard"]);
    }
    public getPageNumber(pagesCount: number, index: number) {
        let pagenum = pagesCount - (index % pagesCount);
        return pagenum;
    }

    private clearSession() {
        sessionStorage.removeItem("CONTACTS");
        sessionStorage.removeItem("SIGNERS");
        sessionStorage.removeItem("FIELDS_ARRAY");
    }


    doAscribeFields() {

        this.pageFieldFromStore.forEach(element => {
            element instanceof RadioField ? this.ascribeFieldToSigner(element, element.groupName, true) : this.ascribeFieldToSigner(element, element.description, false)

        });
        this.ascribeFieldsToSignersModal.showModal = false;
    }



    cancelAscribeFields() {
        this.ascribeFieldsToSignersModal.showModal = false;
    }

    private FindAscribtionFields(fields: DocumentFields): void {

        fields.checkBoxFields.forEach(cbf => this.ascribeFieldToSigner(cbf, cbf.description, false))
        fields.choiceFields.forEach(chf => this.ascribeFieldToSigner(chf, chf.description, false))
        fields.radioGroupFields.forEach(rgf => rgf.radioFields.forEach(rf => this.ascribeFieldToSigner(rf, rgf.name, true)))
        fields.signatureFields.forEach(snf => this.ascribeFieldToSigner(snf, snf.description, false))
        fields.textFields.forEach(tf => this.ascribeFieldToSigner(tf, tf.description, false))
    }

    private ascribeFieldToSigner(field: PageField, fieldDescriptor: string, isRadio: boolean): void {


        if (fieldDescriptor.startsWith(this.ABSTRICTION_TOKEN)) {
            let numberStartIndex = this.ABSTRICTION_TOKEN.length

            let singleDigit = parseInt(fieldDescriptor.substring(numberStartIndex, numberStartIndex + 1))
            let doubleDigit = parseInt(fieldDescriptor.substring(numberStartIndex, numberStartIndex + 2))
            if (doubleDigit >= 10 && doubleDigit <= 25 && fieldDescriptor.substring(numberStartIndex + 2, numberStartIndex + 3) == "_") {
                if (doubleDigit <= this.signers.length) {
                    if (!this.ascribeFieldsToSignersModal.showModal) {
                        this.shouldAscribeFields = true;
                        return
                    }
                    field.signerId = this.signers[doubleDigit - 1].ClassId
                    this.groupAssignService.addField(field);
                    if (isRadio) {
                        let groupName = (<RadioField>field).groupName
                        this.groupAssignService.setAllRadiosInGroupToSigner(field.signerId, groupName);
                        this.updateStateWithSignerIdToRadioGroup(field.signerId, groupName);
                    }

                }


            }
            else if (singleDigit <= 9 && singleDigit > 0 && fieldDescriptor.substring(numberStartIndex + 1, numberStartIndex + 2) == "_") {
                if (singleDigit <= this.signers.length) {
                    if (!this.ascribeFieldsToSignersModal.showModal) {
                        this.shouldAscribeFields = true;
                        return
                    }
                    field.signerId = this.signers[singleDigit - 1].ClassId
                    this.groupAssignService.addField(field);
                    if (isRadio) {
                        let groupName = (<RadioField>field).groupName
                        this.groupAssignService.setAllRadiosInGroupToSigner(field.signerId, groupName);
                        this.updateStateWithSignerIdToRadioGroup(field.signerId, groupName);
                    }

                }
            }

        }
    }

    updateStateWithSignerIdToRadioGroup(signerId, groupName) {

        this.stateService.getState().subscribe(
            (state: AppState) => {
                state.PageFields.forEach(
                    x => {

                        if (x instanceof RadioField && x.groupName == groupName) {
                            x.signerId = signerId;

                        }
                    }
                )
            }
        ).unsubscribe();
    }
}


