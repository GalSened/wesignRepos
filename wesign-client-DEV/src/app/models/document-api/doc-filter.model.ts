export enum SearchParameter {
    DocumentName = 0,
    SignerDetails = 1,
    SenderDetails = 2,
}

export class DocFilter {
    public key: string = "";
    public sent: boolean = true;
    public viewed: boolean = true;
    public signed: boolean = true;
    public declined: boolean = true;
    public sendingFailed: boolean = true;
    public canceled: boolean = true;
    public userId: string = null;
    public from: Date = null;
    public to: Date = null;

    public offset: number = 0;
    public limit: number = 0;
    public searchParameter: SearchParameter = SearchParameter.DocumentName; 
}
