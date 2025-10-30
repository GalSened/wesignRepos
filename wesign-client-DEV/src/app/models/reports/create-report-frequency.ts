import { ReportFrequency } from '@models/enums/report-frequency';
import { ReportType } from './report-type.enum';

export class CreateReportFrequency {
    type: ReportType;
    frequency: ReportFrequency;
}