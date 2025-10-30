import { SmtpConfiguration } from './smtp-configuration.model';
import { BaseUser } from './base-user.model';
import { Notifications } from './notifications.model';
import { Deletion } from './deletion.model';
import { SmsConfiguration } from './sms-configuration.model';
import { ExpandedCompanyResult } from './results/expanded-company-result.model';
import { GroupADMapper } from './group-ad-mapper';
import { SignatureType } from './signature-type.enum';

export class Company{
    public id : string = "";
    public companyName : string ="";
    public groups : [string, string][] = [];  
    public logoBase64String : string  ="";
    public signatureColor : string = "";
    public language : number ;
    public user :BaseUser = new BaseUser();
    public programId : string  ="";
    public expirationTime:  Date = new Date();
    public messageBefore : string  ="";
    public messageBeforeHebrew : string  ="";
    public messageAfter : string  ="";
    public messageAfterHebrew : string  ="";
    public shouldSendWithOTPByDefault : boolean  =false;
    public shouldForceOTPInLogin : boolean  =false;
    public shouldEnableMeaningOfSignatureOption: boolean  =false;
    public shouldEnableVideoConference: boolean  =false;
    public shouldAddAppendicesAttachmentsToSendMail: boolean  =false;    
    public enableVisualIdentityFlow : boolean  =false; 
    public enableDisplaySignerNameInSignature: boolean = true;  
    public isPersonalizedPFX : boolean = false;
    public defaultSigningType : SignatureType =  SignatureType.Graphic; 
    public smtpConfiguration : SmtpConfiguration= new SmtpConfiguration() ;
    public smsConfiguration : SmsConfiguration = new SmsConfiguration();
    public notifications : Notifications = new Notifications();
    public deletionDetails : Deletion= new Deletion();
    public groupsADMapper : GroupADMapper[] = [];
    public companySigner1Details : Signer1Details  = new Signer1Details();
    public transactionId:string;
    public recentPasswordsAmount: number;
    public passwordExpirationInDays: number;
    public minimumPasswordLength: number;
    public enableTabletsSupport: boolean = false;
    // public shouldEnableGovernmentSignatureFormat: boolean = false;
}

export class Signer1Details {
    public shouldShowInUI: boolean = false;
    public shouldSignAsDefaultValue: boolean = false;
    public certId : string = "";
    public certPassword : string = "";
    public signer1Configuration : Signer1Confguration = new Signer1Confguration();

}

export class Signer1Confguration  {
    public endpoint: string = "";
    public user: string = "";
    public password: string = "";
}