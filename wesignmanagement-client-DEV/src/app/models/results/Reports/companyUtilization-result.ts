export class companyReport {
    public companyId: string;
    public companyName: string;
    public startDate: string;
    public expireDate: string;
    public documentsUsage: number = 0;
    public SMSUsage: number = 0;
    public periodicDocumentUsage: number = 0;
    public periodicSMSUsage: number = 0;
    public lastMonthDocumentUsagePercentage: number = 0;
}

export class companyReports {
    public companyReports: companyReport[];
}