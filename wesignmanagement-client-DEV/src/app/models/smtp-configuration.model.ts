export class SmtpConfiguration{
    public beforeSigningHtmlTemplateBase64String : string = "";
    public afterSigningHtmlTemplateBase64String : string = "";
    public smtpFrom : string = "";
    public smtpServer : string = "";
    public smtpPort : string = "";
    public smtpUser : string = "";
    public smtpPassword : string = "";
    public smtpEnableSsl: boolean = false;
}

export class SmtpDetails{
    public from : string ;
    public server : string ;
    public port : number ;
    public user: string ;
    public password: string ;    
    public message : string;
    public email : string;
    public enableSsl: boolean;
}