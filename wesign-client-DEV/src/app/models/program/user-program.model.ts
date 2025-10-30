import { pluck } from 'rxjs/operators';

export class UserProgram {
    public name: string;
    public note: string;
    public expiredTime: Date;
    public lastResetDate: Date;
    public remainingDocumentsForMonth: number;
    public visualIdentificationsLimit: number; 
    public remainingVisualIdentifications: number;
    public remainingVideoConference: number;
    public documentsForMonth: number;
    public smsLimit: number;
    public remainingSMS: number;
    public remainingTemplates: number;
    public templatesLimit: number;
    public remainingUsers: number;
    public usersLimit: number;
    public uiViewLicense : UiViewLicense;
    public serverSignature : boolean;
    public smartCard : boolean;
    public programResetType : ProgramResetType;
    public isSmsProviderSupportGloballySend : boolean = false;
}

export enum ProgramResetType{
    Monthly = 0,
    DocumentsLimitOnly = 1,
    TimeAndDocumentsLimit = 2,
    Yearly = 3
}

export class UiViewLicense {
    public shouldShowSelfSign: boolean = false;
    public shouldShowGroupSign: boolean = false;
    public shouldShowLiveMode: boolean = false;
    public shouldShowContacts: boolean = false;
    public shouldShowTemplates: boolean = false;
    public shouldShowDocuments: boolean = false;
    public shouldShowProfile: boolean = false;
    public shouldShowUploadAndsign: boolean = false;
    public shouldShowAddNewTemplate : boolean = false;
    public shouldShowEditTextField : boolean = false;
    public shouldShowEditSignatureField : boolean = false;
    public shouldShowEditEmailField : boolean = false;
    public shouldShowEditPhoneField : boolean = false;
    public shouldShowEditDateField : boolean = false;
    public shouldShowEditNumberField : boolean = false;
    public shouldShowEditListField : boolean = false;
    public shouldShowEditCheckboxField : boolean = false;
    public shouldShowEditRadioField : boolean = false;
    public shouldShowDistribution : boolean = false;
    public shouldShowMultilineText : boolean = false;
}