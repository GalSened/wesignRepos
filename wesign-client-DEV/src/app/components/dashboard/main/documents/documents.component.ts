import { Component, OnInit, Input, HostBinding, Injectable, ViewChild } from "@angular/core";
import { Router } from "@angular/router";
import { User } from "@models/account/user.model";
import { Contact, SendingMethod } from "@models/contacts/contact.model";
import { DocFilter, SearchParameter } from "@models/document-api/doc-filter.model";
import { DocumentsStatus } from "@models/document-api/documents-status.model";
import { DocStatus } from "@models/enums/doc-status.enum";
import { FLOW_STEP } from "@models/enums/flow-step.enum";
import { SignMode } from "@models/enums/sign-mode.enum";
import { DocumentApiService } from "@services/document-api.service";
import { PagerService } from "@services/pager.service";
import { SharedService } from "@services/shared.service";
import { signer } from '@models/document-api/document-signer.model';
import { SignerStatus } from '@models/enums/signer-status.enum';
import { Modal } from '@models/modal/modal.model';
import { Subscription, Observable } from 'rxjs';
import { ModalService } from '@services/modal.service';
import { switchMap, tap } from 'rxjs/operators';
import { documentCollections } from '@models/document-api/documentsCollection.model';
import { TranslateService } from '@ngx-translate/core';
import { Errors } from '@models/error/errors.model';
import { AlertLevel, IAppState } from '@state/app-state.interface';
import { Store } from '@ngrx/store';
import { BatchRequest } from '@models/document-api/batch-request.model';
import * as appActions from "@state/actions/app.actions";
import * as fieldsActions from "@state/actions/fields.actions";
import * as documentActions from "@state/actions/document.actions";
import * as selectActions from "@state/actions/selection.actions";

@Component({
    selector: "sgn-documents-component",
    templateUrl: "documents.component.html",
})

@Injectable()
export class DocumentsComponent implements OnInit {

    @HostBinding('style.overflow') overflow = 'auto';
    @HostBinding('style.width') width = '100%';
    @ViewChild('select') select: HTMLSelectElement
    @Input()
    isTiny: boolean;
    showSearchSpinner = false;
    docFilter = new DocFilter();
    searchParameters = Object.values(SearchParameter).filter(value => typeof value != 'number')
    public users: User[];
    public documentsCount: number;
    public docStatusData: DocumentsStatus;
    public currentPage = 1;
    public isFromCalendarShown = false;
    public isToCalendarShown = false;
    public allSelected = false;
    public SignMode = SignMode;
    public DocStatus = DocStatus;
    public SendingMethod = SendingMethod;
    public SignerStatus = SignerStatus;
    public activeDocumentId = "";
    public pageCalc: any;
    public orderByField = 'creationTime';
    public orderByDesc = true;
    public selectedDocuments: string[] = [];
    path = "all";
    deleteAllPopupData = new Modal();
    PAGE_SIZE = 10;
    deleteModal: Modal;
    deleteSubscription: Subscription;
    cancelSubscription: Subscription;
    documentsSubscription: Subscription;
    showShare: boolean;
    currDocumentCollectionId: string;
    isBusy: boolean;
    showReplaceSigner: boolean;
    oldSignerId: string;
    shouldOpenLiveSigning = false;
    currentLiveSigningLink = "";
    textDirection: string;

    constructor(private documentApiService: DocumentApiService, private pager: PagerService, private router: Router,
        private sharedService: SharedService, private translate: TranslateService, private modalService: ModalService,
        private documentService: DocumentApiService, private store: Store<IAppState>) {
        this.textDirection = document.dir;
    }

