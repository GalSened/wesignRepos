import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AppConfigService } from './app-config.service';
import { ActiveDirectoryGroups } from '../models/active-directoy-groups.model';

@Injectable({
  providedIn: 'root'
})
export class ActiveDirectoryService {

  private activeDirectoyApi :string;

  constructor(private httpClient: HttpClient,
    private appConfigService: AppConfigService) {
    this.activeDirectoyApi = this.appConfigService.apiUrl + "/activedirectory";
}
 
public ReadAddADGroups(){
  return  this.httpClient.get<ActiveDirectoryGroups>(`${this.activeDirectoyApi}/groups`);
}
}
