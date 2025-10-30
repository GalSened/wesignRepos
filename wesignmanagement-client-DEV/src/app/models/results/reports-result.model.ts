export class ReportResult {
    public id: string;
    public companyId: string;
    public companyName: string;
    public resourceMode :ProgramUtilizationHistoryResourceMode;
    public updateDate : string = "";
    public expired : string = "";
    public documentsUsage : number = 0;
    public smsUsage : number = 0;
    public templatesUsage : number = 0;
    public usersUsage : number = 0;
}

export class ReportsResult {
    public reports: ReportResult[];
}
export enum ProgramUtilizationHistoryResourceMode {
    FromRestProgramUtilizationJob = 0,
    FromUpdateComapny = 1
}