    ngOnInit() {
        this.store.dispatch(new documentActions.ClearFileUploadRequestAction({}));
        this.store.dispatch(new fieldsActions.ClearAllFieldsAction({}))
        this.store.dispatch(new selectActions.ClearTemplateSelectionAction({}))

        this.path = this.router.url.split('/').pop() || 'all';

        if (this.path == 'all') {
            this.changeAllFilters(this.docFilter, true);
        } else {
            this.changeAllFilters(this.docFilter, false);
            if (this.path == 'pending') {
                this.docFilter.sent = true;
                this.docFilter.viewed = true;
            }
            if (this.path == 'signed') this.docFilter.signed = true;
            if (this.path == 'declined') this.docFilter.declined = true;
            if (this.path == 'canceled') this.docFilter.canceled = true;
        }

        this.updateData(true);

        this.sharedService.setFlowState("none", FLOW_STEP.NONE);

        this.deleteSubscription =
            this.modalService.checkConfirm()
                .pipe(
                    switchMap(res => { return res; }),
                    switchMap((res: string) => {
                        this.sharedService.setBusy(true, "DOCUMENT.DELETING");
                        return this.documentApiService.deleteDocument(res)
                    })
                ).subscribe(
                    res => {
                        this.updateData(true);
                        this.sharedService.setSuccessAlert("DOCUMENT.DELETED_SUCCESSFULY");
                        this.sharedService.setBusy(false);
                    }, err => {
                        this.sharedService.setErrorAlert(new Errors(err.error));
                        this.sharedService.setBusy(false);
                    }, () => { this.sharedService.setBusy(false); }
                );

        this.cancelSubscription =
            this.modalService.checkConfirm2()
                .pipe(
                    switchMap(res => { return res; }),
                    switchMap((res: string) => {
                        this.sharedService.setBusy(true, "DOCUMENT.CANCELING");
                        return this.documentApiService.cancelDocument(res)
                    })
                ).subscribe(
                    res => {
                        this.updateData(true);
                        this.sharedService.setSuccessAlert("DOCUMENT.CANCELED_SUCCESSFULY");
                        this.sharedService.setBusy(false);
                    }, err => {
                        this.sharedService.setErrorAlert(new Errors(err.error));
                        this.sharedService.setBusy(false);
                    }, () => { this.sharedService.setBusy(false); }
                );
    }

    pageChanged(page: number) {
        this.showSearchSpinner = true;
        this.currentPage = page;
        this.updateData(false);
    }

    selectedALL() {
        this.selectedDocuments = [];

        if (!this.allSelected) {
            this.docStatusData.documentCollections.forEach(
                x =>
                    this.selectedDocuments.push(x.documentCollectionId));
        }

        this.allSelected = !this.allSelected;
    }

    updateData(showLoading) {
        this.docFilter.limit = this.PAGE_SIZE;
        this.docFilter.offset = (this.currentPage - 1) * this.PAGE_SIZE;
        if (showLoading) {
            this.sharedService.setBusy(true, "DOCUMENT.LOADING")
        }
        this.docFilter.sendingFailed = this.docFilter.declined && this.docFilter.sent && this.docFilter.signed && this.docFilter.viewed;
        this.documentsSubscription = this.documentApiService.getDocuments(this.docFilter)
            .subscribe((data) => {

                this.documentsCount = +data.headers.get("x-total-count");
                this.pageCalc = this.pager.getPager(this.documentsCount, this.currentPage, this.PAGE_SIZE);
                this.docStatusData = data.body;
                this.store.dispatch(new appActions.SetDocumentListStatus({ documentsStatus: this.docStatusData.documentCollections }));
                this.selectedDocuments = [];
                this.allSelected = false;
            }, error => { }, () => {
                this.sharedService.setBusy(false);
                this.showSearchSpinner = false;
            });
    }

    getSignedCount(signers: signer[]): number {
        if (signer && signers.length) {
            return signers.filter(a => a.status == SignerStatus.Signed).length;
        }
        return 0;
    }

    selecteDoc($event, docCollecteionID) {
        if (this.isDocSelected(docCollecteionID)) {
            this.selectedDocuments = this.selectedDocuments.filter(item => item != docCollecteionID);
        }
        else {
            this.selectedDocuments.push(docCollecteionID);
        }
    }

    getUserById(userId: string) {
        return this.users.find((u) => u.id === userId).email;
    }

    isDocSelected(docCollecteionID) {
        return this.selectedDocuments.findIndex((t) => t == docCollecteionID) > -1;
    }

    showFromCalendardWrapper() {
        if (!this.isFromCalendarShown) {
            this.showFromCalendar();
        }
    }

    showFromCalendar() {
        this.isFromCalendarShown = !this.isFromCalendarShown;
        this.isToCalendarShown = false;
    }

    showToCalendardWrapper() {
        if (!this.isToCalendarShown) {
            this.showToCalendar();
        }
    }

    showToCalendar() {
        this.isToCalendarShown = !this.isToCalendarShown;
        this.isFromCalendarShown = false;
    }

