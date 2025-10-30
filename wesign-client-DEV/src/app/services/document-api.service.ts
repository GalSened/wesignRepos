import { AppConfigService } from './app-config.service';
import { HttpClient, HttpHeaders, HttpResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { BaseResult } from "@models/base/base-result.model";
import { DocFilter } from "@models/document-api/doc-filter.model";
import { DocInfoResult } from "@models/document-api/doc-info-result.model";
import { DocUpdateRequest } from "@models/document-api/doc-update-request.model";
import { DocumentCollectionCreateRequest } from "@models/document-api/document-create-request.model";
import { DocumentCreateResult } from "@models/document-api/document-create-result.model";
import { DocumentsStatus } from "@models/document-api/documents-status.model";
import { DocumentPagesRangeResponse, PageDataResult } from "@models/template-api/page-data-result.model";
import { map } from 'rxjs/operators';
import { Subject } from 'rxjs';
import { DocumentResend } from '@models/document-api/document-resend.model';
import { SendingMethod } from '@models/contacts/contact.model';
import { SimpleDocument } from '@models/document-api/document-simple.model';
import { ShareRequest } from '@models/document-api/share-request.model';
import { ReplaceSignerRequest } from '@models/document-api/replace-signer-request';
import { SharedService } from './shared.service';
import { Errors } from '@models/error/errors.model';
import { BatchRequest } from '@models/document-api/batch-request.model';
import { documentCollections } from '@models/document-api/documentsCollection.model';

@Injectable({ providedIn: 'root' })
export class DocumentApiService {

    private documentApi = "";
    private sidebarBS = new Subject<DocumentResend>();
    public sidebarData = this.sidebarBS.asObservable();

    constructor(private httpClient: HttpClient, private sharedService: SharedService, private appConfigService: AppConfigService) {
        this.documentApi = this.appConfigService.apiUrl + "/documentcollections";
    }

    getDocument(collectionId: string) {
        return this.httpClient.get<documentCollections>(
            `${this.documentApi}/info/${collectionId}`);
    }


    getDocuments(docFilter: DocFilter) {
        return this.httpClient.get<DocumentsStatus>(
            `${this.documentApi}?sent=${docFilter.sent}&viewed=${docFilter.viewed}` +
            `&signed=${docFilter.signed}&declined=${docFilter.declined}&sendingFailed=${docFilter.sendingFailed}&canceled=${docFilter.canceled}&userId=${docFilter.userId}` +
            `&key=${docFilter.key}` +
            (docFilter.from != null ? `&from=${docFilter.from.toISOString()}` : "") +
            (docFilter.to != null ? `&to=${docFilter.to.toISOString()}` : "") +
            `&offset=${docFilter.offset}&limit=${docFilter.limit}&searchParameter=${docFilter.searchParameter}`, { observe: "response" });
    }

    documentCreate(request: DocumentCollectionCreateRequest) {
        return this.httpClient.post<DocumentCreateResult>(`${this.documentApi}`, request);
    }

    pageCount(collectionId: string, documentId: string) {
        return this.httpClient.get<DocInfoResult>(`${this.documentApi}/${collectionId}/documents/${documentId}/pages`);
    }

    getPage(collectionId: string, documentId: string, page: number) {
        return this.httpClient.get<PageDataResult>(`${this.documentApi}/${collectionId}/documents/${documentId}/pages/${page}`);
    }

    getPages(collectionId: string, documentId: string, offset: number, limit: number, inViewMode: boolean) {
        return this.httpClient.get<DocumentPagesRangeResponse>(`${this.documentApi}/${collectionId}/documents/${documentId}?offset=${offset}&limit=${limit}&inViewMode=${inViewMode}`);
    }

    docUpdate(collectionId: string, request: DocUpdateRequest) {
        return this.httpClient.put<BaseResult>(`${this.documentApi}/${collectionId}`, request);
    }

    deleteDocument(collectionId: string) {
        return this.httpClient.delete<BaseResult>(`${this.documentApi}/${collectionId}`);
    }

    deleteDocumentBatch(docBatchRequest: BatchRequest) {
        return this.httpClient.put<BaseResult>(`${this.documentApi}/deletebatch`, docBatchRequest);
    }

    cancelDocument(collectionId: string) {
        return this.httpClient.put(`${this.documentApi}/${collectionId}/cancel`, "");
    }

    documentBlob(collectionId: string) {
        return this.httpClient.get(`${this.documentApi}/${collectionId}`, {
            observe: "response",
            responseType: 'arraybuffer'
        })
            .pipe(map((result: any) => {
                return result.body;
            }));
    }

    resendDocument(collectionId: string, signerId: string, sendingMethod: SendingMethod) {
        return this.httpClient.get(`${this.documentApi}/${collectionId}/signers/${signerId}/method/${sendingMethod}`);
    }

    reactivateDocument(collectionId: string) {
        return this.httpClient.get(`${this.documentApi}/${collectionId}/reactivate`);
    }

    replaceSigner(collectionId: string, signerId: string, request: ReplaceSignerRequest) {
        return this.httpClient.put(`${this.documentApi}/${collectionId}/signer/${signerId}/replace`, request);
    }

    shareDocument(request: ShareRequest) {
        return this.httpClient.post(`${this.documentApi}/share`, request);
    }

    downloadDocument(collectionId: string, additional: string = '') {
        this.httpClient.get(`${this.documentApi}/${collectionId}`, {
            observe: "response",
            responseType: "arraybuffer",
        }).subscribe((data) => {
            let fn = data.headers.get("x-file-name");
            let filename = decodeURIComponent(fn.replace(/\+/g, ' ')) ? decodeURIComponent(fn.replace(/\+/g, ' ')) : "doc_dc1";
            if (additional) {
                let splitterIndex = filename.lastIndexOf(".");
                let fileNameWithAdditinal = `${filename.substring(0, splitterIndex)}_${decodeURIComponent(additional.replace(/\+/g, ' '))}${filename.substring(splitterIndex)}`;
                filename = fileNameWithAdditinal;
            }
            let blob = null;
            if (filename.toLocaleLowerCase().endsWith("zip")) {
                blob = new Blob([data.body], { type: "application/zip" });
            }
            else {
                blob = new Blob([data.body], { type: "application/pdf" });
            }

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
            err => {
                let ex = this.sharedService.convertArrayBufferToErrorsObject(err.error);
                this.sharedService.setErrorAlert(ex);
                this.sharedService.setBusy(false);
            }
        );
    }

    downloadDocuments(docBatchRequest: BatchRequest, additional: string = '',) {
        return this.httpClient.post<Blob>(`${this.documentApi}/downloadbatch`, docBatchRequest, {
            observe: "response",
            responseType: 'blob' as 'json'
        }).subscribe((data) => {
            let fn = data.headers.get("x-file-name");
            let filename = decodeURIComponent(fn.replace(/\+/g, ' ')) ? decodeURIComponent(fn.replace(/\+/g, ' ')) : "doc_dc1";
            if (additional) {
                let splitterIndex = filename.lastIndexOf(".");
                let fileNameWithAdditinal = `${filename.substring(0, splitterIndex)}_${decodeURIComponent(additional.replace(/\+/g, ' '))}${filename.substring(splitterIndex)}`;
                filename = fileNameWithAdditinal;
            }
            let blob = null;
            if (filename.toLocaleLowerCase().endsWith("zip")) {
                blob = new Blob([data.body], { type: "application/zip" });
            }
            else {
                blob = new Blob([data.body], { type: "application/pdf" });
            }

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
            err => {
                let ex = this.sharedService.convertArrayBufferToErrorsObject(err.error);
                this.sharedService.setErrorAlert(ex);
                this.sharedService.setBusy(false);
            });
    }

    downloadSignerAttachments(collectionId: string, signerId: string) {
        this.httpClient.get(`${this.documentApi}/${collectionId}/signer/${signerId}`, {
            observe: "response",
            responseType: "arraybuffer",
        }).subscribe((data) => {
            let fn = data.headers.get("x-file-name") + ".zip";
            const filename = decodeURIComponent(fn.replace(/\+/g, ' ')) ? decodeURIComponent(fn.replace(/\+/g, ' ')) : "doc_dc1.zip";
            const blob = new Blob([data.body], { type: "application/zip" });
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
            (error) => {
                let ex = this.sharedService.convertArrayBufferToErrorsObject(error.error);
                this.sharedService.setErrorAlert(ex);
                this.sharedService.setBusy(false);
            });
    }

    downloadTraceDocument(collectionId: string, clientTimezoneOffset, additinalName: string) {
        this.httpClient.get(`${this.documentApi}/${collectionId}/audit/${clientTimezoneOffset}`, {
            observe: "response",
            responseType: "arraybuffer",
        }).subscribe((data) => {
            this.sharedService.setBusy(true, "DATA.DOWNLOADING");
            let fn = data.headers.get("x-file-name");
            let filename = decodeURIComponent(fn.replace(/\+/g, ' ')) ? decodeURIComponent(fn.replace(/\+/g, ' ')) : "doc_dc1";
            if (additinalName) {
                filename = filename + "_" + decodeURIComponent(additinalName.replace(/\+/g, ' '));
            }

            const blob = new Blob([data.body], { type: "application/pdf" });
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement("a");
            a.href = url;
            a.target = "_blank";
            a.download = filename;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            this.sharedService.setBusy(false);
            this.sharedService.setBusy(false);
        }, err => {
            let ex = this.sharedService.convertArrayBufferToErrorsObject(err.error);
            this.sharedService.setErrorAlert(ex);
            this.sharedService.setBusy(false);
        });
    }

    exportfields(collectionId: string) {
        this.httpClient.get(`${this.documentApi}/${collectionId}/fields/CsvXml`, {
            observe: "response",
            responseType: "arraybuffer",
        }).subscribe((data) => {
            let fn = data.headers.get("x-file-name") + ".zip";
            const filename = decodeURIComponent(fn.replace(/\+/g, ' ')) ? decodeURIComponent(fn.replace(/\+/g, ' ')) : "export_data_" + Date.now() + ".zip";
            const blob = new Blob([data.body], { type: "application/zip" });
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
            err => {
                let ex = this.sharedService.convertArrayBufferToErrorsObject(err.error);
                this.sharedService.setErrorAlert(ex);
                this.sharedService.setBusy(false);
            }
        );
    }


    exportDistribution(language = 1) {
        this.httpClient.get(`${this.documentApi}/exportDistribution?language=${language}`, {
            observe: "response",
            responseType: "text",
        }).subscribe((data) => {
            const filename = "exportDistribution_data_" + Date.now() + ".csv"
            const blob = new Blob(["\ufeff", data.body], { type: "text/csv" });
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
            err => {
                let ex: Errors;
                try {
                    ex = this.sharedService.convertArrayBufferToErrorsObject(err.error);
                } catch (error) {
                    ex = new Errors(JSON.parse(err.error));
                }
                this.sharedService.setErrorAlert(ex);
                this.sharedService.setBusy(false);
            });
    }

    export(sent = true, viewed = true, signed = true, declined = true, sendingFailed = true, canceled = true, language = 1) {
        this.httpClient.get(`${this.documentApi}/export?sent=${sent}&viewed=${viewed}&signed=${signed}&declined=${declined}&sendingFailed=${sendingFailed}&canceled=${canceled}&language=${language}`, {
            observe: "response",
            responseType: "text",
        }).subscribe((data) => {
            const filename = "export_data_" + Date.now() + ".csv"
            const blob = new Blob(["\ufeff", data.body], { type: "text/csv" });
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
            err => {
                let ex: Errors;
                try {
                    ex = this.sharedService.convertArrayBufferToErrorsObject(err.error);
                } catch (error) {
                    ex = new Errors(JSON.parse(err.error));
                }
                this.sharedService.setErrorAlert(ex);
                this.sharedService.setBusy(false);
            });
    }

    updateSidebarData(data: DocumentResend) {
        this.sidebarBS.next(data);
    }

    sendSimpleDocument(request: SimpleDocument) {
        return this.httpClient.post<DocumentCreateResult>(`${this.documentApi}/Simple`, request);
    }

    extraServerSign(collectionId: string) {
        return this.httpClient.put(`${this.documentApi}/${collectionId}/serversign`, "");
    }

    GetSenderLiveLink(collectionId: string, signerId: string) {
        return this.httpClient.get<any>(
            `${this.documentApi}/${collectionId}/senderLink/${signerId}`);
    }
}