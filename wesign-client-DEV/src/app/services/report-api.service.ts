import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AppConfigService } from './app-config.service';
import { ReportRequest } from '@models/reports/report-request';
import { UsageDataReports } from '@models/reports/results/documents-report';
import { Observable } from 'rxjs';
import { CreateReportFrequency } from '@models/reports/create-report-frequency';
import { UserPeriodicReport, UserPeriodicReports } from '@models/reports/user-periodic-report';

@Injectable({ providedIn: 'root' })
export class ReportApiService {
    private storage: Storage = sessionStorage || localStorage;
    private readonly JWT_TOKEN: string = 'JWT_TOKEN';
    private reportsApi: string = "";
    constructor(private httpClient: HttpClient, private appConfigService: AppConfigService) {
        this.reportsApi = this.appConfigService.apiUrl + "/reports";
    }

    private get accessToken(): string {
        return this.storage.getItem(this.JWT_TOKEN) || localStorage.getItem(this.JWT_TOKEN);
    }

    public readUsageDataReports(reportRequest: ReportRequest): Observable<UsageDataReports> {
        return this.httpClient.get<UsageDataReports>(`${this.reportsApi}/UsageData?from=${reportRequest.from ? reportRequest.from : ""}&to=${reportRequest.to ? reportRequest.to : ""}&docStatuses=${reportRequest.documentStatuses.join(",")}&groupIds=${reportRequest.groupIds.join(",")}&includeDistributionDocs=${reportRequest.includeDistributionDocs}&offset=${reportRequest.offset}&limit=${reportRequest.limit}`, { headers: { Token: this.accessToken } })
    }

    public readUsageDataReportsCSV(reportRequest: ReportRequest) {
        return this.httpClient.get(`${this.reportsApi}/UsageData?from=${reportRequest.from ? reportRequest.from : ""}&to=${reportRequest.to ? reportRequest.to : ""}&docStatuses=${reportRequest.documentStatuses.join(",")}&groupIds=${reportRequest.groupIds.join(",")}&includeDistributionDocs=${reportRequest.includeDistributionDocs}&offset=${reportRequest.offset}&limit=${reportRequest.limit}&isCSV=${reportRequest.isCsv}`, {
            headers: { Token: this.accessToken },
            observe: "response",
            responseType: "text"
        }).subscribe((data) => {
            this.downloadCSV(data);
        })
    }

    public createFrequencyReports(createFrequencyRequest: CreateReportFrequency): Observable<void> {
        return this.httpClient.post<void>(`${this.reportsApi}/FrequencyReports?frequency=${createFrequencyRequest.frequency ? createFrequencyRequest.frequency : "None"}&reportTypeStr=${createFrequencyRequest.type}`, { headers: { Token: this.accessToken } })
    }

    public readUserPeriodicReports(): Observable<UserPeriodicReports> {
        return this.httpClient.get<UserPeriodicReports>(`${this.reportsApi}/FrequencyReports`, { headers: { Token: this.accessToken } });
    }

    public updateFrequencyReports(createFrequencyRequest: CreateReportFrequency): Observable<void> {
        return this.httpClient.put<void>(`${this.reportsApi}/FrequencyReports?frequency=${createFrequencyRequest.frequency ? createFrequencyRequest.frequency : "None"}&reportTypeStr=${createFrequencyRequest.type}`, { headers: { Token: this.accessToken } })
    }

    public deleteFrequencyReports(): Observable<void> {
        return this.httpClient.delete<void>(`${this.reportsApi}/FrequencyReports`, { headers: { Token: this.accessToken } })
    }

    private downloadCSV(data: any) {
        const filename = "reports_" + Date.now() + ".csv"
        const blob = new Blob(["\ufeff", data.body], { type: "text/csv" });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.target = "_blank";
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
    }
}