import { Component, Input, OnChanges, OnInit, SimpleChanges, ViewChild, ViewChildren } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { AlertType, BannerAlertComponent } from 'src/app/components/shared/banner-alert/banner-alert.component';
import { PaginatorComponent } from 'src/app/components/shared/paginator.component';
import { ReportFrequency } from 'src/app/enums/report-frequency';
import { ReportMode, ReportPropeties } from 'src/app/enums/ReportMode.enum';
import { Program } from 'src/app/models/program.model';
import { ReportParameters } from 'src/app/models/report-parameters';
import { ReportRequest } from 'src/app/models/ReportRequest';
import { FrequencyReportRequest } from 'src/app/models/requests/frequency-report-request';
import { companyUserReport } from 'src/app/models/results/Reports/companyUserReport';
import { companyReport } from 'src/app/models/results/Reports/companyUtilization-result';
import { freeTrialUserReport } from 'src/app/models/results/Reports/freeTrialUserReport';
import { groupDocumentStatusesReport } from 'src/app/models/results/Reports/groupDocumentStatuses-result';
import { groupUtilizationReport } from 'src/app/models/results/Reports/groupUtilization-result';
import { templatesByUsageReport } from 'src/app/models/results/Reports/templatesByUsageReport';
import { usageByCompanyReport } from 'src/app/models/results/Reports/usageByCompanyReport';
import { usageBySignatureTypeReport as usageBySignatureTypeReport } from 'src/app/models/results/Reports/usageBySignatureFieldsReport';
import { usageByUserReport } from 'src/app/models/results/Reports/usageByUserReport';
import { userDocumentReport } from 'src/app/models/results/Reports/userDocumentReport-result';
import { PagerService } from 'src/app/services/pager.service';
import { ReportsApiService } from 'src/app/services/reports-api.service';

@Component({
  selector: 'app-report-output-table',
  templateUrl: './report-output-table.component.html',
  styleUrls: ['./report-output-table.component.css']
})
export class ReportOutputTableComponent implements OnInit, OnChanges {
  @Input('submittedMode') reportMode: ReportMode;
  @Input('monthsForAvgUse') monthsForAvgUse: number;
  @ViewChild(PaginatorComponent) paginator: PaginatorComponent;
  @ViewChildren('tableHeader') tableHeaders;
  @ViewChildren('cellSpan') cellSpans;
  @ViewChild('bannerAlert', { static: true }) bannerAlert: BannerAlertComponent;
  private readonly TOTAL_COUNT_HEADER: string = 'x-total-count';
  private SUCCESS = 1;
  private FAILED = 2;
  public reportRequest: ReportRequest;
  public headerTexts: string[]
  public ReportModes = Object.values(ReportMode).filter(value => typeof value != 'number');
  private PAGE_SIZE = 10;
  public currentPage = 1;
  public pageCalc: any;
  public isEmptyTable: boolean = true;
  public isSubmit: boolean;
  ReportMode = ReportMode;
  public utilizationReports: companyReport[];
  public groupUtilizationReports: groupUtilizationReport[];
  public programReports: Program[];
  public groupDocStatusesReport: groupDocumentStatusesReport[];
  public docsByUsers: userDocumentReport[];
  public companyUsers: companyUserReport[]
  public freeTrialUsers: freeTrialUserReport[]
  public usageByUsers: usageByUserReport[];
  public usageByCompanies: usageByCompanyReport[];
  public templatesByUsage: templatesByUsageReport[];
  public usageBySignatureTypes: usageBySignatureTypeReport;

  constructor(private reportsApi: ReportsApiService,
    private pager: PagerService) { }

  ngOnInit(): void {
    this.resetPagination();
    this.headerTexts = ReportPropeties.find(x => x.mode.includes((this.reportMode))).headers.map(x => x.replace("_TIME_FOR_AVG", this.monthsForAvgUse.toString()));
  }


  ngOnChanges(changes: SimpleChanges): void {
    this.headerTexts = ReportPropeties.find(x => x.mode.includes((this.reportMode))).headers.map(x => x.replace("_TIME_FOR_AVG", this.monthsForAvgUse.toString()));
    if (changes.monthsForAvgUse || changes.reportMode) {
      this.clearTable();
      this.resetPagination();
    }
  }

