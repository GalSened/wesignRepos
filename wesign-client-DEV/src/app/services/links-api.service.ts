import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { DocFilter } from '@models/document-api/doc-filter.model';
import { UserSigningLinksResponse } from '@models/document-api/user-signing-links-response.model';
import { AppConfigService } from './app-config.service';
import { VideoConfrenceRequestDTO, VideoConfrenceResponseDTO } from '@models/contacts/select-distinct-contact-video-confrence.model';
import { TemplateSingleLink } from '@models/template-api/template-single-link.model';
import { BaseResult } from '@models/base/base-result.model';

@Injectable({
  providedIn: 'root'
})
export class LinksApiService {
  private linksApi: string = "";

  constructor(private httpClient: HttpClient,
    private appConfigService: AppConfigService) {
    this.linksApi = this.appConfigService.apiUrl + "/links";
  }
  public updateSingleLinkAttachments(templateSingleLink: TemplateSingleLink) {
       return this.httpClient.post<BaseResult>(`${this.linksApi}/template/${templateSingleLink.templateId}`, templateSingleLink); 
    }
    public getSingleLinkAttachments(templateId: string) {
       return this.httpClient.get<TemplateSingleLink>(`${this.linksApi}/template/${templateId}`);
    }
public createVideoConference(videoConfrenceRequest :VideoConfrenceRequestDTO){
  return this.httpClient.post<VideoConfrenceResponseDTO>(`${this.linksApi}/videoConference`, videoConfrenceRequest);
}


  public getDocuments(filter: DocFilter) {
    return this.httpClient.get<UserSigningLinksResponse>(
        `${this.linksApi}?key=${filter.key}` +        
        `&offset=${filter.offset}&limit=${filter.limit}`, { observe: "response" });
  }
}
