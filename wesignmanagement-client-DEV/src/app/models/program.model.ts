export class Program{
    public id: string ="";
    public name: string ="";
    public users: number = 0;
    public templates: number = 0;
    public documentsPerMonth: number = 0;
    public smsPerMonth: number = 0;
    public visualIdentificationsPerMonth: number = 0;
    public videoConferencePerMonth: number = 0;
    public serverSignature: boolean = true;
    public smartCard: boolean = true;
    public onlineMode: boolean = true;
    public note: string ="";
    public useActiveDirectory: boolean = false;
    public uiViewLicense : UIViewLicense = new UIViewLicense() ;
}

export class UIViewLicense{
    public shouldShowSelfSign: boolean = true;
    public shouldShowGroupSign: boolean = true;
    public shouldShowLiveMode: boolean = true;
    public shouldShowContacts: boolean = true;
    public shouldShowTemplates: boolean = true;
    public shouldShowDocuments: boolean = true;
    public shouldShowProfile: boolean = true;
    public shouldShowUploadAndsign: boolean = true;
    public shouldShowAddNewTemplate : boolean = true;
    public shouldShowEditTextField : boolean = true;
    public shouldShowEditSignatureField : boolean = true;
    public shouldShowEditEmailField : boolean = true;
    public shouldShowEditPhoneField : boolean = true;
    public shouldShowEditDateField : boolean = true;
    public shouldShowEditNumberField : boolean = true;
    public shouldShowEditListField : boolean = true;
    public shouldShowEditCheckboxField : boolean = true;
    public shouldShowEditRadioField : boolean = true;
    public shouldShowDistribution : boolean = false;
    public shouldShowMultilineText : boolean = true;
}