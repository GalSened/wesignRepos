export class LogResult{
    public message : string = "";
    public logLevel : LogLevel;
    public timeStamp : string = "";
}

export enum LogLevel{
    All = 0,
    Debug = 1,
    Information = 2,
    Error = 3,
}