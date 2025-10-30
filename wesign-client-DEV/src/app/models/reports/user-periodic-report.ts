import { ReportFrequency } from '@models/enums/report-frequency';
import { ReportType } from './report-type.enum';

export class UserPeriodicReport {
    public id: string;
    public userId: string;
    public reportType: ReportType;
    public lastTimeSent: Date;
    public reportFrequency: ReportFrequency;
}

export class UserPeriodicReports {
    public userPeriodicReports: UserPeriodicReport[];
}