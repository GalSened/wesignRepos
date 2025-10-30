import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { QRCodeResult } from '../models/results/qr-code-result.model';
import { QRCodeVerification } from '../models/results/qr-code-verification-result.model';
import { QRCode } from '../models/results/qr-code.model';
import { AppConfigService } from './app-config.service';

@Injectable()
export class OTPApiService {
    private otpApi :string;

   
      
    constructor(private httpClient: HttpClient,
        private appConfigService: AppConfigService) {
        this.otpApi = this.appConfigService.apiUrl + "/otp";
     }


    public ReadQRCode() {
        return this.httpClient.get<QRCodeResult>(`${this.otpApi}`);
    }

    public VerifyOtp(code: string) {
        const httpOptions = {
            headers: new HttpHeaders({'Content-Type': 'application/json'})
          }

          
        return this.httpClient.get<QRCodeVerification>(`${this.otpApi}/verify?code=${code}`, httpOptions)
    }
}