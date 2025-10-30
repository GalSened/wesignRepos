import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { LogFilter } from '../models/log-filter.model';
import { companyReports } from '../models/results/Reports/companyUtilization-result';
import { ReportsResult } from '../models/results/reports-result.model';
import { ReportRequest } from '../models/ReportRequest';
import { AppConfigService } from './app-config.service';
import { groupUtilizationReports } from '../models/results/Reports/groupUtilization-result';
import { ProgramsResult } from '../models/results/programs-result.model';
import { groupDocumentStatusesReports } from '../models/results/Reports/groupDocumentStatuses-result';
import { userDocumentReports } from '../models/results/Reports/userDocumentReport-result';
import { GroupsResult } from '../models/results/groups-result.model';
import { companyUserReports } from '../models/results/Reports/companyUserReport';
import { freeTrialUserReports } from '../models/results/Reports/freeTrialUserReport';
import { usageByUserReports } from '../models/results/Reports/usageByUserReport';
import { usageByCompanyReports } from '../models/results/Reports/usageByCompanyReport';
import { templatesByUsageReports } from '../models/results/Reports/templatesByUsageReport';
import { usageBySignatureTypeReport } from '../models/results/Reports/usageBySignatureFieldsReport';
import { FrequencyReportRequest } from '../models/requests/frequency-report-request';
import { ManagementPeriodicReport, ManagementPeriodicReports } from '../models/results/Reports/managementPeriodicReports-result';

@Injectable({
  providedIn: 'root'
})
export class ReportsApiService {

  private reportsApi: string;
  private emptyGuid: string = '00000000-0000-0000-0000-000000000000';

  constructor(private httpClient: HttpClient,
    private appConfigService: AppConfigService) {
    this.reportsApi = this.appConfigService.apiUrl + "/reports";
  }

  public Read(filter: LogFilter) {
    return this.httpClient.get<ReportsResult>(`${this.reportsApi}?key=${filter.key}&offset=${filter.offset}&limit=${filter.limit}&from=${filter.from ? filter.from.toISOString() : ""}&to=${filter.to ? filter.to.toISOString() : ""}`, { observe: "response" });
  }

  public ReadUtilizationByExpiration(reportRequest: ReportRequest) {
    return reportRequest.isExpired == null ? this.httpClient.get<companyReports>(`${this.reportsApi}/UtilizationReport/Expired?monthsForAvgUse=${reportRequest.monthsForAvgUse}&programId=${reportRequest.programId ? reportRequest.programId : ""}&from=${reportRequest.from ? reportRequest.from.toISOString() : ""}&to=${reportRequest.to ? reportRequest.to.toISOString() : ""}&offset=${reportRequest.offset}&limit=${reportRequest.limit}&isCSV=${reportRequest.isCSV}`, { observe: "response" })
      : this.httpClient.get<companyReports>(`${this.reportsApi}/UtilizationReport/Expired?isExpired=${reportRequest.isExpired}&monthsForAvgUse=${reportRequest.monthsForAvgUse}&programId=${reportRequest.programId ? reportRequest.programId : ""}&from=${reportRequest.from ? reportRequest.from.toISOString() : ""}&to=${reportRequest.to ? reportRequest.to.toISOString() : ""}&offset=${reportRequest.offset}&limit=${reportRequest.limit}&isCSV=${reportRequest.isCSV}`, { observe: "response" });
  }


  public ReadUtilizationByProgram(reportRequest: ReportRequest) {
    return this.httpClient.get<companyReports>(`${this.reportsApi}/UtilizationReport/Program/${reportRequest.programId}?monthsForAvgUse=${reportRequest.monthsForAvgUse}&minDocs=${reportRequest.minDocs}&minSMS=${reportRequest.minSMS}&offset=${reportRequest.offset}&limit=${reportRequest.limit}`, { observe: "response" });
  }

  public ReadUtilizationByUsePercentage(reportRequest: ReportRequest) {
    return this.httpClient.get<companyReports>(`${this.reportsApi}/UtilizationReport/Percentage?docsUsagePercentage=${reportRequest.docUsagePercentage}&monthsForAvgUse=${reportRequest.monthsForAvgUse}&offset=${reportRequest.offset}&limit=${reportRequest.limit}`, { observe: "response" });
  }

