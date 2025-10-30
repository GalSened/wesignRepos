import { ReportFrequency } from "src/app/enums/report-frequency";
import { ReportParameters } from "../report-parameters";
import { ReportMode } from "src/app/enums/ReportMode.enum";

export class FrequencyReportRequest {
    public reportParameters: ReportParameters;
    public frequency: ReportFrequency;
    public reportType: ReportMode;
    public emailsToSend: string[];
}