  sortTable(headerName) {
  }

  clearTable() {
    this.isEmptyTable = true
    this.utilizationReports = [];
    this.groupDocStatusesReport = [];
    this.programReports = [];
    this.groupDocStatusesReport = [];
    this.docsByUsers = [];
    this.companyUsers = [];
    this.freeTrialUsers = [];
    this.usageByUsers = [];
    this.usageByCompanies = [];
    this.templatesByUsage = [];
    this.usageBySignatureTypes = null;
    this.resetPagination();
  }

  private getOneYearBefore(): Date {
    const now = new Date();
    return new Date(now.getFullYear() - 1, now.getMonth(), now.getDate(), now.getHours(), now.getMinutes(), now.getSeconds());
  }

  public pageChanged(page: number) {
    this.currentPage = page;
    this.callApi(this.reportRequest);
  }

  public checkPercent() {
    this.cellSpans._results.map((x) => x.textContent = x.textContent.replace("-1", "Unlimited"))
  }

  setData(data) {
    const total = +data.headers.get(this.TOTAL_COUNT_HEADER);
    this.pageCalc = this.pager.getPager(total, this.currentPage, this.PAGE_SIZE);
  }
  public callApi(reportRequest: ReportRequest) {
    this.reportRequest = reportRequest;
    if (!this.reportRequest.from) {
      this.reportRequest.from = this.getOneYearBefore();
    }
    if (!this.reportRequest.to) {
      this.reportRequest.to = new Date();
    }
    if (!reportRequest.isCSV) {
      reportRequest.limit = this.PAGE_SIZE;
      reportRequest.offset = (this.currentPage - 1) * this.PAGE_SIZE;
    }
    if (this.reportMode == ReportMode.ExpirationUtilization) {
      if (!reportRequest.isCSV) {
        this.reportsApi.ReadUtilizationByExpiration(reportRequest).pipe(
          tap((data) => {
            this.setData(data);
          }),
          map((res) => res.body.companyReports)
        ).subscribe(res => {
          if (res.length == 0) {
            this.bannerAlert.showBannerAlert("No results found", this.FAILED);
            this.isEmptyTable = true;
          }
          else {
            this.isEmptyTable = false;
          }
          this.utilizationReports = res;
        });
      }
      else {
        this.reportsApi.ReadUtilizationByExpirationCSV(reportRequest)
      }
    }
    else if (this.reportMode == ReportMode.ProgramUtilization) {
      if (reportRequest.programId == null) {
        this.bannerAlert.showBannerAlert("Missing program", this.FAILED);
        return;
      }
      if (!reportRequest.isCSV) {
        this.reportsApi.ReadUtilizationByProgram(reportRequest).pipe(
          tap((data) => {
            this.setData(data);
          }),
          map((res) => res.body.companyReports)
        ).subscribe(res => {
          if (res.length == 0) {
            this.bannerAlert.showBannerAlert("No results found", this.FAILED);
            this.isEmptyTable = true;
          }
          else {
            this.isEmptyTable = false;
          }
          this.utilizationReports = res;
        });
      }
      else {
        this.reportsApi.ReadUtilizationByProgramCSV(reportRequest)
      }
    }
    else if (this.reportMode == ReportMode.UsePercentageUtilization) {
      if (!reportRequest.isCSV) {
        this.reportsApi.ReadUtilizationByUsePercentage(reportRequest).pipe(
          tap((data) => {
            const total = +data.headers.get(this.TOTAL_COUNT_HEADER);
            this.pageCalc = this.pager.getPager(total, this.currentPage, this.PAGE_SIZE);
          }),
          map((res) => res.body.companyReports)
        ).subscribe(res => {
          if (res.length == 0) {
            this.bannerAlert.showBannerAlert("No results found", this.FAILED);
            this.isEmptyTable = true;
          }
          else {
            this.isEmptyTable = false;
          }
          this.utilizationReports = res;
        });
      }
      else {
        this.reportsApi.ReadUtilizationByUsePercentageCSV(reportRequest)
      }
    }
    else if (this.reportMode == ReportMode.AllCompaniesUtilization) {
      if (!reportRequest.isCSV) {
        this.reportsApi.ReadAllCompaniesUtiliziations(reportRequest).pipe(
          tap((data) => {
            const total = +data.headers.get(this.TOTAL_COUNT_HEADER);
            this.pageCalc = this.pager.getPager(total, this.currentPage, this.PAGE_SIZE);
          }),
          map((res) => res.body.companyReports)
        ).subscribe(res => {
          if (res.length == 0) {
            this.bannerAlert.showBannerAlert("No results found", this.FAILED);
            this.isEmptyTable = true;
          }
          else {
            this.isEmptyTable = false;
          }
          this.utilizationReports = res;
        });
      }
      else {
        this.reportsApi.ReadAllCompaniesUtiliziationsCSV(reportRequest)
      }
    }
    else if (this.reportMode == ReportMode.GroupUtilization) {
      if (reportRequest.companyId == null) {
        this.bannerAlert.showBannerAlert("Missing company", this.FAILED);
        return;
      }
      if (!reportRequest.isCSV) {
        this.reportsApi.ReadGroupUtilization(reportRequest).pipe(
          tap((data) => {
            const total = +data.headers.get(this.TOTAL_COUNT_HEADER);
            this.pageCalc = this.pager.getPager(total, this.currentPage, this.PAGE_SIZE);
          }),
          map((res) => res.body.groupReports)
        ).subscribe(res => {
          if (res.length == 0) {
            this.bannerAlert.showBannerAlert("No results found", this.FAILED);
            this.isEmptyTable = true;
          }
          else {
            this.isEmptyTable = false;
          }
          this.groupUtilizationReports = res;
        });
      }
      else {
        this.reportsApi.ReadGroupUtilizationCSV(reportRequest)
      }
    }
    else if (this.reportMode == ReportMode.ProgramByUtilization) {
      if (!reportRequest.isCSV) {
        this.reportsApi.ReadProgramsReport(reportRequest).pipe(
          tap((data) => {
            const total = +data.headers.get(this.TOTAL_COUNT_HEADER);
            this.pageCalc = this.pager.getPager(total, this.currentPage, this.PAGE_SIZE);
          }),
          map((res) => res.body.programs)
        ).subscribe(res => {
          if (res.length == 0) {
            this.bannerAlert.showBannerAlert("No results found", this.FAILED);
            this.isEmptyTable = true;
          }
          else {
            this.isEmptyTable = false;
          }
          this.programReports = res;
        });
      }
      else {
        this.reportsApi.ReadProgramsReportCSV(reportRequest)
      }
    }
    else if (this.reportMode == ReportMode.ProgramsByUsage) {
      if (!reportRequest.isCSV) {
        this.reportsApi.ReadProgramsByUsageReport(reportRequest).pipe(
          tap((data) => {
            const total = +data.headers.get(this.TOTAL_COUNT_HEADER);
            this.pageCalc = this.pager.getPager(total, this.currentPage, this.PAGE_SIZE);
          }),
          map((res) => res.body.programs)
        ).subscribe(res => {
          if (res.length == 0) {
            this.bannerAlert.showBannerAlert("No results found", this.FAILED);
            this.isEmptyTable = true;
          }
          else {
            this.isEmptyTable = false;
          }
          this.programReports = res;
        });
      }
      else {
        this.reportsApi.ReadProgramsByUsageReportCSV(reportRequest)
      }
    }
    else if (this.reportMode == ReportMode.GroupDocumentStatuses) {
      if (reportRequest.companyId == null) {
        this.bannerAlert.showBannerAlert("Missing company", this.FAILED);
        return;
      }
      if (!reportRequest.isCSV) {
        this.reportsApi.ReadGroupDocStatusesReport(reportRequest).pipe(
          tap((data) => {
            const total = +data.headers.get(this.TOTAL_COUNT_HEADER);
            this.pageCalc = this.pager.getPager(total, this.currentPage, this.PAGE_SIZE);
          }),
          map((res) => res.body.groupDocumentReports)
        ).subscribe(res => {
          if (res.length == 0) {
            this.bannerAlert.showBannerAlert("No results found", this.FAILED);
            this.isEmptyTable = true;
          }
          else {
            this.isEmptyTable = false;
          }
          this.groupDocStatusesReport = res;
        });
      }
      else {
        this.reportsApi.ReadGroupDocStatusesReportCSV(reportRequest)
      }
    }
    else if (this.reportMode == ReportMode.DocsByUsers) {
      if (reportRequest.companyId == null) {
        this.bannerAlert.showBannerAlert("Missing company", this.FAILED);
        return;
      }
      if (!reportRequest.isCSV) {
        this.reportsApi.ReadDocsByUsers(reportRequest).pipe(
          tap((data) => {
            const total = +data.headers.get(this.TOTAL_COUNT_HEADER);
            this.pageCalc = this.pager.getPager(total, this.currentPage, this.PAGE_SIZE);
          }),
          map((res) => res.body.userDocumentReports)
        ).subscribe(res => {
          if (res.length == 0) {
            this.bannerAlert.showBannerAlert("No results found", this.FAILED);
            this.isEmptyTable = true;
          }
          else {
            this.isEmptyTable = false;
          }
          this.docsByUsers = res;
        });
      }
      else {
        this.reportsApi.ReadDocsByUsersCSV(reportRequest)
      }
    }
    else if (this.reportMode == ReportMode.DocsBySigners) {
      if (reportRequest.companyId == null) {
        this.bannerAlert.showBannerAlert("Missing company", this.FAILED);
        return;
      }
      if (!reportRequest.isCSV) {
        this.reportsApi.ReadDocsBySigners(reportRequest).pipe(
          tap((data) => {
            const total = +data.headers.get(this.TOTAL_COUNT_HEADER);
            this.pageCalc = this.pager.getPager(total, this.currentPage, this.PAGE_SIZE);
          }),
          map((res) => res.body.userDocumentReports)
        ).subscribe(res => {
          if (res.length == 0) {
            this.bannerAlert.showBannerAlert("No results found", this.FAILED);
            this.isEmptyTable = true;
          }
          else {
            this.isEmptyTable = false;
          }
          this.docsByUsers = res;
        });
      }
      else {
        this.reportsApi.ReadDocsBySignersCSV(reportRequest)
      }
    }
    else if (this.reportMode == ReportMode.CompanyUsers) {
      if (reportRequest.companyId == null) {
        this.bannerAlert.showBannerAlert("Missing company", this.FAILED);
        return;
      }
      if (!reportRequest.isCSV) {
        this.reportsApi.ReadCompanyUsers(reportRequest).pipe(
          tap((data) => {
            const total = +data.headers.get(this.TOTAL_COUNT_HEADER);
            this.pageCalc = this.pager.getPager(total, this.currentPage, this.PAGE_SIZE);
          }),
          map((res) => res.body.companyUsersReports)
        ).subscribe(res => {
          if (res.length == 0) {
            this.bannerAlert.showBannerAlert("No results found", this.FAILED);
            this.isEmptyTable = true;
          }
          else {
            this.isEmptyTable = false;
          }
          this.companyUsers = res;
        });
      }
      else {
        this.reportsApi.ReadCompanyUsersCSV(reportRequest)
      }
    }
    else if (this.reportMode == ReportMode.FreeTrialUsers) {
      if (!reportRequest.isCSV) {
        this.reportsApi.ReadFreeTrialUsers(reportRequest).pipe(
          tap((data) => {
            const total = +data.headers.get(this.TOTAL_COUNT_HEADER);
            this.pageCalc = this.pager.getPager(total, this.currentPage, this.PAGE_SIZE);
          }),
          map((res) => res.body.freeTrialUsersReports)
        ).subscribe(res => {
          if (res.length == 0) {
            this.bannerAlert.showBannerAlert("No results found", this.FAILED);
            this.isEmptyTable = true;
          }
          else {
            this.isEmptyTable = false;
          }
          this.freeTrialUsers = res;
        });
      }
      else {
        this.reportsApi.ReadFreeTrialUsersCSV(reportRequest)
      }
    }

    else if (this.reportMode == ReportMode.UsageByUsers) {
      if (reportRequest.userEmail == null && reportRequest.companyId == null) {
        this.bannerAlert.showBannerAlert("Missing email or company", this.FAILED);
        return;
      }
      if (!reportRequest.isCSV) {
        this.reportsApi.ReadUsageByUsers(reportRequest).pipe(
          tap((data) => {
            const total = +data.headers.get(this.TOTAL_COUNT_HEADER);
            this.pageCalc = this.pager.getPager(total, this.currentPage, this.PAGE_SIZE);
          }),
          map((res) => res.body.usageByUsersReports)
        ).subscribe(res => {
          if (res.length == 0) {
            this.bannerAlert.showBannerAlert("No results found", this.FAILED);
            this.isEmptyTable = true;
          }
          else {
            this.isEmptyTable = false;
          }
          this.usageByUsers = res;
        });
      }
      else {
        this.reportsApi.ReadUsageByUsersCSV(reportRequest);
      }
    }
    else if (this.reportMode == ReportMode.UsageByCompanies) {
      if (this.reportRequest.companyId == null) {
        this.bannerAlert.showBannerAlert("Missing company", this.FAILED);
        return;
      }
      if (!reportRequest.isCSV) {
        this.reportsApi.ReadUsageByCompanies(reportRequest).pipe(
          tap((data) => {
            const total = +data.headers.get(this.TOTAL_COUNT_HEADER);
            this.pageCalc = this.pager.getPager(total, this.currentPage, this.PAGE_SIZE);
          }),
          map((res) => res.body.usageByCompaniesReports)
        ).subscribe(res => {
          if (res.length == 0) {
            this.bannerAlert.showBannerAlert("No results found", this.FAILED);
            this.isEmptyTable = true;
          }
          else {
            this.isEmptyTable = false;
          }
          this.usageByCompanies = res;
        });
      }
      else {
        this.reportsApi.ReadUsageByCompaniesCSV(reportRequest);
      }
    }
    else if (this.reportMode == ReportMode.TemplatesByUsage) {
      if (this.reportRequest.companyId == null) {
        this.bannerAlert.showBannerAlert("Missing company", this.FAILED);
        return;
      }
      if (!reportRequest.isCSV) {
        this.reportsApi.ReadTemplatesByUsage(reportRequest).pipe(
          tap((data) => {
            const total = +data.headers.get(this.TOTAL_COUNT_HEADER);
            this.pageCalc = this.pager.getPager(total, this.currentPage, this.PAGE_SIZE);
          }),
          map((res) => res.body.templatesByUsageReports)
        ).subscribe(res => {
          if (res.length == 0) {
            this.bannerAlert.showBannerAlert("No results found", this.FAILED);
            this.isEmptyTable = true;
          }
          else {
            this.isEmptyTable = false;
          }
          this.templatesByUsage = res;
        });
      }
      else {
        this.reportsApi.ReadTemplatesByUsageCSV(reportRequest);
      }
    }
    else if (this.reportMode == ReportMode.UsageBySignatureType) {
      if (this.reportRequest.companyId == null) {
        this.bannerAlert.showBannerAlert("Missing company", this.FAILED);
        return;
      }
      if (!reportRequest.isCSV) {
        this.reportsApi.ReadUsageBySignatureType(reportRequest).pipe(
          tap((data) => {
            const total = data.body ? 1 : 0;
            this.pageCalc = this.pager.getPager(total, this.currentPage, this.PAGE_SIZE);
          }),
          map((res) => res.body)
        ).subscribe(res => {
          if (res == null) {
            this.bannerAlert.showBannerAlert("No results found", this.FAILED);
            this.isEmptyTable = true;
          }
          else {
            this.isEmptyTable = false;
          }
          this.usageBySignatureTypes = res;
        });
      }
      else {
        this.reportsApi.ReadUsageBySignatureTypeCSV(reportRequest);
      }
    }
  }

  private resetPagination() {
    this.pageCalc = this.pager.getPager(this.PAGE_SIZE, 1, this.PAGE_SIZE);
    this.currentPage = 1;
    this.paginator?.reset();
  }
}
