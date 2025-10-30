import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { LogFilter } from '../models/log-filter.model';
import { LogsResult } from '../models/results/logs-result.model';
import { AppConfigService } from './app-config.service';

@Injectable({ providedIn: 'root' })
export class LogsApiService {

    private logsApi :string;

    constructor(private httpClient: HttpClient,
        private appConfigService: AppConfigService) {
            this.logsApi = this.appConfigService.apiUrl + "/logs";
    }

    public Read(logFilter : LogFilter){
        return this.httpClient.get<LogsResult>(`${this.logsApi}?source=${logFilter.applicationSource}&key=${logFilter.key}&` +
            `offset=${logFilter.offset}&logLevel=${logFilter.logLevel}&`
            +`from=${logFilter.from? logFilter.from.toISOString():""}&to=${logFilter.to? logFilter.to.toISOString():""}&`+
            `limit=${logFilter.limit}`, { observe: "response" });
        
    }

}