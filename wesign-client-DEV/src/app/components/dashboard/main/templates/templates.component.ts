import { Component, OnDestroy, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { User } from "@models/account/user.model";
import { FLOW_STEP } from "@models/enums/flow-step.enum";
import { TemplateFilter } from "@models/template-api/template-filter.model";
import { TemplateInfo } from "@models/template-api/template-info.model";
import { TemplateInfos } from "@models/template-api/template-infos.model";
import { Store } from "@ngrx/store";
import { TranslateService } from "@ngx-translate/core";
import { PagerService } from "@services/pager.service";
import { SharedService } from "@services/shared.service";
import { TemplateApiService } from "@services/template-api.service";
import * as selectActions from "@state/actions/selection.actions";
import { IAppState, AppState } from "@state/app-state.interface";
import { Observable, Subscription } from "rxjs";
import { switchMap } from "rxjs/operators";
import { Modal } from '@models/modal/modal.model';
import { ModalService } from '@services/modal.service';
import { Errors } from '@models/error/errors.model';
import * as fieldsActions from "@state/actions/fields.actions";
import { userType } from '@models/enums/user-type.enum';
import * as documentActions from "@state/actions/document.actions";
import { BatchRequest } from '@models/document-api/batch-request.model';

import { DocumentSigner } from '@models/document-api/document-create-request.model';
import { TemplateSingleLink } from '@models/template-api/template-single-link.model';
import { LinksApiService } from '@services/links-api.service';

@Component({
    selector: "sgn-templates-component",
    templateUrl: "templates.component.html",
})

export class TemplatesComponent implements OnInit, OnDestroy {

    public users: User[] = [];
    public isFromCalendarShown = false;
    public isToCalendarShown = false;
    public templateFilter: TemplateFilter = new TemplateFilter();
    public templateCount: number;
    public pageCalc: any;
    public currentPage = 1;

    public templateInfos: TemplateInfos = new TemplateInfos();

    public FLOW_STEP = FLOW_STEP;

    public state$: Observable<AppState>;

    private PAGE_SIZE = 10;

    public showCheckboxes: boolean = false;

    public orderByField: string = 'timeCreated';
    public orderByDesc: boolean = true;

    private deleteModal: Modal;
    private deleteSubscription: Subscription;
    private templatesSubscription: Subscription;
    private stateSubscription: Subscription;

    public selectedSingleLink: string;
    public selectedtemplateId: string;
    public showSingleLink: boolean;
    public isTemplateSelected: boolean = true;
    public showAddTemplate: boolean = false
    showUploadTemplate: boolean;
    public deleteAllPopupData: Modal = new Modal();
    public showSearchSpinner: boolean = false;
    public allSelected: boolean = false;
    public selectedTemplates: string[] = [];
    public tempStateCopy: AppState;
    currSigner: DocumentSigner;
    constructor(
        private templateApiService: TemplateApiService,
        private sharedService: SharedService,
        public router: Router,
        private pager: PagerService,
        private translate: TranslateService,
        private store: Store<IAppState>,
        private modalService: ModalService,
        private linksApiService: LinksApiService,
    ) {
        this.state$ = this.store.select<any>('appstate');
        this.stateSubscription = this.state$.subscribe(a => {
            this.showCheckboxes = a.FlowStep == FLOW_STEP.ONLINE_SELECT || a.FlowStep == FLOW_STEP.MULTISIGN_SELECT;
            this.showAddTemplate = a.currentUserType != userType.Basic && a.program.uiViewLicense.shouldShowAddNewTemplate;
            this.selectedTemplates = [];
            a.SelectedTemplates.forEach(x =>
                this.selectedTemplates.push(x.templateId)
            );

        })
        this.currSigner = new DocumentSigner();
    }

    public ngOnInit() {
        this.updateData(true);
        this.store.dispatch(new documentActions.ClearFileUploadRequestAction({}));
        this.store.dispatch(new fieldsActions.ClearAllFieldsAction({}))
        this.store.dispatch(new selectActions.ClearTemplateSelectionAction({}))

        this.deleteSubscription =
            this.modalService.checkConfirm()
                .pipe(
                    switchMap(res => { return res; }),
                    switchMap((res: string) => {
                        this.sharedService.setBusy(true, "TEMPLATE.DELETING");
                        return this.templateApiService.delete(res)
                    })
                ).subscribe(
                    res => {
                        this.store.dispatch(new fieldsActions.ClearAllFieldsAction({}));
                        this.updateData(true);
                        this.sharedService.setSuccessAlert("TEMPLATE.DELETE");


                    }, err => {
                        this.sharedService.setErrorAlert(new Errors(err.error));
                        this.sharedService.setBusy(false);
                    }, () => { this.sharedService.setBusy(false); }
                );

        let elements = document.getElementsByName("checkboxOverlay[]");
        Array.from(elements).forEach((x: HTMLInputElement) => {
            x.checked = false;
        })
    }

    public onSignTemplates() {
        if (this.selectedTemplates.length > 0 && this.selectedTemplates.length <= 10) {
            // let firstTemplateSelected = this.templateInfos.templates.find(t => t.templateId === this.selectedTemplates[0]);
            let firstTemplateSelected = this.templateInfos.templates.find(t => t.templateId === this.selectedTemplates[0]);

            if (firstTemplateSelected != null) {
                this.store.dispatch(new documentActions.SetDocumentName({ CurrentDocumentName: firstTemplateSelected.name.slice(0, 50) }));
                this.router.navigate(["dashboard", "selectsigners"]);
            }
            else {
                // this.updateAllData(true);

                this.templateFilter.Limit = -1; // 0 means no limit
                this.templateFilter.Offset = 0; // 1000000 means no offset

                this.sharedService.setBusy(true, "TEMPLATE.LOADING")

                this.templatesSubscription = this.templateApiService.getTemplates(this.templateFilter).subscribe((data) => {

                    this.templateCount = +data.headers.get("x-total-count");
                    // this.pageCalc = this.pager.getPager(this.templateCount, this.currentPage, this.PAGE_SIZE);

                    if (this.templateCount === 0) {
                        this.templateInfos.templates.length = 0;
                    } else {
                        this.templateInfos = data.body;
                        //this.templateInfos.templates.forEach((t) => t.userName = this.getUserById(t.userId));
                    }

                    firstTemplateSelected = this.templateInfos.templates.find(t => t.templateId === this.selectedTemplates[0]);
                    if (firstTemplateSelected != null) {
                        this.store.dispatch(new documentActions.SetDocumentName({ CurrentDocumentName: firstTemplateSelected.name.slice(0, 50) }));
                        this.router.navigate(["dashboard", "selectsigners"]);
                    }

                }, error => {
                    this.sharedService.setErrorAlert(new Errors(error.errors))

                },
                    () => {
                        this.sharedService.setBusy(false);
                        this.showSearchSpinner = false;
                    }
                );



                // firstTemplateSelected = this.templateInfos.templates.find(t => t.templateId === this.selectedTemplates[0]);
                // if (firstTemplateSelected != null) {
                //     this.store.dispatch(new documentActions.SetDocumentName({ CurrentDocumentName: firstTemplateSelected.name.slice(0, 50) }));
                //     this.router.navigate(["dashboard", "selectsigners"]);
                // }
                // let errMsg = this.translate.instant("ERROR.OPERATION.7");
                // this.sharedService.setErrorAlert(errMsg);
            }
        }
        else {
            let errMsg = this.translate.instant("ERROR.OPERATION.7");
            this.sharedService.setErrorAlert(errMsg);
        }
    }

    public pageChanged(page: number) {
        this.currentPage = page;
        this.showSearchSpinner = true;
        this.updateData(false);
    }

    public ngOnDestroy() {
        this.deleteSubscription.unsubscribe();
        this.templatesSubscription.unsubscribe();
        this.stateSubscription.unsubscribe();
    }

    public updateData(showLoading) {
        this.templateFilter.Limit = this.PAGE_SIZE;
        this.templateFilter.Offset = (this.currentPage - 1) * this.PAGE_SIZE;
        if (showLoading) {
            this.sharedService.setBusy(true, "TEMPLATE.LOADING")
        }

        this.templatesSubscription = this.templateApiService.getTemplates(this.templateFilter).subscribe((data) => {

            this.templateCount = +data.headers.get("x-total-count");
            this.pageCalc = this.pager.getPager(this.templateCount, this.currentPage, this.PAGE_SIZE);

            if (this.templateCount === 0) {
                this.templateInfos.templates.length = 0;
            } else {
                this.templateInfos = data.body;
                //this.templateInfos.templates.forEach((t) => t.userName = this.getUserById(t.userId));
            }

        }, error => {
            this.sharedService.setErrorAlert(new Errors(error.errors))

        },
            () => {
                this.sharedService.setBusy(false);
                this.showSearchSpinner = false;
            }
        );
    }

    public showFromCalendar() {
        this.isFromCalendarShown = !this.isFromCalendarShown;
        this.isToCalendarShown = false;
    }

    public showToCalendar() {
        this.isToCalendarShown = !this.isToCalendarShown;
        this.isFromCalendarShown = false;
    }

    public fromSelected(date: Date) {
        this.templateFilter.From = date;
        this.updateData(false);
    }

    public toSelected(date: Date) {
        this.templateFilter.To = date;
        this.updateData(false);
    }

    public removeDate($event: any, dateElement: any) {
        $event.preventDefault();
        this.templateFilter[dateElement] = null;
        this.updateData(false);
    }


    public duplicate(templateId: string) {
        this.sharedService.setBusy(true, "TEMPLATE.DUPLICATING");
        this.templateApiService.duplicate(templateId).pipe(
            switchMap((res) => {
                return this.translate.get("TEMPLATE.DUPLICATED");
            })).subscribe((msg) => {
                this.sharedService.setSuccessAlert(msg);
                this.updateData(false);
            }, (error) => {
                this.sharedService.setErrorAlert(new Errors(error.error));
                this.sharedService.setBusy(false);
            });
    }

    public download(templateId: string) {
        this.sharedService.setBusy(true, "GLOBAL.DOWNLOADING");
        this.templateApiService.forceDownloadTemplate(templateId);
    }

    public cancelAsk(delNav: HTMLElement, btnNav: HTMLElement, event) {
        delNav.classList.remove("ws_is-shown");
        btnNav.classList.remove("ws_is-hidden");
        event.stopPropagation();
    }
    public selectedALL($event, state: AppState) {

        if (this.allSelected) {
            let temp = []
            state.SelectedTemplates.forEach(element => {
                temp.push(element)
            });

            temp.forEach(template =>
                this.store.dispatch(new selectActions.UnselectTemplateAction({ templateInfo: template }))
            );
        }
        else {
            this.templateInfos.templates.forEach(template => {
                if (!this.isSelected(state, template)) {
                    this.store.dispatch(new selectActions.SelectTemplateAction({ templateInfo: template }))
                }
            }

            );
        }

        this.allSelected = !this.allSelected;

    }

    public doDeleteAllTempatesEvent() {
        this.deleteAllPopupData.showModal = false;
        this.sharedService.setBusy(true, "TEMPLATE.DELETING");
        let docBatchReq = new BatchRequest();
        let stateSelectedTemplates = [];
        this.tempStateCopy.SelectedTemplates.forEach(template => {
            docBatchReq.Ids.push(template.templateId)
            stateSelectedTemplates.push(template);
        })

        return this.templateApiService.deleteDocumentBatch(docBatchReq).subscribe(
            res => {
                stateSelectedTemplates.forEach(template => {
                    this.store.dispatch(new selectActions.UnselectTemplateAction({ templateInfo: template }));
                });
                this.updateData(true);
                this.sharedService.setSuccessAlert("TEMPLATE.DELETEDS_SUCCESSFULY");
                this.allSelected = false;
                this.sharedService.setBusy(false);
            }, err => {
                this.sharedService.setErrorAlert(new Errors(err.error));
                this.sharedService.setBusy(false);
            }, () => {
            }
        );
    }

    public deleteBatchTemplates($event, state: AppState) {
        if (state.SelectedTemplates.length == 0) {
            return;
        }
        $event.preventDefault();
        $event.stopPropagation();
        this.tempStateCopy = state;
        this.deleteAllPopupData.showModal = true;
        this.translate.get(['TEMPLATE.MODALTITLE', 'TEMPLATE.MODALTEXT_BATCH_DELETE', 'REGISTER.CANCEL', 'GLOBAL.DELETE'])
            .subscribe((res: object) => {
                let keys = Object.keys(res);
                this.deleteAllPopupData.title = res[keys[0]];
                this.deleteAllPopupData.content = (res[keys[1]] as string).replace("{#*#}", state.SelectedTemplates.length.toString());
                this.deleteAllPopupData.rejectBtnText = res[keys[2]];
                this.deleteAllPopupData.confirmBtnText = res[keys[3]];
            });

    }


    public deleteTemplate(template: TemplateInfo) {

        this.deleteModal = new Modal({ showModal: true });

        this.translate.get(['TEMPLATE.MODALTITLE', 'TEMPLATE.MODALTEXT', 'TEMPLATE.MODALCANCELBTN', 'TEMPLATE.MODALDELETEBTN']).subscribe((res: object) => {
            let keys = Object.keys(res);
            this.deleteModal.title = res[keys[0]];
            this.deleteModal.content = (res[keys[1]] as string).replace("{#*#}", template.name);

            this.deleteModal.rejectBtnText = res[keys[2]];
            this.deleteModal.confirmBtnText = res[keys[3]];

            let confirmAction = new Observable(ob => {
                ob.next(template.templateId);
                ob.complete();

                this.store.dispatch(new selectActions.UnselectTemplateAction({ templateInfo: template }));
                return { unsubscribe() { } };
            });
            this.deleteModal.confirmAction = confirmAction;

            this.modalService.showModal(this.deleteModal);
        });

    }

    public selected(event, templateInfo: TemplateInfo) {
        if (event.target.checked) {
            this.store.dispatch(new selectActions.SelectTemplateAction({ templateInfo }));
        } else {

            this.store.dispatch(new selectActions.UnselectTemplateAction({ templateInfo }));
        }


    }

    public isSelected(state: AppState, templateInfo: TemplateInfo) {
        return state.SelectedTemplates.findIndex((t) => t.templateId === templateInfo.templateId) > -1;
    }

    public focusOutUpdate(newValue, contact: TemplateInfo, property: string) {
        //***TODO***

        // if (newValue.valid && newValue.value !== contact[property]) {
        //     contact[property] = newValue.value;
        //     this.saveContact(contact);
        // }
    }


    trackByFn(index, item) {
        return index;
    }

    public orderByFunction(prop: string) {
        if (prop) {
            if (this.orderByField == prop) {
                this.orderByDesc = !this.orderByDesc;
            }
            this.orderByField = prop;
        }
    }

    public getLocalTime(utcTime: string) {
        return this.sharedService.getLocalTime(utcTime);
    }



    public url($event, templateId: string, singleLinkUrl: string) {
        $event.preventDefault();
        $event.stopPropagation();
        this.selectedSingleLink = singleLinkUrl;
        this.selectedtemplateId = templateId;

        this.linksApiService.getSingleLinkAttachments(templateId).subscribe((res: TemplateSingleLink) => {
            if (res.singleLinkAdditionalResources == null || res.singleLinkAdditionalResources.length == 0) {
                this.currSigner.signerAttachments = [];
            }
            else {
                this.currSigner.signerAttachments = res.singleLinkAdditionalResources.map((x) => {
                    return {
                        name: x.data,
                        isMandatory: x.isMandatory
                    }
                });


            }
            this.showSingleLink = true;
        });





    }



    public onDropDownSelect(value: number) {
        this.PAGE_SIZE = value;
        this.updateData(true)
    }

}
