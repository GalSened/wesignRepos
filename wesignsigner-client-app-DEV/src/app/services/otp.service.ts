import { Injectable } from '@angular/core';
import { GenerateCodeRequest, GenerateCodeResponse } from '../models/requests/generate-code-request.model';
import { AppConfigService } from './app-config.service';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class OtpService {
  private documentSignerApi: string;

  constructor(private httpClient: HttpClient,
      private appConfigService: AppConfigService) {
      this.documentSignerApi = this.appConfigService.apiUrl + "/otp";
  }

  public isValidCode(token: string, code: string) {
      return this.httpClient.get<GenerateCodeResponse>(`${this.documentSignerApi}?token=${token}&code=${code}`);
  }

  public validatePassword(input: GenerateCodeRequest) {
      return this.httpClient.post<GenerateCodeResponse>(`${this.documentSignerApi}`, input);
  }

  public initOtpDetailsToLocalStorage(){
    localStorage.setItem('otpExpirationInHours', this.appConfigService.otpExpirationInHours);
    localStorage.setItem('shouldSaveOtpLocalStorage', this.appConfigService.shouldSaveOtpLocalStorage);
  }
}