  public ReadAllCompaniesUtiliziations(reportRequest: ReportRequest) {
    return this.httpClient.get<companyReports>(`${this.reportsApi}/UtilizationReport/AllCompanies?monthsForAvgUse=${reportRequest.monthsForAvgUse}&offset=${reportRequest.offset}&limit=${reportRequest.limit}`, { observe: "response" });
  }

  public ReadGroupUtilization(reportRequest: ReportRequest) {
    return this.httpClient.get<groupUtilizationReports>(`${this.reportsApi}/UtilizationReport/GroupUtilization/${reportRequest.companyId}?docsUsagePercentage=${reportRequest.docUsagePercentage}&monthsForAvgUse=${reportRequest.monthsForAvgUse}&offset=${reportRequest.offset}&limit=${reportRequest.limit}`, { observe: "response" });
  }

  public ReadProgramsReport(reportRequest: ReportRequest) {
    return this.httpClient.get<ProgramsResult>(`${this.reportsApi}/Programs?minDocs=${reportRequest.minDocs}&minSMS=${reportRequest.minSMS}&offset=${reportRequest.offset}&limit=${reportRequest.limit}`, { observe: "response" });
  }

  public ReadProgramsByUsageReport(reportRequest: ReportRequest) {
    return reportRequest.isProgramUsed != null ? this.httpClient.get<ProgramsResult>(`${this.reportsApi}/UnusedPrograms?isUsed=${reportRequest.isProgramUsed}&offset=${reportRequest.offset}&limit=${reportRequest.limit}`, { observe: "response" }) : this.httpClient.get<ProgramsResult>(`${this.reportsApi}/UnusedPrograms?offset=${reportRequest.offset}&limit=${reportRequest.limit}`, { observe: "response" });
  }

  public ReadGroupDocStatusesReport(reportRequest: ReportRequest) {
    return this.httpClient.get<groupDocumentStatusesReports>(`${this.reportsApi}/GroupDocumentReports/${reportRequest.companyId}?offset=${reportRequest.offset}&limit=${reportRequest.limit}`, { observe: "response" });
  }

  public ReadDocsByUsers(reportRequest: ReportRequest) {
    return this.httpClient.get<userDocumentReports>(`${this.reportsApi}/DocsByUsers/${reportRequest.companyId}?groupIds=${reportRequest.groupIds.join(",")}&from=${reportRequest.from ? reportRequest.from.toISOString() : ""}&to=${reportRequest.to ? reportRequest.to.toISOString() : ""}&offset=${reportRequest.offset}&limit=${reportRequest.limit}`, { observe: "response" });
  }

  public ReadDocsBySigners(reportRequest: ReportRequest) {
    return this.httpClient.get<userDocumentReports>(`${this.reportsApi}/DocsBySigners/${reportRequest.companyId}?groupIds=${reportRequest.groupIds.join(",")}&from=${reportRequest.from ? reportRequest.from.toISOString() : ""}&to=${reportRequest.to ? reportRequest.to.toISOString() : ""}&offset=${reportRequest.offset}&limit=${reportRequest.limit}`, { observe: "response" });
  }

  public ReadCompanyUsers(reportRequest: ReportRequest) {
    return this.httpClient.get<companyUserReports>(`${this.reportsApi}/UsersByCompany/${reportRequest.companyId}?offset=${reportRequest.offset}&limit=${reportRequest.limit}`, { observe: "response" });
  }

  public ReadFreeTrialUsers(reportRequest: ReportRequest) {
    return this.httpClient.get<freeTrialUserReports>(`${this.reportsApi}/FreeTrialUsers?offset=${reportRequest.offset}&limit=${reportRequest.limit}`, { observe: "response" });
  }

  public ReadUsageByUsers(reportRequest: ReportRequest) {
    return this.httpClient.get<usageByUserReports>(`${this.reportsApi}/UsageByUsers?companyId=${reportRequest.companyId || this.emptyGuid}&userEmail=${reportRequest.userEmail ? reportRequest.userEmail : ''}&groupIds=${reportRequest.groupIds.join(",")}&offset=${reportRequest.offset}&limit=${reportRequest.limit}&from=${reportRequest.from ? reportRequest.from.toISOString() : ""}&to=${reportRequest.to ? reportRequest.to.toISOString() : ""}`, { observe: "response" });
  }

