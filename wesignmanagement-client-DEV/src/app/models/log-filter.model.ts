export class LogFilter{
    public key: string = "";
    public logLevel: number = 3;
    public offset: number = 0;
    public limit: number = 10;
    public from: Date= null;
    public to: Date= null;
    public applicationSource: ApplicationSource = ApplicationSource.UserApp; 
}

export enum ApplicationSource{
    UserApp = 0,
    SignerApp = 1,
    ManagementApp = 2
}