    fromSelected(date: Date) {
        this.docFilter.from = date;
        if (this.docFilter.to != null && this.docFilter.from > this.docFilter.to) {
            this.removeDate(new Event("emptyEvent"), 'to')
        }
        this.updateData(true);
    }

    toSelected(date: Date) {
        this.docFilter.to = date;
        if (this.docFilter.from != null && this.docFilter.from > this.docFilter.to) {
            this.removeDate(new Event("emptyEvent"), 'from')
        }
        this.updateData(true);
    }

    removeDate($event: any, dateElement: any) {
        $event.preventDefault();
        this.docFilter[dateElement] = null;
        this.updateData(true);
    }

    readRowInformation(docId: string) {
        this.documentService.getDocument(docId).subscribe(
            (res) => {
                let index = this.docStatusData.documentCollections.findIndex((e: documentCollections) => e.documentCollectionId == docId);

                if (index >= 0) {
                    this.docStatusData.documentCollections[index] = res;


                }
            },
            (err) => {
                // console.log(err) 
            },
            () => { });
    }

    public onRowClick(docId: string, signers: any[]) {
        this.currentLiveSigningLink = "";
        if (this.activeDocumentId === docId || !signers || signers.length < 1) {
            this.activeDocumentId = "";
        } else {
            this.activeDocumentId = docId;
            this.readRowInformation(docId);

        }
    }

    public formatContact(contact: Contact): string {
        if (contact.name /*|| contact.LastName*/) {
            return `${contact.name}`;
        }
        if (contact.defaultSendingMethod === SendingMethod.SMS) {
            return contact.phone;
        }
        return contact.email;
    }

    public deleteAsk(delNav: HTMLElement, btnNav: HTMLElement, event) {
        delNav.classList.add("ws_is-shown");
        btnNav.classList.add("ws_is-hidden");
        event.stopPropagation();
    }

    public cancelAsk(delNav: HTMLElement, btnNav: HTMLElement, event) {
        delNav.classList.remove("ws_is-shown");
        btnNav.classList.remove("ws_is-hidden");
        event.stopPropagation();
    }

    public deleteDocument($event, document: documentCollections) {
        $event.preventDefault();
        $event.stopPropagation();

        this.deleteModal = new Modal({ showModal: true });

        this.translate.get(['DOCUMENT.MODALTITLE', 'DOCUMENT.MODALTEXT', 'REGISTER.CANCEL', 'GLOBAL.DELETE']).subscribe((res: object) => {
            let keys = Object.keys(res);
            this.deleteModal.title = res[keys[0]];
            this.deleteModal.content = (res[keys[1]] as string).replace("{#*#}", "<span>" + document.name + "</span>");

            this.deleteModal.rejectBtnText = res[keys[2]];
            this.deleteModal.confirmBtnText = res[keys[3]];

            let confirmAction = new Observable(ob => {
                ob.next(document.documentCollectionId);
                ob.complete();

                return { unsubscribe() { } };
            });
            this.deleteModal.confirmAction = confirmAction;

            this.modalService.showModal(this.deleteModal);
        });
    }

    public deleteDocumentAllSelected($event) {
        if (this.selectedDocuments.length == 0) {
            return;
        }
        $event.preventDefault();
        $event.stopPropagation();
        this.deleteAllPopupData.showModal = true;
        this.translate.get(['DOCUMENT.MODALTITLE', 'DOCUMENT.MODALTEXT_BATCH_DELETE', 'REGISTER.CANCEL', 'GLOBAL.DELETE'])
            .subscribe((res: object) => {
                let keys = Object.keys(res);
                this.deleteAllPopupData.title = res[keys[0]];
                this.deleteAllPopupData.content = (res[keys[1]] as string).replace("{#*#}", this.selectedDocuments.length.toString());
                this.deleteAllPopupData.rejectBtnText = res[keys[2]];
                this.deleteAllPopupData.confirmBtnText = res[keys[3]];
            });
    }

    public async downloadAllSelectedDocuments($event) {
        if (this.selectedDocuments.length == 0) {
            return;
        }
        $event.preventDefault();
        $event.stopPropagation();
        // this.sharedService.setBusy(true, "GLOBAL.DOWNLOADING");
        let docBatchReq = new BatchRequest();
        docBatchReq.Ids = this.selectedDocuments;
        if (docBatchReq.Ids.length > 20) {
            this.sharedService.setErrorAlert(this.translate.instant("ERROR.OPERATION.6"));
            return;
        } else {
            this.documentApiService.downloadDocuments(docBatchReq);
        }
    }