  public ReadUsageByCompanies(reportRequest: ReportRequest) {
    return this.httpClient.get<usageByCompanyReports>(`${this.reportsApi}/UsageByCompanies/${reportRequest.companyId}?groupIds=${reportRequest.groupIds.join(",")}&offset=${reportRequest.offset}&limit=${reportRequest.limit}&from=${reportRequest.from ? reportRequest.from.toISOString() : ""}&to=${reportRequest.to ? reportRequest.to.toISOString() : ""}`, { observe: "response" });
  }

  public ReadTemplatesByUsage(reportRequest: ReportRequest) {
    return this.httpClient.get<templatesByUsageReports>(`${this.reportsApi}/TemplatesByUsage/${reportRequest.companyId}?groupIds=${reportRequest.groupIds.join(",")}&offset=${reportRequest.offset}&limit=${reportRequest.limit}&from=${reportRequest.from ? reportRequest.from.toISOString() : ""}&to=${reportRequest.to ? reportRequest.to.toISOString() : ""}`, { observe: "response" });
  }

  public ReadUsageBySignatureType(reportRequest: ReportRequest) {
    return this.httpClient.get<usageBySignatureTypeReport>(`${this.reportsApi}/UsageBySignatureType/${reportRequest.companyId}?signatureTypes=${reportRequest.signatureTypes.join(",")}&from=${reportRequest.from ? reportRequest.from.toISOString() : ""}&to=${reportRequest.to ? reportRequest.to.toISOString() : ""}`, { observe: "response" });
  }

  public ReadCompanyGroups(companyId: string) {
    return this.httpClient.get<GroupsResult>(`${this.reportsApi}/CompanyGroups/${companyId}`, { observe: "response" });
  }

  public ReadUtilizationByExpirationCSV(reportRequest: ReportRequest) {
    return reportRequest.isExpired != null ?
      this.httpClient.get(`${this.reportsApi}/UtilizationReport/Expired?isExpired=${reportRequest.isExpired}&monthsForAvgUse=${reportRequest.monthsForAvgUse}&from=${reportRequest.from ? reportRequest.from.toISOString() : ""}&to=${reportRequest.to ? reportRequest.to.toISOString() : ""}&offset=${reportRequest.offset}&limit=${reportRequest.limit}&isCSV=${reportRequest.isCSV}`, {
        observe: "response",
        responseType: "text",
      }).subscribe((data) => {
        this.downloadCSV(data);
      }) :
      this.httpClient.get(`${this.reportsApi}/UtilizationReport/Expired?monthsForAvgUse=${reportRequest.monthsForAvgUse}&from=${reportRequest.from ? reportRequest.from.toISOString() : ""}&to=${reportRequest.to ? reportRequest.to.toISOString() : ""}&offset=${reportRequest.offset}&limit=${reportRequest.limit}&isCSV=${reportRequest.isCSV}`, {
        observe: "response",
        responseType: "text",
      }).subscribe((data) => {
        this.downloadCSV(data);
      })
  }

  public ReadUtilizationByProgramCSV(reportRequest: ReportRequest) {
    return this.httpClient.get(`${this.reportsApi}/UtilizationReport/Program/${reportRequest.programId}?monthsForAvgUse=${reportRequest.monthsForAvgUse}&minDocs=${reportRequest.minDocs}&minSMS=${reportRequest.minSMS}&offset=${reportRequest.offset}&limit=${reportRequest.limit}&isCSV=${reportRequest.isCSV}`, {
      observe: "response",
      responseType: "text",
    }).subscribe((data) => {
      this.downloadCSV(data);
    });
  }

  public ReadUtilizationByUsePercentageCSV(reportRequest: ReportRequest) {
    return this.httpClient.get(`${this.reportsApi}/UtilizationReport/Percentage?docsUsagePercentage=${reportRequest.docUsagePercentage}&monthsForAvgUse=${reportRequest.monthsForAvgUse}&offset=${reportRequest.offset}&limit=${reportRequest.limit}&isCSV=${reportRequest.isCSV}`, {
      observe: "response",
      responseType: "text",
    }).subscribe((data) => {
      this.downloadCSV(data);
    });
  }

