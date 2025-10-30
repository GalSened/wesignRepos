export class usageByUserReport {
    public companyName: string;
    public groupName: string;
    public email: string;
    public sentDocumentsCount: number;
    public signedDocumentsCount: number;
    public declinedDocumentsCount: number;
    public canceledDocumentsCount: number;
    public deletedDocumentsCount: number;
}

export class usageByUserReports {
    public usageByUsersReports: usageByUserReport[];
}