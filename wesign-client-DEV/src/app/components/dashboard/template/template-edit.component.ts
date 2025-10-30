import { ChangeDetectorRef, Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { FLOW_STEP } from "@models/enums/flow-step.enum";
import {CheckBoxField, ChoiceField, PageDataResult, SignatureField, TemplatePagesRangeResponse, TextField } from "@models/template-api/page-data-result.model";
import { UpdateTemplateRequest } from "@models/template-api/update-template-request.model";
import { Actions } from "@ngrx/effects";
import { Store } from "@ngrx/store";
import { SharedService } from "@services/shared.service";
import { TemplateApiService } from "@services/template-api.service";
import * as fieldActions from "@state/actions/fields.actions";
import { IAppState, AlertLevel, AppState } from "@state/app-state.interface";
import { forkJoin, Observable } from "rxjs";
import { map, switchMap} from "rxjs/operators";
import { StateProcessService } from '@services/state-process.service';
import { Errors } from '@models/error/errors.model';
import * as selectActions from "@state/actions/selection.actions";
import { DomSanitizer } from '@angular/platform-browser';

@Component({
    selector: "sgn-template-edit",
    templateUrl: "template-edit.component.html",
})

export class TemplateEditComponent implements OnInit {

    public templatePages : PageDataResult [] = [];
    public pages: number[] = [];
    public pages$: Observable<number[]>;
    public isDark: boolean = true;
    public templateId: string;
    public templateName = "";
    public signers: any;
    public signatureFields: SignatureField[];
    public activeSignatureFieldName: string;
    public FLOW_STEP = FLOW_STEP;
    public zmodel: { ZoomLevel: number, Bright: boolean } = { ZoomLevel: 1, Bright: true };
    pagesCount: number;

    constructor(
        private templateApi: TemplateApiService,
        private router: Router,
        private route: ActivatedRoute,
        private store: Store<IAppState>,
        private sharedService: SharedService,
        private changeDetectorRef: ChangeDetectorRef,
        private stateProcess: StateProcessService,
        private sanitizer: DomSanitizer
    ) {
        this.sharedService.setFlowState("template", FLOW_STEP.TEMPLATE_EDIT);
        this.store.dispatch(new fieldActions.ClearAllFieldsAction({}));
        this.store.dispatch(new selectActions.SelectSignerClassId({classId : "" }));
    }

    public currentPage : number = 1;
public changePageNumber(pageNumer)
{
    this.currentPage = pageNumer
   
}
    public ngOnInit() {
        this.route.params.pipe(
            switchMap((params) => {
                this.sharedService.setBusy(true,  "GLOBAL.LOADING");
                this.templateId = params.id;
                this.changeDetectorRef.detectChanges();
                return this.templateApi.pageCount(this.templateId);
            }),
            map((res) => {
                this.sharedService.setBusy(false);
                this.pagesCount = res.pagesCount;
                //this.pages$ = new Observable (x => x.next( Array.from(Array(res.pagesCount).keys()).map((v) => v + 1)));
                let limit = this.pagesCount < 100 ? 10 : Math.ceil(this.pagesCount / 10)
                let arrSize = Math.ceil(this.pagesCount / limit);
                this.sharedService.setLoadingBanner(Math.ceil(Number(arrSize / 2 )), "TEMPLATE.LOADING");
                this.templateName = res.templateName;
                let offset = 1;
                let arrtest = new Array(arrSize).fill(this.templateId).map(
                    (id) => {
                        let res = { id, offset, limit };
                        offset = offset + limit;
                        return res;
                    });
                let pagesRequests =  arrtest.map(arra => this.templateApi.getPages(arra.id, arra.offset, arra.limit) )

                return pagesRequests;
            }),
            switchMap(ar => forkJoin(ar))
        ).subscribe((data: TemplatePagesRangeResponse[]) => {
            data.forEach(element => {
                this.templatePages = this.templatePages.concat(element.templatePages);
                this.templatePages.forEach(templatePage => {
                    if (templatePage.ocrString) {
                        templatePage.ocrHtml = this.sanitizer.bypassSecurityTrustHtml(templatePage.ocrString || '');
                    }
                });
            });   
            this.pages$ = new Observable (x => x.next( Array.from(Array(this.pagesCount).keys()).map((v) => v + 1)));               
        });
    }

    public ngOnDestroy() {
    }

    public next($event: any) {
        if (!this.templateName || this.templateName.length === 0) {
            this.sharedService.setTranslateAlert("TEMPLATE.NAME_EMPTY", AlertLevel.ERROR);
            return;
        }

        this.store.dispatch(new fieldActions.ShouldLoadFields({shouldLoad: true}));

        this.stateProcess.getStateSnapshot().subscribe((state: AppState) => {
            const request = new UpdateTemplateRequest();
            request.fields.textFields = this.stateProcess.getPageFields<TextField>(state, TextField);
            this.removeErrorClassFromElements(request.fields.textFields);
            request.fields.textFields = this.sharedService.FormatDateTextFromDOMDateFormat(request.fields.textFields);
            request.fields.checkBoxFields = this.stateProcess.getPageFields<CheckBoxField>(state, CheckBoxField);
            request.fields.choiceFields = this.stateProcess.getPageFields<ChoiceField>(state, ChoiceField);
            request.fields.radioGroupFields = this.stateProcess.getRadioGroupFields(state);// state.RadioGroupNames;
            request.fields.signatureFields = this.stateProcess.getPageFields<SignatureField>(state, SignatureField).
                map((sf) => { sf.image = ""; return sf; });


              let fieldsArray = []
              fieldsArray = fieldsArray.concat(request.fields.textFields );
              fieldsArray = fieldsArray.concat(request.fields.checkBoxFields );
              fieldsArray = fieldsArray.concat(request.fields.choiceFields );
              fieldsArray = fieldsArray.concat(request.fields.signatureFields );              
              request.fields.radioGroupFields.forEach(x =>  
                {
                    fieldsArray = fieldsArray.concat(x.radioFields )
                }
                );
              

                fieldsArray = this.sharedService.FixFieldWithDotsInFieldName (fieldsArray);
                

                let field = this.sharedService.isFieldsIntersect(fieldsArray);
               if(field != undefined)
               {
                this.store.dispatch(new fieldActions.FieldsIntersectAction({Field : field}));
                this.sharedService.setTranslateAlert("GLOBAL.OVERLAYING_FIELDS", AlertLevel.ERROR);                   
                return;
               }            

            request.id = this.templateId;
            request.name = this.templateName;

            this.sharedService.setBusy(true, "TEMPLATE.SAVING");
            let ligalRadioGroup = true;
            request.fields.radioGroupFields.forEach(x => {
                if (x.radioFields.length == 1) {
                    ligalRadioGroup = false;
                }
            });

            if (!ligalRadioGroup) {
                this.sharedService.setBusy(false);
                this.sharedService.setTranslateAlert("GLOBAL.SHOULD_CONTAIN_TWO_ELEMENTS", AlertLevel.ERROR);

                return;
            }

            this.templateApi.update(request).subscribe((_) => {
                this.sharedService.setSuccessAlert("TEMPLATE.SAVED");
                this.sharedService.setBusy(false);    
            }, (error) => {
                this.sharedService.setBusy(false);
                let ex  = new Errors(error.error);
                this.scrollIntoInvalidField(ex);   
                this.sharedService.setErrorAlert(ex);

            }, () => { this.sharedService.setBusy(false); });
        });
    }

    private scrollIntoInvalidField(ex){
        if(ex.errorCode == 400){
            let errorValue =  ex.errors.errors[Object.keys(ex.errors.errors)[0]];
            let openIndex = errorValue[0].lastIndexOf('[');
            let closeIndex = errorValue[0].indexOf(']', openIndex);
            let fieldName = errorValue[0].substring(openIndex + 1, closeIndex);
            let htmlElements = document.querySelectorAll('[data-fieldname="' + fieldName +'"]');
            if(htmlElements.length > 0)
            {
                setTimeout(() => {

                    for (let index = 0; index < htmlElements[0].childNodes.length; index++) {
                        let divChild = htmlElements[0].childNodes[index];
                        if(divChild instanceof HTMLDivElement){
                            (<HTMLDivElement>divChild).classList.add("is-error");
                            htmlElements[0].scrollIntoView();
                            break;
                        }
                    }

                }, 2000);
            }
        }
    }
    private removeErrorClassFromElements(textFields : TextField[]){
        textFields.forEach(textField => {
            let htmlElements = document.querySelectorAll('[data-fieldname="' + textField.name +'"]');
            if(htmlElements.length > 0)
            {
                let divChild = htmlElements[0].childNodes[0];
                if(divChild instanceof HTMLDivElement){
                    (<HTMLDivElement>divChild).classList.remove("is-error");
                }
            }
        });
    }

    public back(): void {
        this.router.navigateByUrl(this.sharedService.getBackUrl());
    }

    public updateTemplateName(name) {
        this.templateName = name;
    }

    changeBackgroud() {
        this.isDark = !this.isDark;
    }
}
