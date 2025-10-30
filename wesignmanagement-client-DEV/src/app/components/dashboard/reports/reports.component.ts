import { Component, OnInit, ViewChild } from '@angular/core';
import { ReportMode } from 'src/app/enums/ReportMode.enum';
import { ReportRequest } from 'src/app/models/ReportRequest';
import { ReportOutputTableComponent } from './report-output-table/report-output-table.component';
import { BannerAlertComponent } from '../../shared/banner-alert/banner-alert.component';

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.css']
})
export class ReportsComponent implements OnInit {
  @ViewChild(ReportOutputTableComponent) tableOutput: ReportOutputTableComponent;
  public submittedRequest: ReportRequest;
  public submittedMode: ReportMode;
  public months: number;
  @ViewChild('bannerAlert', { static: true }) bannerAlert: BannerAlertComponent;
  
  constructor() { }

  ngOnInit(): void {
  }

  SubmittedRequest(request: ReportRequest) {
    this.tableOutput.callApi(request);
  }

  SubmittedMode(mode) {
    this.submittedMode = mode;
  }

  monthsForAvgUse(month) {
    this.months = month;
  }
}
