
import { Component, OnInit, Output, ViewChild } from '@angular/core';
import { EventEmitter } from '@angular/core';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { AlertType, BannerAlertComponent } from 'src/app/components/shared/banner-alert/banner-alert.component';
import { ReportFrequency } from 'src/app/enums/report-frequency';
import { ReportMode, ReportModeLabels } from 'src/app/enums/ReportMode.enum';
import { Filter } from 'src/app/models/filter.model';
import { Group } from 'src/app/models/group.model';
import { ProgramFilter } from 'src/app/models/program-filter.model';
import { Program } from 'src/app/models/program.model';
import { ReportRequest } from 'src/app/models/ReportRequest';
import { FrequencyReportRequest } from 'src/app/models/requests/frequency-report-request';
import { CompanyResult } from 'src/app/models/results/companany-result.model';
import { ManagementPeriodicReportEmail } from 'src/app/models/results/Reports/managementPeriodicReportEmail';
import { ManagementPeriodicReport } from 'src/app/models/results/Reports/managementPeriodicReports-result';
import { SignatureType } from 'src/app/models/signature-type.enum';
import { User } from 'src/app/models/user.model';
import { CompaniesApiService } from 'src/app/services/companies-api.service';
import { ProgramsApiService } from 'src/app/services/programs-api.service';
import { ReportsApiService } from 'src/app/services/reports-api.service';
import { UsersApiService } from 'src/app/services/users-api.service';
import { UtilsService as UtilsService } from 'src/app/services/utils.service';
import { v4 as uuidv4 } from 'uuid';

@Component({
  selector: 'app-report-filter',
  templateUrl: './report-filter.component.html',
  styleUrls: ['./report-filter.component.css']
})
export class ReportFilterComponent implements OnInit {
  emptyGuid: string = '00000000-0000-0000-0000-000000000000';;
  @Output() public SubmittedRequest = new EventEmitter<ReportRequest>();
  @Output() public SubmittedMode = new EventEmitter<ReportMode>();
  @Output() public monthsForAvgUse = new EventEmitter<Number>();
  @ViewChild('bannerAlert', { static: true }) bannerAlert: BannerAlertComponent;

  private previousRequest: ReportRequest = null;
  public reportRequest: ReportRequest = new ReportRequest();
  public frequencyReportRequest: FrequencyReportRequest = new FrequencyReportRequest();
  public reportModes = [];
  public reportMode = ReportMode.ExpirationUtilization;
  public allFrequencies = Object.keys(ReportFrequency).filter(key => isNaN((Number(key)))).map(key => ({
    label: key,
    value: ReportFrequency[key]
  }));
  ReportMode = ReportMode;
  ReportFrequency = ReportFrequency;
  public programs$: Observable<Program[]>;
  public programs: Program[];
  public programFilter: ProgramFilter = new ProgramFilter()

  public companies$: Observable<CompanyResult[]>;
  public companies: CompanyResult[];
  public companyFilter: Filter = new Filter()

  public companyGroups$: Observable<Group[]>
  public groups: Group[];

  public users$: Observable<User[]>;
  public users: User[];
  public usersFilter: Filter = new Filter();

