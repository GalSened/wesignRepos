import { Component, EventEmitter, OnInit, Output, ViewChild, ViewContainerRef } from '@angular/core';
import { DocStatus } from '@models/enums/doc-status.enum';
import { ReportFrequency } from '@models/enums/report-frequency';
import { group } from '@models/managment/groups/management-groups.model';
import { CreateReportFrequency as CreateFrequencyReport } from '@models/reports/create-report-frequency';
import { ReportRequest } from '@models/reports/report-request';
import { ReportType } from '@models/reports/report-type.enum';
import { TranslateService } from '@ngx-translate/core';
import { AutoReportsNotificationService } from '@services/auto-reports-notification.service';
import { SharedService } from '@services/shared.service';
import { UserApiService } from '@services/user-api.service';
import { AlertLevel } from '@state/app-state.interface';
import { forkJoin, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
  selector: 'sgn-report-filter',
  templateUrl: './report-filter.component.html',
  styles: [
  ]
})
export class ReportFilterComponent implements OnInit {
  @Output() public SubmittedRequest = new EventEmitter<{ type: ReportType, request: ReportRequest }>();
  @Output() public SubmittedFrequencyRequest = new EventEmitter<CreateFrequencyReport>();
  @Output() public DeletedFrequencyRequest = new EventEmitter<void>();
  public reportType: ReportType;
  public optionalReportTypes: any[];
  public docStatuses: any[];
  public optionalDocStatusesTypes: DocStatus[] = [0, 2, 4, 5, 8];
  public previousSelectedDocStatuses = [];
  public reportFrequencies: any[];
  public groups: group[];
  public reportRequest: ReportRequest;
  public isFromCalendarShown = false;
  public isToCalendarShown = false;
  public allSelected = false;
  public showFrequencyReportCreation = false;
  public distributionOptions = [];

  constructor(private translate: TranslateService,
    private userApiService: UserApiService,
    private sharedService: SharedService,
    private autoReportsNotificationService: AutoReportsNotificationService) {
    this.reportRequest = new ReportRequest();
    this.updateData();
    
    this.translate.onLangChange.subscribe(_ => {
      this.updateData();
    });
  }

  public onDocumentStatusesChanged(event) {
    const addedStatuses = this.reportRequest.documentStatuses.filter(color => !this.previousSelectedDocStatuses.includes(color));
    if (addedStatuses.includes(DocStatus.All)) {
      this.reportRequest.documentStatuses = [DocStatus.All];
    } else {
      const allIndex = this.reportRequest.documentStatuses.indexOf(DocStatus.All);
      if (allIndex !== -1) {
        this.reportRequest.documentStatuses.splice(allIndex, 1);
      }
    }
    this.reportRequest.documentStatuses = [...this.reportRequest.documentStatuses];
    this.previousSelectedDocStatuses = this.reportRequest.documentStatuses;
  }

  public ngOnInit(): void {
    this.userApiService.GetUserGroups().subscribe(res => {
      this.groups = res.groups;
    });

    this.autoReportsNotificationService.newReportNotification$.subscribe(() => {
      this.showFrequencyReportCreation = false;
    });

    this.autoReportsNotificationService.deletedReportNotification$.subscribe(() => {
      this.showFrequencyReportCreation = false;
    });
  }

  public updateData() {
    this.initOptionsReportTypes();
    this.initDocumentStatuses();
    this.initReportFrequencies();
    this.initDistributionOptions();
  }

  public submitRequest() {
    let copyRequest: ReportRequest = JSON.parse(JSON.stringify(this.reportRequest));
    if (this.reportType != null) {
      if (this.reportRequest.documentStatuses.includes(DocStatus.All)) {
        copyRequest.documentStatuses = this.optionalDocStatusesTypes.filter(key => key !== DocStatus.All);
      }
      this.SubmittedRequest.emit({ type: this.reportType, request: copyRequest });
    }
    else {
      this.sharedService.setTranslateAlert("TINY.INVALID_REPORT_TYPE", AlertLevel.ERROR);
    }
  }

  public getCSV() {
    this.reportRequest.isCsv = true;
    this.reportRequest.offset = 0;
    this.reportRequest.limit = 100000;//TODO: Change it later
    let copyRequest: ReportRequest = JSON.parse(JSON.stringify(this.reportRequest));

    if (this.reportType != null) {
      if (this.reportRequest.documentStatuses.includes(DocStatus.All)) {
        copyRequest.documentStatuses = this.optionalDocStatusesTypes.filter(key => key !== DocStatus.All);
      }
      this.SubmittedRequest.emit({ type: this.reportType, request: copyRequest });
    }
    this.reportRequest.isCsv = false;
  }