  public ReadAllCompaniesUtiliziationsCSV(reportRequest: ReportRequest) {
    return this.httpClient.get(`${this.reportsApi}/UtilizationReport/AllCompanies?monthsForAvgUse=${reportRequest.monthsForAvgUse}&offset=${reportRequest.offset}&limit=${reportRequest.limit}&isCSV=${reportRequest.isCSV}`, {
      observe: "response",
      responseType: "text",
    }).subscribe((data) => {
      this.downloadCSV(data);
    });
  }

  public ReadGroupUtilizationCSV(reportRequest: ReportRequest) {
    return this.httpClient.get(`${this.reportsApi}/UtilizationReport/GroupUtilization/${reportRequest.companyId}?docsUsagePercentage=${reportRequest.docUsagePercentage}&monthsForAvgUse=${reportRequest.monthsForAvgUse}&offset=${reportRequest.offset}&limit=${reportRequest.limit}&isCSV=${reportRequest.isCSV}`, {
      observe: "response",
      responseType: "text",
    }).subscribe((data) => {
      this.downloadCSV(data);
    });
  }

  public ReadProgramsReportCSV(reportRequest: ReportRequest) {
    return this.httpClient.get(`${this.reportsApi}/Programs?minDocs=${reportRequest.minDocs}&minSMS=${reportRequest.minSMS}&offset=${reportRequest.offset}&limit=${reportRequest.limit}&isCSV=${reportRequest.isCSV}`, {
      observe: "response",
      responseType: "text",
    }).subscribe((data) => {
      this.downloadCSV(data);
    });
  }

  public ReadProgramsByUsageReportCSV(reportRequest: ReportRequest) {
    return reportRequest.isProgramUsed != null ?
      this.httpClient.get(`${this.reportsApi}/UnusedPrograms?isUsed=${reportRequest.isProgramUsed}&offset=${reportRequest.offset}&limit=${reportRequest.limit}&isCSV=${reportRequest.isCSV}`, {
        observe: "response",
        responseType: "text",
      }).subscribe((data) => {
        this.downloadCSV(data);
      }) :
      this.httpClient.get(`${this.reportsApi}/UnusedPrograms?offset=${reportRequest.offset}&limit=${reportRequest.limit}&isCSV=${reportRequest.isCSV}`, {
        observe: "response",
        responseType: "text",
      }).subscribe((data) => {
        this.downloadCSV(data);
      });
  }

  public ReadGroupDocStatusesReportCSV(reportRequest: ReportRequest) {
    return this.httpClient.get(`${this.reportsApi}/GroupDocumentReports/${reportRequest.companyId}?offset=${reportRequest.offset}&limit=${reportRequest.limit}&isCSV=${reportRequest.isCSV}`, {
      observe: "response",
      responseType: "text",
    }).subscribe((data) => {
      this.downloadCSV(data);
    });
  }

  public ReadDocsByUsersCSV(reportRequest: ReportRequest) {
    return this.httpClient.get(`${this.reportsApi}/DocsByUsers/${reportRequest.companyId}?groupIds=${reportRequest.groupIds.join(",")}&from=${reportRequest.from ? reportRequest.from.toISOString() : ""}&to=${reportRequest.to ? reportRequest.to.toISOString() : ""}&offset=${reportRequest.offset}&limit=${reportRequest.limit}&isCSV=${reportRequest.isCSV}`, {
      observe: "response",
      responseType: "text",
    }).subscribe((data) => {
      this.downloadCSV(data);
    });
  }

  public ReadDocsBySignersCSV(reportRequest: ReportRequest) {
    return this.httpClient.get(`${this.reportsApi}/DocsBySigners/${reportRequest.companyId}?groupIds=${reportRequest.groupIds.join(",")}&from=${reportRequest.from ? reportRequest.from.toISOString() : ""}&to=${reportRequest.to ? reportRequest.to.toISOString() : ""}&offset=${reportRequest.offset}&limit=${reportRequest.limit}&isCSV=${reportRequest.isCSV}`, {
      observe: "response",
      responseType: "text",
    }).subscribe((data) => {
      this.downloadCSV(data);
    });
  }

