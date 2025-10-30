import { AppConfigService } from './app-config.service';
import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { BaseResult } from "@models/base/base-result.model";
import { PageDataResult, TemplatePagesRangeResponse } from "@models/template-api/page-data-result.model";
import { TemplateFilter } from "@models/template-api/template-filter.model";
import { TemplateInfos } from "@models/template-api/template-infos.model";
import { TemplatePagesResult } from "@models/template-api/template-pages-result.model";
import { UpdateTemplateRequest } from "@models/template-api/update-template-request.model";
import { UploadRequest } from "@models/template-api/upload-request.model";
import { UploadResult } from "@models/template-api/upload-result.model";
import { Observable } from 'rxjs';
import { DuplicateTemplateResult } from '@models/template-api/duplicate-template-result-model';
import { SharedService } from './shared.service';
import { BatchRequest } from '@models/document-api/batch-request.model';
import { MergeTemplatesRequest } from '@models/template-api/merge-templates-request.model';

@Injectable()
export class TemplateApiService {

    private templateApi : string = "";
    constructor(private httpClient: HttpClient,
        private sharedService: SharedService,
        private appConfigService: AppConfigService) {
        this.templateApi = this.appConfigService.apiUrl + "/templates";
    }

    public getRecentlyUsedTemplates() {
        return this.httpClient.get<TemplateInfos>(`${this.templateApi}?recent=true&limit=4`);
    }

    public getPopularTemplates() {
        return this.httpClient.get<TemplateInfos>(`${this.templateApi}?popular=true&limit=4`);
    }

    public upload(uploadRequest: UploadRequest) {
        return this.httpClient.post<UploadResult>(`${this.templateApi}`, uploadRequest);
    }

    public pageCount(templateId: string) : Observable<TemplatePagesResult>{
        return this.httpClient.get<TemplatePagesResult>(`${this.templateApi}/${templateId}/pages`);
    }

    public getPage(templateId: string, page: number) {
        return this.httpClient.get<PageDataResult>(`${this.templateApi}/${templateId}/pages/${page}`);
    }

    public getPages(templateId: string,  offset: number, limit: number) {
        return this.httpClient.get<TemplatePagesRangeResponse>(`${this.templateApi}/${templateId}/pages/range?offset=${offset}&limit=${limit}`);
    }

    public update(templateUpdateRequest: UpdateTemplateRequest) { 
        return this.httpClient.put<BaseResult>(`${this.templateApi}/${templateUpdateRequest.id}`, templateUpdateRequest);
    }

    public delete(templateId: string) {
        return this.httpClient.delete<BaseResult>(`${this.templateApi}/${templateId}`);
    }

    public mergeDocuments(mergeTemplatesRequest : MergeTemplatesRequest)
    {
        return this.httpClient.post<UploadResult>(`${this.templateApi}/merge`,mergeTemplatesRequest);        
    }
    public getTemplates(filter: TemplateFilter) {
        return this.httpClient.get<TemplateInfos>(
            `${this.templateApi}?key=${filter.Search}` +
            (filter.From != null ? `&from=${filter.From.toISOString()}` : "") +
            (filter.To != null ? `&to=${filter.To.toISOString()}` : "") +
            `&offset=${filter.Offset}&limit=${filter.Limit}`, { observe: "response" });
    }

    public duplicate(templateId: string, isOneTimeUseTemplate : boolean = false) {
        return this.httpClient.post<DuplicateTemplateResult>(`${this.templateApi}/${templateId}`, {"IsOneTimeUseTemplate" : isOneTimeUseTemplate});
    }

    public downloadTemplate(id: string) {
        return this.httpClient.get(`${this.templateApi}/${id}/download`, {
            observe: "response",
            responseType: "arraybuffer",
        });
    }

    
    public deleteDocumentBatch(  templatesBatchReq :   BatchRequest)
    {
        return this.httpClient.put<BaseResult>(`${this.templateApi}/deletebatch`,templatesBatchReq );
    }

    public forceDownloadTemplate(id: string) {
        this.httpClient.get(`${this.templateApi}/${id}/download`, {
            observe: "response",
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
            this.sharedService.setBusy(false);
        },
        err=>{
            let ex = this.sharedService.convertArrayBufferToErrorsObject(err.error);
            this.sharedService.setErrorAlert(ex);
            this.sharedService.setBusy(false);
        }
        );
    }
}
