import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { ReportFrequency } from '@models/enums/report-frequency';
import { CreateReportFrequency } from '@models/reports/create-report-frequency';
import { ReportType } from '@models/reports/report-type.enum';
import { TranslateService } from '@ngx-translate/core';
import { ReportApiService } from '@services/report-api.service';
import { forkJoin, Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
  selector: 'sgn-add-report-frequency',
  templateUrl: './add-report-frequency.component.html',
  styles: [
  ]
})
export class AddReportFrequencyComponent implements OnInit {
  createReportFrequency: CreateReportFrequency;
  allReportsFrequencies: Map<string, ReportFrequency>;
  reportTypes: any[];
  reportFrequencies: any[];

  @Output()
  public cancel = new EventEmitter<void>();

  @Output()
  public submit = new EventEmitter<CreateReportFrequency>();
  constructor(private translate: TranslateService, private reportsService: ReportApiService) {
    this.createReportFrequency = new CreateReportFrequency();
    this.allReportsFrequencies = new Map<string, ReportFrequency>();
    this.initOptionsReportTypes();
    this.initReportFrequencies();
  }

  public ngOnInit(): void {
    this.reportsService.readUserPeriodicReports().subscribe(res => {
      if (res != null && res.userPeriodicReports != null) {
        res.userPeriodicReports.forEach(report => {
          const reportTypeString = Object.values(ReportType)[report.reportType];
          if (this.allReportsFrequencies.has(reportTypeString)) {
            this.allReportsFrequencies[reportTypeString] = report.reportFrequency;
          }
          else {
            this.allReportsFrequencies.set(reportTypeString, report.reportFrequency);
          }
        });
      }
    });
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
      this.reportTypes = reportTypes;
    });
  }

  private initReportFrequencies() {
    forkJoin(
      Object.keys(ReportFrequency)
        .filter(key => {
          const value = ReportFrequency[key as keyof typeof ReportFrequency];
          return typeof value === 'number';
        })
        .map(key =>
          this.getNameObservable(key).pipe(
            map(name => ({
              id: ReportFrequency[key],
              name: name
            }))
          )
        )
    ).subscribe(frequencies => {
      this.reportFrequencies = frequencies;
    });
  }

  public save() {
    this.submit.emit(this.createReportFrequency);
  }

  public close() {
    this.cancel.emit();
  }

  public onReportTypeChanged(event) {
    const value = event.id;
    const existsInEnum = Object.values(ReportType).includes(value);
    if (existsInEnum && this.allReportsFrequencies.has(value)) {
      this.createReportFrequency.frequency = this.allReportsFrequencies.get(value);
    }
  }

  private getNameObservable(key: string): Observable<string> {
    return this.translate.get(`GLOBAL.REPORTS.${key}`)
  }
}
