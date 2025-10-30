import { Router } from '@angular/router';
import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { HttpClient } from "@angular/common/http";
import { DocumentCollectionDataResponse } from '../models/responses/document-collection-count.model';
import { UpdateDocumentCollectionRequest } from '../models/requests/update-document-collection-request.model';
import { DocumentPage } from '../models/responses/document-page.model';
import { UpdateResponse } from '../models/responses/update-response.model';
import { AppConfigService } from './app-config.service';
import { off } from 'process';
import { DocumentCollectionHtmlDataResponse } from '../models/responses/document-collectiond-ata-html-response.model';
import { DocumentCollectionDataFlowInfoResponse } from '../models/responses/document-collection-flow-info.model';
import {  CreateAuthFlowModel, CreateAuthFlowResultModel, IdentityCheckFlow, IdentityCheckFlowResultModel } from '../models/requests/identity-flow-create.model';

@Injectable({
  providedIn: 'root'
})
export class DocumentsService {

  private documentSignerApi :string;
  public releseButton = new Subject();

  constructor(private httpClient: HttpClient,
              private router: Router,
              private appConfigService: AppConfigService) {
                this.documentSignerApi = this.appConfigService.apiUrl + "/documentCollections/";
  }  

  public getCollectionData(token: string): Observable<DocumentCollectionDataResponse> {
    return this.httpClient.get<DocumentCollectionDataResponse>(`${this.documentSignerApi}${token}`);
  }


  public getCollectionDataFlowInfo(token: string): Observable<DocumentCollectionDataFlowInfoResponse> {
    return this.httpClient.get<DocumentCollectionDataFlowInfoResponse>(`${this.documentSignerApi}${token}/flowinfo`);
  }

  public getDocumentsHtmlData(token: string, documentId: string) {
    return this.httpClient.get<DocumentCollectionHtmlDataResponse>(`${this.documentSignerApi}${token}/documents/${documentId}/html`);
  }

  public getDocumentsData(token: string, documentId: string, offset: number, limit: number) {
    return this.httpClient.get<DocumentPage>(`${this.documentSignerApi}${token}/documents/${documentId}?offset=${offset}&limit=${limit}`);
  }

  
  public downloadDocument(token: string) {
    this.httpClient.get(`${this.documentSignerApi}${token}/download`, {
      observe: "response",
      // responseType: "text",
      responseType: "arraybuffer",
    }).subscribe((data) => {
      let fn = data.headers.get("x-file-name");
      const filename = decodeURIComponent(fn.replace(/\+/g, ' ')) ? decodeURIComponent(fn.replace(/\+/g, ' ')) : "doc_dc1";
      let blob = null;
      if(filename.toLocaleLowerCase().endsWith("zip"))
      {
           blob = new Blob([data.body], { type: "application/zip"  });
      }
      else
      {
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
      this.releseButton.next();
    },
    (err)=>{
      //TODO if failed to download, redirect to another page 
      this.router.navigate(["/"]);
      this.releseButton.next();
    });
  }

  public downloadAppendix(token: string, appendixName :string) {
    this.httpClient.get(`${this.documentSignerApi}${token}/appendix/${appendixName}`, {
      observe: "response",      
      responseType: "arraybuffer",
    }).subscribe((data) => {
      let fn = data.headers.get("x-file-name");
      let contentType = data.headers.get("x-file-content");

      let filename = decodeURIComponent(fn.replace(/\+/g, ' ')) ? decodeURIComponent(fn.replace(/\+/g, ' ')) : "doc_dc1";
      const blob = new Blob([data.body],{type: contentType });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement("a");

      if(contentType == "application/vnd.ms-outlook" && !filename.toLocaleLowerCase().endsWith("msg"))
      {
        filename += ".msg";
      }
        a.href = url
      a.href = url;
      a.target = "_blank";
      a.download = filename;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
    },
    (err)=>{
      //TODO if failed to download, redirect to another page 
      this.router.navigate(["/"]);

    });
  }

  public downloadSmartCardDesktopClientInstaller() {
    return this.httpClient.get(`${this.documentSignerApi}download/smartcard`, {
      observe: "response",
      responseType: "arraybuffer",
    });
  }

  public updateDocument(token: string, input: UpdateDocumentCollectionRequest) {
    return this.httpClient.put<UpdateResponse>(`${this.documentSignerApi}${token}`, input);
  }

 

}
