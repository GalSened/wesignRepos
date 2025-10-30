export class LogMessage {
    public message: string = "";
    public timeStamp : Date ;
    public logLevel : LogLevel = LogLevel.Debug;
    public applicationName: string = "";
    public exception: string = "";
    public token : string = "";
}

export enum LogLevel{
    All = 0,
    Debug = 1,
    Information = 2,
    Error = 3 
}