  public openFrequencyPopup() {
    this.showFrequencyReportCreation = true;
  }

  public createFrequencyReport(event: CreateFrequencyReport) {
    if (event.frequency == null) {
      this.sharedService.setErrorAlert("Invalid frequency selected");
      return;
    }
    if (event.frequency != ReportFrequency.None) {
      this.SubmittedFrequencyRequest.emit(event);
    }
    else {
      this.DeletedFrequencyRequest.emit();
    }
  }

  public cancelFrequencyReport() {
    this.showFrequencyReportCreation = false;
  }

  private getOneYearBefore(): Date {
    const now = new Date();
    return new Date(now.getFullYear() - 1, now.getMonth(), now.getDate(), now.getHours(), now.getMinutes(), now.getSeconds());
  }

  public showFromCalendardWrapper(): void {
    if (!this.isFromCalendarShown) {
      this.showFromCalendar();
    }
  }
  public showFromCalendar(): void {
    this.isFromCalendarShown = !this.isFromCalendarShown;
    this.isToCalendarShown = false;
  }

  public showToCalendardWrapper(): void {
    if (!this.isToCalendarShown) {
      this.showToCalendar();
    }
  }
  public showToCalendar(): void {
    this.isToCalendarShown = !this.isToCalendarShown;
    this.isFromCalendarShown = false;
  }

  // TODO: Add generic mechanism to Dates component to setup min max range
  public fromSelected(date: Date): void {
    const oneYearBefore = this.getOneYearBefore();
    this.reportRequest.from = date < oneYearBefore ? oneYearBefore : date;
    if (this.reportRequest.to != null && this.reportRequest.from > this.reportRequest.to) {
      (new Event("emptyEvent")).preventDefault();
      this.reportRequest.to = null;
    }
  }

  public toSelected(date: Date): void {
    const now = new Date();
    this.reportRequest.to = date > now ? now : date;
    if (this.reportRequest.from != null && this.reportRequest.from > this.reportRequest.to) {
      (new Event("emptyEvent")).preventDefault();
      this.reportRequest.from = null;
    }
  }

  public removeDate($event: any, dateElement: string): void {
    $event.preventDefault();
    if (dateElement == "from") {
      this.reportRequest.from = null;
    }
    else {
      this.reportRequest.to = null;
    }
  }

  private initOptionsReportTypes() {
    forkJoin(
      Object.keys(ReportType)
        .filter(key => {
          const value = ReportType[key as keyof typeof ReportType];
          return typeof value === 'string';
        })
        .map(key =>
          this.getNameObservable(key).pipe(
            map(name => ({
              id: ReportType[key],
              name: name
            }))
          )
        )
    ).subscribe(reportTypes => {
      this.optionalReportTypes = reportTypes;
    });
  }

  private initDocumentStatuses() {
    forkJoin(
      Object.keys(DocStatus)
        .filter(key => {
          const value = DocStatus[key as keyof typeof DocStatus];
          return typeof value === 'number' && this.optionalDocStatusesTypes.includes(value);
        })
        .map(key =>
          this.getNameObservable(key).pipe(
            map(name => ({
              id: DocStatus[key],
              name: name
            }))
          )
        )
    ).subscribe(docStatuses => {
      // Here docStatuses will be the array of results from each observable
      this.docStatuses = docStatuses;
    });
  }

  private initReportFrequencies() {
    this.reportFrequencies = Object.keys(ReportFrequency)
      .filter(key => {
        const value = ReportFrequency[key as keyof typeof ReportFrequency];
        return typeof value === 'number'
      })
      .map(key => ({
        key,
        value: ReportFrequency[key as keyof typeof ReportFrequency]
      }));
  }

  private initDistributionOptions() {
    this.distributionOptions = [];
    this.translate.get("GLOBAL.REPORTS.ENABLE_DISTRIBUTION").subscribe(res => {
      this.distributionOptions.push({ label: res, value: true });
    });
    this.translate.get("GLOBAL.REPORTS.DISABLE_DISTRIBUTION").subscribe(res => {
      this.distributionOptions.push({ label: res, value: false });
    });
  }

  private getNameObservable(key: string): Observable<string> {
    if (key === 'Sent') {
      key = 'Pending';
    }
    return this.translate.get(`GLOBAL.REPORTS.${key}`)
  }
}
