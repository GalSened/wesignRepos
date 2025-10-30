export class UsageDataReport {
    public groupName: string;
    public pendingDocumentsCount: number;
    public signedDocumentsCount: number;
    public declinedDocumentsCount: number;
    public canceledDocumentsCount: number;
    public distributionDocumentsCount: number;
}

export class UsageDataReports {
    public usageDataReports: UsageDataReport[];
}