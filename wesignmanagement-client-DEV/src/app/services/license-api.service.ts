import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AppConfigService } from './app-config.service';
import { UserInfo } from '../models/user-info.model';
import { LicenseRequest } from '../models/license-request';
import {  WeSignLicense } from '../models/wesign-license';
import { ActivateLicense } from '../models/activate-license';

@Injectable({
  providedIn: 'root'
})
export class LicenseService {


  private licenseApi: string;

  constructor(private httpClient: HttpClient,
    private appConfigService: AppConfigService) {
    this.licenseApi = this.appConfigService.apiUrl + "/licenses";
  }
  public generateLicense(userInfo: UserInfo) {
    return this.httpClient.post<LicenseRequest>(`${this.licenseApi}`, userInfo);
  }
  public read() {
    return this.httpClient.get<WeSignLicense>(`${this.licenseApi}`);
  }
  public readSimpleInfo() {
    return this.httpClient.get<WeSignLicense>(`${this.licenseApi}` + "/simpleInfo");
  }

  public update(activateLicense: ActivateLicense) {
    return this.httpClient.put<number>(`${this.licenseApi}`, activateLicense);
  }

}
