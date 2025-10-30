export class groupDocumentStatusesReport {
    public groupName: string
    public createdDocs: number
    public sentDocs: number
    public viewdDocs: number
    public signedDocs: number
    public declinedDocs: number 
    public deletedDocs: number
    public canceledDocs: number
    public serverSignedDocs: number


}

export class groupDocumentStatusesReports {
    public groupDocumentReports: groupDocumentStatusesReport[];
}