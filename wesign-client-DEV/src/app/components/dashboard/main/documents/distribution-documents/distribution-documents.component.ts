import { Component, HostBinding, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { DistributionDocument, DistributionDocumentExpanded } from '@models/distribution-api/all-distribution-documents-resposne.model';
import { DocFilter } from '@models/document-api/doc-filter.model';
import { Store } from '@ngrx/store';
import { DistributionApiService } from '@services/distribution-api.service';
import { PagerService } from '@services/pager.service';
import { SharedService } from '@services/shared.service';
import { IAppState } from '@state/app-state.interface';
import * as appActions from "@state/actions/app.actions";
import { DocumentApiService } from '@services/document-api.service';
import { Errors } from '@models/error/errors.model';
import { DocStatus } from '@models/enums/doc-status.enum';
import { Modal } from '@models/modal/modal.model';
import { TranslateService } from '@ngx-translate/core';
import { documentCollections } from '@models/document-api/documentsCollection.model';
import { SendingMethod } from '@models/contacts/contact.model';

@Component({
  selector: 'sgn-distribution-documents',
  templateUrl: './distribution-documents.component.html',
  styles: []
})
export class DistributionDocumentsComponent implements OnInit {
  @HostBinding('style.width') width = '100%';

  public distributionDocFilter: DocFilter = new DocFilter();
  public distributionDocInlineFilter: DocFilter = new DocFilter();
  private INLINE_PAGE_SIZE = 10;
  public currentPage = 1;
  public innerCurrentPage = 1;
  private PAGE_SIZE = 10;
  public distributionDocumentsCount: number;
  public innerDistributionDocumentsCount: number;
  public pageCalc: any;
  public rowCalc: any;
  public distributionDocs: DistributionDocument[];
  public activeDistributionDocumentId: string = "";
  public distributionInnerDocs: DistributionDocumentExpanded[];
  public DocStatus = DocStatus;
  public deleteSinglePopupData: Modal = new Modal();
  public deleteAllPopupData: Modal = new Modal();
  public documentCollectionForDelete: DistributionDocumentExpanded;
  public distributionIdForDelete: any;
  public orderByField: string = 'creationTime';
  public orderByDesc: boolean = true;
  public showSearchSpinner: boolean = false;
  public totalPending = 0;
  public totalDecline = 0;
  public totalSigned = 0;
  public totalServerSigned = 0;
  public totalViewed = 0;
  public totalfailed = 0;
  totalCreatedButNotSent = 0;
  shouldSignUsingSigner1AfterDocumentSigningFlow: boolean;
  isBusy: boolean;

  constructor(private sharedService: SharedService, private documentApiService: DocumentApiService,
    private router: Router, private translate: TranslateService, private store: Store<IAppState>,
    private pager: PagerService, private distributionApiService: DistributionApiService) { }

  ngOnInit() {
    this.updateData(true);
  }

  public rowPageChanged(page: number) {
    this.showSearchSpinner = true
    this.innerCurrentPage = page;
    this.updateInnerDataData(false);
  }
  public pageChanged(page: number) {
    this.showSearchSpinner = true
    this.currentPage = page;
    this.updateData(false);
  }

  public updateData(showLoading) {
    this.distributionDocFilter.limit = this.PAGE_SIZE;
    this.distributionDocFilter.offset = (this.currentPage - 1) * this.PAGE_SIZE;
    if (showLoading) {
      this.sharedService.setBusy(true, "DOCUMENT.LOADING")

    }
    this.distributionApiService.getDocuments(this.distributionDocFilter)
      .subscribe((data) => {
        this.distributionDocumentsCount = +data.headers.get("x-total-count");
        this.pageCalc = this.pager.getPager(this.distributionDocumentsCount, this.currentPage, this.PAGE_SIZE);
        this.distributionDocs = data.body.documentCollections;

      },
        error => {
          let ex = new Errors(error.error);
          this.sharedService.setErrorAlert(ex);

        },
        () => {
          this.sharedService.setBusy(false);
          this.showSearchSpinner = false;
        });
  }

  public trackByFn(index, item) {
    return index;
  }

  public updateInnerDataData(showLoading) {
    this.distributionDocInlineFilter.limit = this.INLINE_PAGE_SIZE;
    this.distributionDocInlineFilter.offset = (this.innerCurrentPage - 1) * this.INLINE_PAGE_SIZE;
    if (showLoading) {
      this.sharedService.setBusy(true, "DOCUMENT.LOADING")

    }
    this.distributionApiService.getDocumentsExpandedInfo(this.activeDistributionDocumentId, this.distributionDocInlineFilter)
      .subscribe((data) => {

        this.sharedService.setBusy(false);
        this.distributionInnerDocs = data.body.documentCollections;
        this.innerDistributionDocumentsCount = +data.headers.get("x-total-count");
        this.rowCalc = this.pager.getPager(this.innerDistributionDocumentsCount, this.innerCurrentPage, this.INLINE_PAGE_SIZE);
        this.totalPending = data.body.totalPending;
        this.totalDecline = data.body.totalDeclined;
        this.totalSigned = data.body.totalSigned;
        this.totalServerSigned = data.body.totalServerSigned;
        this.totalfailed = data.body.totalFailed;
        this.totalViewed = data.body.totalViewed;
        this.totalCreatedButNotSent = data.body.totalCreatedButNotSent;
        this.shouldSignUsingSigner1AfterDocumentSigningFlow = data.body.shouldSignUsingSigner1AfterDocumentSigningFlow;
        let documentsStatus: documentCollections[] = [];
        this.distributionInnerDocs.forEach(element => {
          let doc = new documentCollections();
          doc.documentCollectionId = element.documentCollectionId;
          doc.documentStatus = element.documentStatus;
          doc.documentsIds = element.documentsIds;
          documentsStatus.push(doc);
        });
        this.store.dispatch(new appActions.SetDocumentListStatus({ documentsStatus: documentsStatus }));

      },
        error => {
          let ex = new Errors(error.error);
          this.sharedService.setErrorAlert(ex);
          this.sharedService.setBusy(false);
        },
        () => {
          this.sharedService.setBusy(false);
          this.showSearchSpinner = false;
        });
  }

  public onRowClick(distributionId: string) {

    this.activeDistributionDocumentId = this.activeDistributionDocumentId === distributionId ? "" : distributionId;
    this.distributionDocInlineFilter.limit = this.INLINE_PAGE_SIZE;
    this.innerCurrentPage = 1;
    if (this.activeDistributionDocumentId == "") {
      return;
    }
    this.updateInnerDataData(true)
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

  public dateIsDefault(date: string): boolean {
    return date == '0001-01-01T00:00:00';
  }

  public downloadAllSignedDocs(distributionId) {
    this.sharedService.setBusy(true, "GLOBAL.DOWNLOADING");
    this.distributionApiService.download(distributionId)
      .subscribe((data) => {
        let fn = data.headers.get("x-file-name");
        const filename = decodeURIComponent(fn.replace(/\+/g, ' ')) ? decodeURIComponent(fn.replace(/\+/g, ' ')) : "doc_dc1";
        let blob = null;
        blob = new Blob([data.body], { type: "application/zip" });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.target = "_blank";
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        this.sharedService.setBusy(false);
      },
        error => {
          let ex = this.sharedService.convertArrayBufferToErrorsObject(error.error);
          this.sharedService.setErrorAlert(ex);
          this.sharedService.setBusy(false);
        }
      );
  }

  public extraServerSign(event, collectionId) {
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

  public resendFailedToSendDocs(distributionId: string) {
    this.sharedService.setBusy(true, "DOCUMENT.SENDING");
    this.distributionApiService.resendInStatus(distributionId, DocStatus.Created).subscribe(
      data => {
        this.sharedService.setSuccessAlert("GLOBAL.SUCCESS_DISTRIBUTION_REQUEST");
        this.sharedService.setBusy(false);
      },
      err => {
        this.sharedService.setErrorAlert(new Errors(err.error));
        this.sharedService.setBusy(false);
      }
    )
  }

  //

  public resendUnsignedDocs(distributionId: string) {
    this.sharedService.setBusy(true, "DOCUMENT.SENDING");
    this.distributionApiService.resend(distributionId).subscribe(
      data => {
        this.sharedService.setSuccessAlert("GLOBAL.SUCCESS_DISTRIBUTION_REQUEST");
        this.sharedService.setBusy(false);
      },
      err => {
        this.sharedService.setErrorAlert(new Errors(err.error));
        this.sharedService.setBusy(false);
      }
    )
  }


  public openDocumentView(event, collectionId, firstDocId) {
    this.store.dispatch(new appActions.SetSelectedDocumentCollectionToView({ documentsCollectionId: collectionId }));
    this.router.navigate(['/dashboard', 'docview', collectionId, firstDocId])
  }

  public async downloadFile(collectionId: string, additinal: string) {
    this.sharedService.setBusy(true, "GLOBAL.DOWNLOADING");
    this.documentApiService.downloadDocument(collectionId, additinal);
  }

  public resend($event, documentCollectionId: string, signerId: string, sendingMethod: SendingMethod) {
    this.sharedService.setBusy(true, "DOCUMENT.SENDING");
    this.documentApiService.resendDocument(documentCollectionId, signerId, sendingMethod).subscribe(
      data => {
        this.sharedService.setSuccessAlert("GLOBAL.SUCCESS_DISTRIBUTION_REQUEST");
        this.sharedService.setBusy(false);
      },
      err => {
        this.sharedService.setErrorAlert(new Errors(err.error));
        this.sharedService.setBusy(false);
      }
    )
  }

  public downloadTraceDocument(collectionId: string, additinal: string) {
    this.sharedService.setBusy(true, "GLOBAL.DOWNLOADING");
    var clientTimezoneOffset = new Date().getTimezoneOffset() / 60;//offset in hours
    this.documentApiService.downloadTraceDocument(collectionId, clientTimezoneOffset, additinal);
  }

  public deleteDocument($event, document: DistributionDocumentExpanded) {
    $event.preventDefault();
    $event.stopPropagation();
    this.deleteSinglePopupData.showModal = true;
    this.translate.get(['DOCUMENT.MODALTITLE', 'DOCUMENT.MODALTEXT', 'REGISTER.CANCEL', 'GLOBAL.DELETE'])
      .subscribe((res: object) => {
        let keys = Object.keys(res);
        this.deleteSinglePopupData.title = res[keys[0]];
        this.deleteSinglePopupData.content = (res[keys[1]] as string).replace("{#*#}", document.name);
        this.deleteSinglePopupData.rejectBtnText = res[keys[2]];
        this.deleteSinglePopupData.confirmBtnText = res[keys[3]];
      });
    this.documentCollectionForDelete = document;
  }

  deleteInnerDocEvent() {
    this.sharedService.setBusy(true, "DOCUMENT.DELETING");
    this.deleteSinglePopupData.showModal = false;
    this.documentApiService.deleteDocument(this.documentCollectionForDelete.documentCollectionId).subscribe(
      data => {
        this.sharedService.setSuccessAlert("DOCUMENT.DELETED_SUCCESSFULY");
        this.sharedService.setBusy(false);
        this.onRowClick(this.documentCollectionForDelete.distributionId);
        this.updateData(true);
      },
      err => {
        this.sharedService.setErrorAlert(new Errors(err.error));
        this.sharedService.setBusy(false);
      }
    )
  }

  public deleteAll(distributionDoc: DistributionDocument) {
    this.distributionIdForDelete = distributionDoc.distributionId;
    this.deleteAllPopupData.showModal = true;
    this.translate.get(['DOCUMENT.MODALTITLE', 'DOCUMENT.MODALTEXT', 'REGISTER.CANCEL', 'GLOBAL.DELETE'])
      .subscribe((res: object) => {
        let keys = Object.keys(res);
        this.deleteAllPopupData.title = res[keys[0]];
        this.deleteAllPopupData.content = (res[keys[1]] as string).replace("{#*#}", distributionDoc.name);
        this.deleteAllPopupData.rejectBtnText = res[keys[2]];
        this.deleteAllPopupData.confirmBtnText = res[keys[3]];
      });
  }

  deleteAllDocsEvent() {
    this.deleteAllPopupData.showModal = false;
    this.sharedService.setBusy(true, "DOCUMENT.DELETING");
    this.distributionApiService.delete(this.distributionIdForDelete).subscribe(
      data => {
        this.sharedService.setSuccessAlert("DOCUMENT.DELETED_SUCCESSFULY");
        this.sharedService.setBusy(false);
        this.updateData(true);
      },
      err => {
        this.sharedService.setErrorAlert(new Errors(err.error));
        this.sharedService.setBusy(false);
      }
    )
  }
  public onDropDownInnerSelect(value: number) {
    this.INLINE_PAGE_SIZE = value;
    this.updateInnerDataData(true)
  }

  public onDropDownSelect(value: number) {
    this.PAGE_SIZE = value;
    this.updateData(true)
  }
}