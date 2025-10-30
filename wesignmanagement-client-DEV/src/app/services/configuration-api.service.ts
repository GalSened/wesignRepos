
import { HttpClient } from "@angular/common/http";
import { Injectable } from '@angular/core';
import { Configuration } from '../models/results/configuration.model';
import { AppConfigService } from './app-config.service';
import { InitConfiguration } from '../models/results/init-configuration.model';
import { SmsDetails } from '../models/sms-configuration.model';
import { SmtpDetails } from "../models/smtp-configuration.model";

@Injectable({ providedIn: 'root' })
export class ConfigurationApiService {

    private configurationApi :string;

    constructor(private httpClient: HttpClient,
        private appConfigService: AppConfigService) {
            this.configurationApi =  this.appConfigService.apiUrl + "/configuration";
        }

    public Read(){
        return this.httpClient.get<Configuration>(`${this.configurationApi}`, { observe: "response" });
    }

    public ReadInitConfiguration(){
        return this.httpClient.get<InitConfiguration>(`${this.configurationApi}/init`, { observe: "response" });
    }

    public Update(configuration: Configuration) {
        return this.httpClient.put(`${this.configurationApi}`, configuration);
    }

    public SendSmsTestMessage(smsDetails : SmsDetails){
        return this.httpClient.post(`${this.configurationApi}/sms/message`, smsDetails);
    }

    public SendSmtpTestMessage(smtpDetails : SmtpDetails){
        return this.httpClient.post(`${this.configurationApi}/smtp/message`, smtpDetails);
    }
}