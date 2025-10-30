import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { AppConfigService } from './app-config.service';
import { CreateAuthFlowModel, CreateAuthFlowResultModel, IdentityCheckFlow, IdentityCheckFlowResultModel } from '../models/requests/identity-flow-create.model';
import { SplitDocumentProcessResult } from '../models/responses/split-document-process-result.model';

@Injectable({
  providedIn: 'root'
})
export class IdentificationService {
  
  private identificationSignerApi :string;
  constructor(private httpClient: HttpClient,
    private router: Router,
    private appConfigService: AppConfigService) {
      this.identificationSignerApi = this.appConfigService.apiUrl + "/identification/";
}  

public CheckidentityFlowEIDASSign( input: IdentityCheckFlow)
  {
    return this.httpClient.post<SplitDocumentProcessResult>(`${this.identificationSignerApi}CheckidentityFlowEIDASSign`, input);
  }

public CreateidentityFlowEIDASSign( input: CreateAuthFlowModel) 
  {
    return this.httpClient.post<CreateAuthFlowResultModel>(`${this.identificationSignerApi}CreateidentityFlowEIDASSign`, input);
  }

public CreateidentityFlow( input: CreateAuthFlowModel){
  return this.httpClient.post<CreateAuthFlowResultModel>(`${this.identificationSignerApi}createidentityFlow`, input);
}


public checkidentityFlow( input: IdentityCheckFlow){
  return this.httpClient.post<IdentityCheckFlowResultModel>(`${this.identificationSignerApi}CheckIdentityFlow`, input);
  }
}
