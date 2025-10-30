import { Component, OnInit, ViewChild } from '@angular/core';
import { ReportRequest } from '@models/reports/report-request';
import { ReportOutputTableComponent } from './report-output-table/report-output-table.component';
import { CreateReportFrequency } from '@models/reports/create-report-frequency';
import { ReportType } from '@models/reports/report-type.enum';
import { SharedService } from '@services/shared.service';
import { AutoReportsNotificationService } from '@services/auto-reports-notification.service';

@Component({
  selector: 'sgn-reports',
  templateUrl: './reports.component.html',
  styles: [
  ]
})
export class ReportsComponent implements OnInit {
  @ViewChild(ReportOutputTableComponent) tableOutput: ReportOutputTableComponent;

  constructor(private readonly sharedService: SharedService,
    private readonly autoReportsNotificationService:AutoReportsNotificationService
  ) {
  }

  public ngOnInit(): void {
  }

  SubmittedRequest(newReportRequest: {type: ReportType, request: ReportRequest}) {
    this.tableOutput.callApi(newReportRequest);
  }

  SubmittedFrequencyRequest(request: CreateReportFrequency) {
    this.tableOutput.createFrequencyRequest(request).subscribe(res => {
      this.autoReportsNotificationService.onAddedReport();
      this.sharedService.setSuccessAlert("TINY.FREQUENCY_REPORT_SUCCESS");
    });
  }

  DeletedFrequencyRequest() {
    this.tableOutput.deleteFrequencyRequests().subscribe(res => {
      this.autoReportsNotificationService.onDeletedReport();
      this.sharedService.setSuccessAlert("TINY.DELETE_FREQUENCY_REPORT_SUCCESS");
    });
  }
}
