import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AppConfigService } from './app-config.service';
import { InitConfiguration } from '@models/configuration/init-configuration.model';
import { TabletsConfiguration } from '@models/configuration/tablets-configuration.model';



@Injectable({ providedIn: 'root' })
export class ConfigurationApiService{

    private configurationApi : string = "";
    
    constructor(private httpClient: HttpClient,
        private appConfigService: AppConfigService) {
        this.configurationApi = this.appConfigService.apiUrl + "/configuration";
    }

    public readInitConfiguration() {
        return this.httpClient.get<InitConfiguration>(`${this.configurationApi}`);
    }

    public readTablets(key:string){
        return this.httpClient.get<any>(`${this.configurationApi}/tablets?key=${key}`);
    }
}