    doDeleteAllDocsEvent() {
        this.deleteAllPopupData.showModal = false;
        this.sharedService.setBusy(true, "DOCUMENT.DELETING");
        let docBatchReq = new BatchRequest();
        docBatchReq.Ids = this.selectedDocuments;
        return this.documentApiService.deleteDocumentBatch(docBatchReq).subscribe(
            res => {
                this.updateData(true);
                this.sharedService.setSuccessAlert("DOCUMENT.DELETEDS_SUCCESSFULY");
                this.sharedService.setBusy(false);
            }, err => {
                this.sharedService.setErrorAlert(new Errors(err.error));
                this.sharedService.setBusy(false);
            }, () => {
            }
        );
    }

    public cancelDocument($event, document: documentCollections) {
        $event.preventDefault();
        $event.stopPropagation();

        this.deleteModal = new Modal({ showModal: true });

        this.translate.get(['DOCUMENT.MODAL_CANCEL_TITLE', 'DOCUMENT.MODAL_CANCEL_TEXT', 'REGISTER.CANCEL', 'BUTTONS.SUBMIT']).subscribe((res: object) => {
            let keys = Object.keys(res);
            this.deleteModal.title = res[keys[0]];
            this.deleteModal.content = (res[keys[1]] as string).replace("{#*#}", "<span>" + document.name + "</span>");

            this.deleteModal.rejectBtnText = res[keys[2]];
            this.deleteModal.confirmBtnText = res[keys[3]];

            let confirmAction2 = new Observable(ob => {
                ob.next(document.documentCollectionId);
                ob.complete();

                return { unsubscribe() { } };
            });
            this.deleteModal.confirmAction2 = confirmAction2;

            this.modalService.showModal(this.deleteModal);
        });
    }

    public async downloadFile(collectionId: string) {
        this.sharedService.setBusy(true, "GLOBAL.DOWNLOADING");
        this.documentApiService.downloadDocument(collectionId);

    }

    public downloadTraceDocument(collectionId: string) {
        this.sharedService.setBusy(true, "GLOBAL.DOWNLOADING");
        var clientTimezoneOffset = new Date().getTimezoneOffset() / 60;//offset in hours
        this.documentApiService.downloadTraceDocument(collectionId, clientTimezoneOffset, '');
    }

    public export() {
        if (this.documentsCount > 0) {
            this.sharedService.setBusy(true, "DATA.EXPORT_EXCEL");
            this.documentApiService.export();
        }
        else {
            this.sharedService.setTranslateAlert("GLOBAL.NO_DOCUMENTS_YET", AlertLevel.ERROR);
        }
    }

    public exportfields(collectionId: string) {
        this.sharedService.setBusy(true, "DATA.EXPORT_DATA");
        this.documentApiService.exportfields(collectionId);
    }

    public edit(collectionId: string) {
        const docData = this.docStatusData.documentCollections.find((d) => d.documentCollectionId === collectionId);

        switch (docData.mode) {
            case SignMode.SelfSign:
                this.router.navigate(["/dashboard", "selfsignfields", docData.documentCollectionId]);
                break;
            case SignMode.Online:
                this.router.navigate(["/dashboard", "onlinesign", docData.documentCollectionId]);
                break;
        }
    }

    public openDocumentView(event, collectionId, firstDocId) {
        this.store.dispatch(new appActions.SetSelectedDocumentCollectionToView({ documentsCollectionId: collectionId }));
        this.router.navigate(['/dashboard', 'docview', collectionId, firstDocId])

    }

    public selfSignSigning(event, collectionId, docId) {
        this.router.navigate(["dashboard", "selfsignfields", `${collectionId}`, `${docId}`]);

    }

    public share($event: any, documentId: string, documentName: string) {
        this.showShare = true;
        $event.preventDefault();
        this.currDocumentCollectionId = documentId;
    }

    public replaceSigner($event: any, documentId: string, signerId: string) {
        this.showReplaceSigner = true;
        this.oldSignerId = signerId;
        $event.preventDefault();
        this.currDocumentCollectionId = documentId;
    }

