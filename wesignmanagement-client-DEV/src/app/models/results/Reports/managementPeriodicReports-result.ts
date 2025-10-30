import { ReportFrequency } from "src/app/enums/report-frequency";
import { ReportMode } from "src/app/enums/ReportMode.enum";
import { ManagementPeriodicReportEmail } from "./managementPeriodicReportEmail";

export class ManagementPeriodicReport {
    public id: string;
    public reportType: ReportMode;
    public reportFrequency: ReportFrequency;
    public emails: ManagementPeriodicReportEmail[];
    public emailsStrFormat: string;
}

export class ManagementPeriodicReports {
    public managementPeriodicReports: ManagementPeriodicReport[];
}