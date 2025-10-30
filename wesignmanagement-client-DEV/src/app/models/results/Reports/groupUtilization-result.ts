export class groupUtilizationReport {
    public groupName: string
    public periodicDocumentUsage: number = 0;
    public periodicSMSUsage: number = 0;

}

export class groupUtilizationReports {
    public groupReports: groupUtilizationReport[];
}