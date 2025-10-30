export class Notifications{
    public shouldSendSignedDocument : boolean;
    public shouldNotifyWhileSignerSigned : boolean;
    public signerLinkExpirationInHours : number;
    public shouldEnableSignReminders: boolean;
    public canUserControlReminderSettings: boolean;
    public signReminderFrequencyInDays: number;
    public shouldSendDocumentNotifications: boolean;
    public documentNotificationsEndpoint: string;

    constructor()
    {
        this.shouldEnableSignReminders = false;
        this.canUserControlReminderSettings = true;
        this.signReminderFrequencyInDays = 0;
    }

}




