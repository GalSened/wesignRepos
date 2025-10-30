import { userConfiguration } from './user-configuration.model';
import { userType } from '@models/enums/user-type.enum';
import { UserProgram } from '@models/program/user-program.model';
import { SignatureType } from '@models/enums/signature-type.enum';

export class User {
    public id: string;
    public groupId: string;
    public companyId: string;
    public name: string;
    public companyName: string;
    public groupName: string;
    public email: string;
    public username: string;
    public phone: string;
    public type: userType;
    public creationTime: Date;
    public programUtilizationId: string;
    public userConfiguration: userConfiguration;    
    public companyLogo: string;
    public program: UserProgram;
    public companySigner1Details : CompanySigner1Details;
    public shouldSendWithOTPByDefault: boolean = false;
    public enableVisualIdentityFlow: boolean = false;
    public enableVideoConferenceFlow: boolean = false;
    public defaultSigningType : SignatureType = SignatureType.Graphic;
    public transactionId :string
    public enableSignReminderSettings: boolean;
    public enableDisplaySignerNameInSignature: boolean;
    public additionalGroupsIds: string[];
    public passwordSetupTime: Date;
    public enableMeaningOfSignature : boolean;
    public shouldSignEidasSignatureFlow : boolean;
    public enableTabletsSupport : boolean;
    // public shouldEnableGovernmentSignatureFormat: boolean;
}

export class CompanySigner1Details{
    public certId :string;
    public certPassword :string;
    public shouldSignAsDefaultValue :boolean;
    public shouldShowInUI :boolean;
}
