import { HttpClient } from '@angular/common/http';
// import { StringMap } from '@angular/compiler/src/compiler_facade_interface';
import { Injectable } from '@angular/core';
import { AllDistributionDocumentsExpandedResposne, AllDistributionDocumentsResposne } from '@models/distribution-api/all-distribution-documents-resposne.model';
import { CreateDistributionDocuments } from '@models/distribution-api/create-distribution-documents.model';
import { ReadSignersFromFileResponse } from '@models/distribution-api/read-signers-from-file-response.model';
import { DocFilter } from '@models/document-api/doc-filter.model';
import { DocStatus } from '@models/enums/doc-status.enum';
import { Errors } from '@models/error/errors.model';
import { UploadRequest } from '@models/template-api/upload-request.model';
import { AppConfigService } from './app-config.service';
import { SharedService } from './shared.service';

@Injectable({
  providedIn: 'root'
})
export class DistributionApiService {

  private distributionApi: string = "";

  constructor(private httpClient: HttpClient,
    private sharedService: SharedService,
    private appConfigService: AppConfigService) {
    this.distributionApi = this.appConfigService.apiUrl + "/distribution";
  }

  public readSignersFromFile(request: UploadRequest) {
    return this.httpClient.post<ReadSignersFromFileResponse>(`${this.distributionApi}/signers`, request);
  }

  public distributionMechanism(request: CreateDistributionDocuments) {
    return this.httpClient.post(this.distributionApi, request);
  }

  public getDocuments(docFilter: DocFilter) {
    return this.httpClient.get<AllDistributionDocumentsResposne>(`${this.distributionApi}?key=${docFilter.key}` +
      (docFilter.from != null ? `&from=${docFilter.from.toISOString()}` : "") +
      (docFilter.to != null ? `&to=${docFilter.to.toISOString()}` : "") +
      `&offset=${docFilter.offset}&limit=${docFilter.limit}`, { observe: "response" });
  }

  public getDocumentsExpandedInfo(docId: string, docFilter : DocFilter) {
  
    return this.httpClient.get<AllDistributionDocumentsExpandedResposne>(`${this.distributionApi}/${docId}?key=${docFilter.key}` +
      (docFilter.from != null ? `&from=${docFilter.from.toISOString()}` : "") +
      (docFilter.to != null ? `&to=${docFilter.to.toISOString()}` : "") +
      `&offset=${docFilter.offset}&limit=${docFilter.limit}`, { observe: "response" });
  }

  public download(distributionId: string) {
    return this.httpClient.get(`${this.distributionApi}/download/${distributionId}`, {
      observe: "response",
      responseType: "arraybuffer",
    });
  }

  public resend(distributionId: string) {
    return this.httpClient.get(`${this.distributionApi}/resend/${distributionId}`);
  }


  public resendInStatus(distributionId: String, docStatus : DocStatus) {
    return this.httpClient.get(`${this.distributionApi}/resend/${distributionId}/status/${docStatus}`);
  }
  public delete(distributionId: string) {
    return this.httpClient.delete(`${this.distributionApi}/${distributionId}`);
  }

}
