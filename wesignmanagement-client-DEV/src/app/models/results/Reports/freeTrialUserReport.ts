export class freeTrialUserReport {
    public Name: string;
    public email: string;
    public userName: string;
    public documentsUsage: number = 0;
    public smsUsage: number = 0;
    public templatesUsage: number = 0;
    public creationDate: Date = new Date();
    public expirationDate: Date = new Date();
     
}

export class freeTrialUserReports {
    public freeTrialUsersReports: freeTrialUserReport[];
}