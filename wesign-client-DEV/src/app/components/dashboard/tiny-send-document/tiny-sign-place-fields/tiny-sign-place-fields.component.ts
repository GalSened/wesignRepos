import { Component, OnInit, OnDestroy, ChangeDetectorRef, SimpleChange, ViewChild, ElementRef } from '@angular/core';
import { TemplateApiService } from '@services/template-api.service';
import { DocumentApiService } from '@services/document-api.service';
import { SelfSignApiService } from '@services/self-sign-api.service';
import { Router, ActivatedRoute } from '@angular/router';
import { Store } from '@ngrx/store';
import { IAppState, AlertLevel, AppState } from '@state/app-state.interface';
import { SharedService } from '@services/shared.service';
import { Actions } from '@ngrx/effects';
import { StateProcessService } from '@services/state-process.service';
import { UserApiService } from '@services/user-api.service';
import { forkJoin, Observable, Subscription } from 'rxjs';
import { SignMode } from '@models/enums/sign-mode.enum';
import { FLOW_STEP } from '@models/enums/flow-step.enum';
import * as fieldActions from "@state/actions/fields.actions";
import * as styleActions from "@state/actions/style.actions";
import { switchMap, map, take, combineAll } from 'rxjs/operators';
import { DocumentOperations } from '@models/enums/document-operations.enum';
import { UpdateSelfSignRequest } from '@models/self-sign-api/update-self-sign.model';
import { TextField, CheckBoxField, ChoiceField, SignatureField, TemplatePagesRangeResponse, PageDataResult } from '@models/template-api/page-data-result.model';
import { Errors } from '@models/error/errors.model';
import { ContactApiService } from '@services/contact-api.service';
import { UpdateTemplateRequest } from '@models/template-api/update-template-request.model';
import { reportInvalidActions } from '@ngrx/effects/src/effect_notification';
import { ContactFilter } from '@models/contacts/contact-filter.model';
import { SimpleDocument } from '@models/document-api/document-simple.model';
import { User } from '@models/account/user.model';
import { Signer } from '@models/document-api/signer.model';

// TODO - DELETE COMPONENT ***********************
@Component({
    selector: 'sgn-tiny-sign-place-fields',
    templateUrl: './tiny-sign-place-fields.component.html',
    styles: []
})
export class TinySignPlaceFieldsComponent implements OnInit, OnDestroy {

    public pages$: Observable<number[]>;
    public templateId: string;
    public currentDocumentName = "";

    public mode: SignMode = SignMode.Workflow;

    public FLOW_STEP = FLOW_STEP;

    public zmodel: { ZoomLevel: number, Bright: boolean } = { ZoomLevel: 1, Bright: false };

    public currentFlowStep: FLOW_STEP;

    public signatureColor: string;
    isDark: boolean = true;
    private storeSelectSub: Subscription;
    private userSub: Subscription;
    private isBusy: boolean;
    private documentSent: boolean;

    @ViewChild("elem", { static: true })
    public elem: ElementRef;
    public pagesCount: number;
    public templatePages: PageDataResult[] = [];
    public currentPage: number = 1;
    public changePageNumber(pageNumer) {
        this.currentPage = pageNumer
    }
    constructor(private templateApi: TemplateApiService,
        private documentApi: DocumentApiService,
        private router: Router,
        private route: ActivatedRoute,
        private store: Store<IAppState>,
        private sharedService: SharedService,
        private changeDetectorRef: ChangeDetectorRef,
        private stateProcess: StateProcessService) {
        this.isBusy = false;
        this.documentSent = false;
    }

    public ngOnDestroy(): void {
        if (this.storeSelectSub) {
            this.storeSelectSub.unsubscribe();
        }

        if (this.userSub)
            this.userSub.unsubscribe();

        if (!this.documentSent) {
            this.templateApi.delete(this.templateId).subscribe(x => {
                //console.log(this.templateId + ' template deleted')
            }).unsubscribe();
        }
    }

