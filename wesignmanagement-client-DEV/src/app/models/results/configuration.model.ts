import { ActiveDirecrotyConfiguration } from './active-directory-configuration.model';

export class Configuration {
    public messageAfter: string = "";
    public messageAfterHebrew: string = "";
    public messageBefore: string = "";
    public messageBeforeHebrew: string = "";
    public deleteSignedDocumentAfterXDays: number = 0;
    public deleteUnsignedDocumentAfterXDays: number = 0;

    public signer1Endpoint: string = "";
    public signer1User: string = "";
    public signer1Password: string = "";

    public smsFrom: string = "";
    public smsPassword: string = "";
    public smsUser: string = "";
    public smsProvider: number = 0;
    public smsLanguage: number = 0;

    public smtpAttachmentMaxSize: number = 0;
    public smtpPort: number = 0;
    public smtpEnableSsl: boolean = false;
    public smtpFrom: string = "";
    public smtpPassword: string = "";
    public smtpServer: string = "";
    public smtpUser: string = "";

    public logArichveIntervalInDays: number = 0;

    public useManagementOtpAuth: boolean = false;
    public enableFreeTrailUsers: boolean = false;
    public enableTabletsSupport: boolean = false;
    public enableSigner1ExtraSigningTypes: boolean = false;
    public shouldUseReCaptchaInRegistration: boolean = false;
    public shouldUseSignerAuth: boolean = false;
    public shouldUseSignerAuthDefault: boolean = false;
    public enableShowSSOOnlyInUserUI: boolean = false;
    public enableVisualIdentityFlow: boolean = false;
    public visualIdentityURL: string = "";
    public visualIdentityUser: string = "";
    public visualIdentityPassword: string = "";
    public shouldSendWithOTPByDefault: boolean = false;
    public externalPdfServiceURL: string = "";
    public externalPdfServiceAPIKey: string = "";
    public historyIntegratorServiceURL: string = "";
    public historyIntegratorServiceAPIKey: string = "";
    public useExternalGraphicSignature: boolean = false;
    public externalGraphicSignatureSigner1Url: string = "";
    public externalGraphicSignatureCert: string = "";
    public externalGraphicSignaturePin: string = "";
    public shouldReturnActivationLinkInAPIResponse: boolean = false;
    public recentPasswordsAmount: number;

    public activeDirecrotyConfiguration: ActiveDirecrotyConfiguration;
    constructor() {
        this.activeDirecrotyConfiguration = new ActiveDirecrotyConfiguration();
    }
}
