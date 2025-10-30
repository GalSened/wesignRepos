import { Injectable } from '@angular/core';
import { AppConfigService } from './app-config.service';
import { SingleLinkResponse, SingleLinkDataResponseDTO } from '../models/responses/single-link-response.model';
import { SingleLinkRequest } from '../models/requests/single-link-request.model';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class SingleLinkService {

  private singleLinkApi: string;

  constructor(private httpClient: HttpClient,
    private appConfigService: AppConfigService) {
    this.singleLinkApi = this.appConfigService.apiUrl + "/singlelink";
  }

  public createDocuemnt(input: SingleLinkRequest) {
    return this.httpClient.post<SingleLinkResponse>(`${this.singleLinkApi}`, input);
  }

  public getData(templateId: string) {
    return this.httpClient.get<SingleLinkDataResponseDTO>(`${this.singleLinkApi}/${templateId}`);
  }
}