    public ngOnInit() {
        this.router.routeReuseStrategy.shouldReuseRoute = () => false;
        this.store.dispatch(new fieldActions.ClearAllFieldsAction({}));
        this.documentSent = false;

        this.storeSelectSub = this.stateProcess.getStateSnapshot().subscribe((state) => {
            if (state.FlowStep !== FLOW_STEP.TINY_SIGN_PLACE_FIELDS) {
                const prevUrl = sessionStorage.getItem("prev.url") || "/dashboard";
                this.router.navigateByUrl(prevUrl);
            }
        });

        this.isBusy = false;
        this.store.dispatch(new styleActions.StyleHeaderClassesAction({ Classes: "" }));
        this.route.params.pipe(
            switchMap((params) => {

                this.templateId = params.templateId;
                this.sharedService.setBusy(true, "GLOBAL.LOADING");
                this.changeDetectorRef.detectChanges();
                return this.templateApi.pageCount(this.templateId);
            }),
            map((res) => {
                this.sharedService.setBusy(false);
                this.pagesCount = res.pagesCount;
                let limit = 5;
                let arrSize = Math.ceil(res.pagesCount / limit);
                this.sharedService.setLoadingBanner(Math.ceil(Number(arrSize / 2)), "DOCUMENT.LOADING");
                const pageCount = res.pagesCount;
                this.sharedService.setLoadingBanner(pageCount, "DOCUMENT.LOADING");
                this.currentDocumentName = res.templateName;
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
            switchMap((res) => forkJoin(res))

        ).subscribe((data: TemplatePagesRangeResponse[]) => {
            data.forEach(element => {
                this.templatePages = this.templatePages.concat(element.templatePages);
            });
            this.pages$ = new Observable(x => x.next(Array.from(Array(this.pagesCount).keys()).map((v) => v + 1)));
        }
        );
    }

    public next() {
        this.sharedService.setBusy(true);

        if (!this.currentDocumentName || this.currentDocumentName.length === 0) {
            this.sharedService.setTranslateAlert("DOCUMENT.NAME_EMPTY", AlertLevel.ERROR);
            this.sharedService.setBusy(false);
            return;
        }
        if (!this.isBusy) {
            this.saveDocumentAndSend();
        }
    }

    public back() {
        this.templateApi.delete(this.templateId).subscribe(x => { });
        const prevUrl = sessionStorage.getItem("prev.url") || "/dashboard";
        this.router.navigateByUrl(prevUrl);
    }

    public addSignature() {
        this.store.dispatch(new fieldActions.FieldButtonClickedAction({
            FieldType: 'Signature',
            // Y: this.elem.nativeElement.getBoundingClientRect().top + 100
            Y: (window.innerHeight / 2),
            X: (window.innerWidth / 2),
            Width: undefined,
            Height: undefined,
            GroupName: "",
            Mandatory: true
        }));
    }


    private saveDocumentAndSend()/*: Promise<any>*/ { // TODO - convert to observable? maybe   
        this.isBusy = true;
        let signer: Signer;
        let missingSigFields = false;
        let intersectFields = false;
        let field;
        //return new Promise((resolve, reject) => {
        this.userSub = this.stateProcess.getStateSnapshot().pipe(
            switchMap((state: AppState) => {
                const request = new UpdateTemplateRequest();
                request.id = this.templateId;
                let fieldsArray = [];
                request.fields.textFields = this.stateProcess.getPageFields<TextField>(state, TextField);
                request.fields.checkBoxFields = this.stateProcess.getPageFields<CheckBoxField>(state, CheckBoxField);
                request.fields.choiceFields = this.stateProcess.getPageFields<ChoiceField>(state, ChoiceField);
                request.fields.radioGroupFields = this.stateProcess.getRadioGroupFields(state);// state.RadioGroupNames;
                request.fields.signatureFields = this.stateProcess.getPageFields<SignatureField>(state, SignatureField);
                if (request.fields.signatureFields.length <= 0) {
                    missingSigFields = true;
                    throw "missing Sig fields";
                }
                fieldsArray = fieldsArray.concat(request.fields.textFields)
                fieldsArray = fieldsArray.concat(request.fields.checkBoxFields)
                fieldsArray = fieldsArray.concat(request.fields.choiceFields)
                fieldsArray = fieldsArray.concat(request.fields.signatureFields)
                request.fields.radioGroupFields.forEach(x => {
                    fieldsArray = fieldsArray.concat(x.radioFields)
                }
                );

                field = this.sharedService.isFieldsIntersect(fieldsArray);
                if (field != undefined) {
                    intersectFields = true;
                    throw "intersect fields";


                }

                request.name = this.currentDocumentName;
                signer = state.Signers[0];
                return this.templateApi.update(request);
            }),
            switchMap(data => {
                const simpleDocument = new SimpleDocument();
                simpleDocument.templateId = this.templateId;
                simpleDocument.documentName = this.currentDocumentName;
                simpleDocument.signerMeans = signer.DeliveryMeans;
                simpleDocument.signerName = signer.FullName;
                simpleDocument.rediretUrl = "";

                return this.documentApi.sendSimpleDocument(simpleDocument)
            })
        ).subscribe(res => {
            this.sharedService.setBusy(false);
            this.documentSent = true;
            this.router.navigate(["/dashboard", "success"]);
        }, (error) => {
            this.isBusy = false;
            if (missingSigFields) {
                this.sharedService.setTranslateAlert("DOCUMENT.NO_FIELDS", AlertLevel.ERROR);
            }
            else if (intersectFields) {
                this.store.dispatch(new fieldActions.FieldsIntersectAction({ Field: field }));
                this.sharedService.setTranslateAlert("GLOBAL.OVERLAYING_FIELDS", AlertLevel.ERROR);

            }
            else {
                this.sharedService.setErrorAlert(new Errors(error.error));
            }

            this.sharedService.setBusy(false);
        });

        //});

    }


}
