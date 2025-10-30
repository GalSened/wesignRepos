import { BaseUser } from '../base-user.model';
import { SmsConfiguration } from '../sms-configuration.model';
import { Notifications } from '../notifications.model';
import { Deletion } from '../deletion.model';
import { SmtpConfiguration } from '../smtp-configuration.model';
import { GroupADMapper } from '../group-ad-mapper';
import { Signer1Details } from '../company.model';
import { SignatureType } from '../signature-type.enum';


export class ExpandedCompanyResult {
    public id : string = "";    
    public companyName : string = "";    
    public groups :  [string, string][] = [];  
    public logoBase64String : string = "";    
    public signatureColor : string = "";  
    public language :number = 0;
    public user : BaseUser ;  
    public programId : string = "";  
    public expirationTime : Date ;  
    public messageBefore : string = "";  
    public messageBeforeHebrew : string = "";  
    public shouldSendWithOTPByDefault : boolean  =false;
    public defaultSigningType : SignatureType =  SignatureType.Graphic;
    public enableVisualIdentityFlow : boolean  =false;    
    public enableDisplaySignerNameInSignature : boolean = true;       
    public isPersonalizedPFX: boolean = false;
    public messageAfter : string = "";  
    public messageAfterHebrew : string = "";  
    public smtpConfiguration : SmtpConfiguration= new SmtpConfiguration();
    public deletionDetails : Deletion = new Deletion();
    public smsConfiguration : SmsConfiguration = new SmsConfiguration();
    public notifications : Notifications = new Notifications();
    public activeDirectoryGroups : GroupADMapper[] =[];
    public companySigner1Details : Signer1Details  = new Signer1Details();
    public transactionId :string;
    public shouldForceOTPInLogin : boolean;
    public recentPasswordsAmount: number;
    public passwordExpirationInDays: number;
    public minimumPasswordLength: number;
    public shouldEnableMeaningOfSignatureOption: boolean  =false;
    public shouldEnableVideoConference: boolean  =false;
    public shouldAddAppendicesAttachmentsToSendMail : boolean = true; 
    public enableTabletsSupport : boolean = false; 
    // public shouldEnableGovernmentSignatureFormat: boolean = false;
}