  public signatureTypes;
  public reportFrequencies;
  public months: number[] = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12]
  public percentages: number[] = [0, 5, 10, 15, 20, 25, 50, 75, 100]
  public minimums: number[] = [0, 5, 20, 50, 100, 200, 300, 400, 500, 1000]
  public emailsToSendStr: string;
  public isFromCalendarShown = false;
  public isToCalendarShown = false;
  public isAutomatedReportsRequested = false;
  public showDeletionAlert: boolean = false;
  public autoReports: ManagementPeriodicReport[];
  public autoReportToDelete: ManagementPeriodicReport;
  constructor(private reportsApi: ReportsApiService,
    private programsApiService: ProgramsApiService,
    private companyApiService: CompaniesApiService,
    private usersApiService: UsersApiService,
    private utilsService: UtilsService) { }

  ngOnInit(): void {
    this.programFilter.limit = 1000000 // TODO change to a negative 1 or some other inifite CONST
    this.programs$ = this.programsApiService.Read(this.programFilter).pipe(
      tap(), map((res) => res.body.programs)
    );
    this.programs$.subscribe((programs) => {
      this.programs = programs;
    });

    this.reportModes = Object.values(ReportMode)  // Get the numeric enum values
      .filter(value => !isNaN(Number(value)))  // Filter out the string keys
      .filter(value => value !== ReportMode.UsePercentageUtilization)
      .map(value => ({
        value: value, // Numeric value
        label: ReportModeLabels[value]  // Corresponding label from the ReportModeLabels object
      }));

    this.companyFilter.limit = 1000000;
    this.companies$ = this.companyApiService.Read(this.companyFilter).pipe(
      tap(), map((res => res.body.companies))
    );
    this.companies$.subscribe((companies) => {
      this.companies = companies;
    });

    this.usersFilter.limit = 1000000;
    this.users$ = this.usersApiService.Read(this.usersFilter).pipe(
      tap(), map((res => res.body.users))
    );
    this.users$.subscribe((users) => {
      this.users = users;
    });

    this.getCurrentAutoReports();
    this.signatureTypes = Object.keys(SignatureType).filter(key => typeof SignatureType[key as keyof typeof SignatureType] === 'number');
    this.reportFrequencies = Object.keys(ReportFrequency).filter(key => typeof ReportFrequency[key as keyof typeof ReportFrequency] === 'number');
    this.SubmittedMode.emit(this.reportMode);
    this.monthsForAvgUse.emit(this.reportRequest.monthsForAvgUse);
  }

  public programDropDownUpdate(selectedId: string) {
    this.reportRequest.programId = selectedId;
  }

  onCompanySelect() {
    this.groups = [];
    this.reportRequest.groupIds = [];
    if (!this.reportRequest.companyId)
      return;
    if (this.reportMode == ReportMode.DocsByUsers || this.reportMode == ReportMode.DocsBySigners
      || this.reportMode == ReportMode.UsageByUsers || this.reportMode == ReportMode.UsageByCompanies
      || this.reportMode == ReportMode.TemplatesByUsage) {
      this.companyGroups$ = this.reportsApi.ReadCompanyGroups(this.reportRequest.companyId).pipe(
        tap(), map((res) => res.body.groups));
      this.companyGroups$.subscribe((groups) => {
        this.groups = groups;
      });
    }
  }

  changeMode() {
    this.groups = [];
    this.reportRequest = new ReportRequest(this.reportRequest.monthsForAvgUse);
    this.monthsForAvgUse.emit(this.reportRequest.monthsForAvgUse);
    this.SubmittedMode.emit(this.reportMode);
  }

  monthsChanged() {
    this.monthsForAvgUse.emit(this.reportRequest.monthsForAvgUse);
  }

  submitRequest() {
    if (!this.previousRequest || !this.utilsService.deepEqual(this.previousRequest, this.reportRequest)) {
      this.SubmittedRequest.emit(this.reportRequest);
      this.previousRequest = Object.assign({}, this.reportRequest);
    }
  }

  getCSV() {
    this.reportRequest.isCSV = true;
    this.reportRequest.offset = 0;
    this.reportRequest.limit = 100000;
    this.SubmittedRequest.emit(this.reportRequest);
    this.reportRequest.isCSV = false;
  }

  onAutomatedReports() {
    this.isAutomatedReportsRequested = true;
  }

  saveAutomatedReports() {
    if (!this.emailsToSendStr || this.emailsToSendStr.split(";").length == 0) {
      this.bannerAlert.showBannerAlert("At least one email is required", AlertType.FAILED);
      return;
    }
    this.frequencyReportRequest.emailsToSend = this.emailsToSendStr.split(";");
    if (this.hasDuplicates(this.frequencyReportRequest.emailsToSend)) {
      this.bannerAlert.showBannerAlert("Any email can only be used once. Please provide a unique email addresses.", AlertType.FAILED);
      return;
    }
    if (this.frequencyReportRequest.emailsToSend.some(email => !this.isValidEmail(email))) {
      this.bannerAlert.showBannerAlert("One or more emails are invalid", AlertType.FAILED);
      return;
    }
    if (this.frequencyReportRequest.frequency == null) {
      this.bannerAlert.showBannerAlert("Report frequency is required", AlertType.FAILED);
      return;
    }
    if (this.reportMode == ReportMode.ProgramUtilization && this.reportRequest.programId == null) {
      this.bannerAlert.showBannerAlert("Missing program", AlertType.FAILED);
      return;
    }
    if (this.reportMode == ReportMode.UsageByUsers && this.reportRequest.userEmail == null && this.reportRequest.companyId == null) {
      this.bannerAlert.showBannerAlert("Missing email or company", AlertType.FAILED);
      return;
    }
    if ((this.reportMode == ReportMode.GroupUtilization || this.reportMode == ReportMode.GroupDocumentStatuses || this.reportMode == ReportMode.DocsByUsers
      || this.reportMode == ReportMode.DocsBySigners || this.reportMode == ReportMode.CompanyUsers || this.reportMode == ReportMode.UsageByCompanies
      || this.reportMode == ReportMode.TemplatesByUsage || this.reportMode == ReportMode.UsageBySignatureType) && this.reportRequest.companyId == null) {
      this.bannerAlert.showBannerAlert("Missing company", AlertType.FAILED);
      return;
    }
    this.frequencyReportRequest.reportParameters = {
      offset: this.reportRequest.offset,
      limit: this.reportRequest.limit,
      programUtilizationHistoryKey: null,
      isProgramUtilizationExpired: this.reportRequest.isExpired,
      monthsForAvgUse: this.reportRequest.monthsForAvgUse,
      programId: this.reportRequest.programId ?? this.emptyGuid,
      docsUsagePercentage: this.reportRequest.docUsagePercentage,
      companyId: this.reportRequest.companyId ?? this.emptyGuid,
      groupIds: this.reportRequest.groupIds,
      minDocs: this.reportRequest.minDocs,
      minSms: this.reportRequest.minSMS,
      isProgramUsed: this.reportRequest.isProgramUsed,
      userEmail: this.reportRequest.userEmail,
      signatureTypes: this.reportRequest.signatureTypes
    };
    this.frequencyReportRequest.reportType = this.reportMode;
    this.reportsApi.CreateFrequencyReport(this.frequencyReportRequest).subscribe(res => {
      this.getCurrentAutoReports();
      this.bannerAlert.showBannerAlert("Frequency reports created successfully", AlertType.SUCCESS);
    });
    this.SubmittedRequest.emit(this.reportRequest);
  }

  onFrequencyReportCreated() {
    this.reportsApi.ReadFrequencyReports().pipe(map((res) => res.body.managementPeriodicReports)).subscribe(frequencyReports => {
      this.autoReports = frequencyReports;
      for (let index = 0; index < this.autoReports.length; index++) {
        this.autoReports[index].emailsStrFormat = this.autoReports[index].emails.map(_ => _.email).join(';');
      }
      this.autoReports = [...this.autoReports];
    });
  }

  cancelAutomatedReports() {
    this.isAutomatedReportsRequested = false;
  }

  public showFromCalendar() {
    this.isFromCalendarShown = !this.isFromCalendarShown;
    this.isToCalendarShown = false;
  }

  public showToCalendar() {
    this.isToCalendarShown = !this.isToCalendarShown;
    this.isFromCalendarShown = false;
  }

  public fromSelected(date: Date) {
    this.reportRequest.from = date;
  }

  public removeDate($event: any, dateElement: any) {
    $event.preventDefault();
    this.reportRequest[dateElement] = null;
  }

  public toSelected(date: Date) {
    this.reportRequest.to = date;
  }

  public updateAutoReport(autoReport: ManagementPeriodicReport) {
    if (!autoReport.emailsStrFormat || autoReport.emailsStrFormat.split(";").length == 0) {
      this.bannerAlert.showBannerAlert("At least one email is required", AlertType.FAILED);
      return;
    }
    const emailList = autoReport.emailsStrFormat.split(';').map(email => email.trim());
    if (this.hasDuplicates(emailList)) {
      this.bannerAlert.showBannerAlert("Any email can only be used once. Please provide a unique email addresses.", AlertType.FAILED);
      return;
    }
    if (emailList.some(email => !this.isValidEmail(email))) {
      this.bannerAlert.showBannerAlert("One or more emails are invalid", AlertType.FAILED);
      return;
    }
    if (autoReport.reportFrequency == null) {
      this.bannerAlert.showBannerAlert("Report frequency is required", AlertType.FAILED);
      return;
    }
    autoReport.emails = [];
    emailList.forEach(email => {
      var newMail = new ManagementPeriodicReportEmail();
      newMail.id = uuidv4();
      newMail.periodicReportId = autoReport.id;
      newMail.email = email;
      autoReport.emails.push(newMail);
    });
    this.reportsApi.UpdateFrequencyReport(autoReport).subscribe(res => {
      this.bannerAlert.showBannerAlert("Periodic report is updated successfully", AlertType.SUCCESS);
    });
  }

  public openDeletionAlert(autoReport: ManagementPeriodicReport) {
    this.autoReportToDelete = autoReport;
    this.showDeletionAlert = true;
  }

  public deleteAutoReport() {
    this.reportsApi.DeleteFrequencyReport(this.autoReportToDelete.id).subscribe(res => {
      this.getCurrentAutoReports();
      this.bannerAlert.showBannerAlert("Periodic report is deleted successfully", AlertType.SUCCESS);
      this.showDeletionAlert = false;
    }, (err) => {
      this.bannerAlert.showBannerAlert("Periodic report is failed to delete", AlertType.FAILED);
    });
  }

  public hideAlert() {
    this.showDeletionAlert = false;
  }

  hasDuplicates(arr: string[]): boolean {
    return arr.filter((item, index) => arr.indexOf(item) !== index).length > 0;
  }

  isValidEmail(email: string): boolean {
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    return emailRegex.test(email);
  }

  private getCurrentAutoReports() {
    this.reportsApi.ReadFrequencyReports().pipe(map((res) => res.body.managementPeriodicReports)).subscribe(frequencyReports => {
      this.autoReports = frequencyReports;
      for (let index = 0; index < this.autoReports.length; index++) {
        this.autoReports[index].emailsStrFormat = this.autoReports[index].emails.map(_ => _.email).join(';');
      }
    });
  }
}