    public hideReplaceSignerForm() {
        this.showReplaceSigner = false;
        this.updateData(true);
    }
    public ngOnDestroy() {
        this.deleteSubscription.unsubscribe();
        this.cancelSubscription.unsubscribe();
        this.documentsSubscription.unsubscribe();

    }

    trackByFn(index, item) {
        return index;
    }

    orderByFunction(prop: string) {
        if (prop) {
            if (this.orderByField == prop) {
                this.orderByDesc = !this.orderByDesc;
            }
            this.orderByField = prop;
        }
    }

    getLocalTime(utcTime: string) {
        return this.sharedService.getLocalTime(utcTime);
    }

    dateIsDefault(date: string): boolean {
        return date == '0001-01-01T00:00:00';
    }

    changeAllFilters(filter: DocFilter, setTrue: boolean) {
        filter.declined = setTrue;
        filter.sent = setTrue;
        filter.signed = setTrue;
        filter.viewed = setTrue;
        filter.canceled = setTrue;
    }

    resend($event: any, documentId: string, documentName: string, signer: signer) {
        this.isBusy = true;
        this.sharedService.setBusy(true, "DOCUMENT.RESEND");
        this.documentService.resendDocument(documentId, signer.id, signer.sendingMethod)
            .subscribe(x => {
                this.sharedService.setSuccessAlert('ERROR.OPERATION.1');
                this.isBusy = false;
                this.sharedService.setBusy(false);
                setTimeout(() => {
                    if (this.activeDocumentId === documentId) {
                        this.readRowInformation(documentId);
                    }
                }, 3500);
            },
                (err) => {
                    this.sharedService.setErrorAlert(new Errors(err.error));
                    this.isBusy = false;
                    this.sharedService.setBusy(false);
                });
    }

    reactivateDocument($event: any, documentId: string) {
        this.isBusy = true;
        this.sharedService.setBusy(true, "DOCUMENT.SIGNING");
        this.documentService.reactivateDocument(documentId)
            .subscribe(_ => {
                let documents = this.docStatusData.documentCollections.find((d) => d.documentCollectionId === documentId);
                documents.documentStatus = DocStatus.Sent;
                if (documents.mode === SignMode.Online) {
                    let signerRejected = documents.signers.find(s => s.status === SignerStatus.Rejected);
                    this.documentApiService.GetSenderLiveLink(documentId, signerRejected.id)
                        .subscribe(res => {
                            this.currentLiveSigningLink = res.link;
                            this.liveSigningRedirect();
                        });
                }
                this.isBusy = false;
                this.sharedService.setBusy(false);
            },
                (err) => {
                    this.sharedService.setErrorAlert(new Errors(err.error));
                    this.isBusy = false;
                    this.sharedService.setBusy(false);
                })
        $event.preventDefault();
    }

    downloadAttchment($event: any, documentId: string, documentName: string, signer: signer) {
        this.sharedService.setBusy(true, "DATA.DOWNLOADING");
        this.documentApiService.downloadSignerAttachments(documentId, signer.id);
    }

    extraServerSign(event, collectionId) {
        this.isBusy = true;
        this.sharedService.setBusy(true, "DOCUMENT.SIGNING")
        this.documentApiService.extraServerSign(collectionId)
            .subscribe(
                (data) => {
                    this.sharedService.setSuccessAlert('ERROR.OPERATION.1');
                    this.isBusy = false;
                    this.updateData(true);
                    this.sharedService.setBusy(false)
                },
                (err) => {
                    this.sharedService.setErrorAlert(new Errors(err.error));
                    this.isBusy = false;
                    this.sharedService.setBusy(false)
                });
    }

    openSenderLiveSigning(docID: string, signerId: string) {
        this.documentApiService.GetSenderLiveLink(docID, signerId).subscribe(
            (res) => {

                this.currentLiveSigningLink = res.link;
                this.shouldOpenLiveSigning = true;
            });
    }

    onSearchParameterChange() {
        if (this.docFilter.key != "") {
            this.pageChanged(1)
        }
    }

    liveSigningRedirect() {
        window.open(this.currentLiveSigningLink);
        this.shouldOpenLiveSigning = false;
        this.currentLiveSigningLink = "";
    }

    public onDropDownSelect(value: number) {
        this.PAGE_SIZE = value;
        this.updateData(true)
    }
}