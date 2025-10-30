import { Component } from '@angular/core';
import { CreateReportFrequency } from '@models/reports/create-report-frequency';
import { ReportRequest } from '@models/reports/report-request';
import { ReportType } from '@models/reports/report-type.enum';
import { UsageDataReport as UsageDataReport } from '@models/reports/results/documents-report';
import { ReportApiService } from '@services/report-api.service';
import { SharedService } from '@services/shared.service';
import { AlertLevel } from '@state/app-state.interface';

@Component({
  selector: 'sgn-report-output-table',
  templateUrl: './report-output-table.component.html',
  styles: [
  ]
})
export class ReportOutputTableComponent {
  usageDataReports: UsageDataReport[];
  constructor(private readonly reportApiService: ReportApiService,
    private readonly sharedService: SharedService
  ) {
  }

  public callApi(newReportRequest: { type: ReportType, request: ReportRequest }) {
    switch (newReportRequest.type) {
      case ReportType.UsageData:
        if (!newReportRequest.request.isCsv) {
          this.reportApiService.readUsageDataReports(newReportRequest.request).subscribe(res => {
            this.usageDataReports = res.usageDataReports;
            if (this.usageDataReports.length == 0) {
              this.sharedService.setTranslateAlert("TINY.EMPTY_TABLE", AlertLevel.ERROR);
            }
          });
        }
        else {
          this.reportApiService.readUsageDataReportsCSV(newReportRequest.request);
        }
        break;
      default:
        break;
    }
  }

  public createFrequencyRequest(frequencyRequest: CreateReportFrequency) {
    return this.reportApiService.createFrequencyReports(frequencyRequest);
  }

  public deleteFrequencyRequests() {
    return this.reportApiService.deleteFrequencyReports();
  }
}