  public ReadCompanyUsersCSV(reportRequest: ReportRequest) {
    return this.httpClient.get(`${this.reportsApi}/UsersByCompany/${reportRequest.companyId}?offset=${reportRequest.offset}&limit=${reportRequest.limit}&isCSV=${reportRequest.isCSV}`, {
      observe: "response",
      responseType: "text",
    }).subscribe((data) => {
      this.downloadCSV(data);
    });
  }


  public ReadFreeTrialUsersCSV(reportRequest: ReportRequest) {
    return this.httpClient.get(`${this.reportsApi}/FreeTrialUsers?offset=${reportRequest.offset}&limit=${reportRequest.limit}&isCSV=${reportRequest.isCSV}`, {
      observe: "response",
      responseType: "text",
    }).subscribe((data) => {
      this.downloadCSV(data);
    });
  }

  public ReadUsageByUsersCSV(reportRequest: ReportRequest) {
    return this.httpClient.get(`${this.reportsApi}/UsageByUsers?companyId=${reportRequest.companyId || this.emptyGuid}&userEmail=${reportRequest.userEmail ? reportRequest.userEmail : ''}&groupIds=${reportRequest.groupIds.join(",")}&offset=${reportRequest.offset}&limit=${reportRequest.limit}&from=${reportRequest.from ? reportRequest.from.toISOString() : ""}&to=${reportRequest.to ? reportRequest.to.toISOString() : ""}&isCsv=${reportRequest.isCSV}`, {
      observe: "response",
      responseType: "text"
    }).subscribe((data) => {
      this.downloadCSV(data);
    });
  }

  public ReadUsageByCompaniesCSV(reportRequest: ReportRequest) {
    return this.httpClient.get(`${this.reportsApi}/UsageByCompanies/${reportRequest.companyId}?groupIds=${reportRequest.groupIds.join(",")}&offset=${reportRequest.offset}&limit=${reportRequest.limit}&from=${reportRequest.from ? reportRequest.from.toISOString() : ""}&to=${reportRequest.to ? reportRequest.to.toISOString() : ""}&isCsv=${reportRequest.isCSV}`, {
      observe: "response",
      responseType: "text"
    }).subscribe((data) => {
      this.downloadCSV(data);
    });
  }

  public ReadTemplatesByUsageCSV(reportRequest: ReportRequest) {
    return this.httpClient.get(`${this.reportsApi}/TemplatesByUsage/${reportRequest.companyId}?groupIds=${reportRequest.groupIds.join(",")}&offset=${reportRequest.offset}&limit=${reportRequest.limit}&from=${reportRequest.from ? reportRequest.from.toISOString() : ""}&to=${reportRequest.to ? reportRequest.to.toISOString() : ""}&isCsv=${reportRequest.isCSV}`, {
      observe: "response",
      responseType: "text"
    }).subscribe((data) => {
      this.downloadCSV(data);
    });
  }

  public ReadUsageBySignatureTypeCSV(reportRequest: ReportRequest) {
    return this.httpClient.get(`${this.reportsApi}/UsageBySignatureType/${reportRequest.companyId}?signatureTypes=${reportRequest.signatureTypes.join(",")}&offset=${reportRequest.offset}&limit=${reportRequest.limit}&from=${reportRequest.from ? reportRequest.from.toISOString() : ""}&to=${reportRequest.to ? reportRequest.to.toISOString() : ""}&isCsv=${reportRequest.isCSV}`, {
      observe: "response",
      responseType: "text"
    }).subscribe((data) => {
      this.downloadCSV(data);
    });
  }

  public CreateFrequencyReport(frequencyRequest: FrequencyReportRequest) {
    return this.httpClient.post(`${this.reportsApi}/FrequencyReport`, frequencyRequest, { observe: "response" });
  }

  public ReadFrequencyReports() {
    return this.httpClient.get<ManagementPeriodicReports>(`${this.reportsApi}/FrequencyReports`, { observe: "response" })
  }

  public UpdateFrequencyReport(frequencyReport: ManagementPeriodicReport) {
    return this.httpClient.put(`${this.reportsApi}/FrequencyReports`, frequencyReport, { observe: "response" });
  }

  public DeleteFrequencyReport(frequencyReportId: string) {
    return this.httpClient.delete(`${this.reportsApi}/FrequencyReports/${frequencyReportId}`);
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