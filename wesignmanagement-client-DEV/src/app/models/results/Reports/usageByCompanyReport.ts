export class usageByCompanyReport {
    public CompanyName: string;
    public GroupName: string;
    public SentDocumentsCount: number;
    public SignedDocumentsCount: number;
    public DeclinedDocumentsCount: number;
    public CanceledDocumentsCount: number;
}

export class usageByCompanyReports {
    public usageByCompaniesReports: usageByCompanyReport[];
}