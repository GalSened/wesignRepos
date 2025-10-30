import { SignMode } from "@models/enums/sign-mode.enum";
import { SendingMethod } from '@models/contacts/contact.model';
import { OtpMode } from '@models/enums/otp-mode.enum';
import { AuthMode } from '@models/enums/auth-mode.enum';

export class DocumentCollectionCreateRequest {
    public documentMode: SignMode;
    public documentName: string;
    public templates: string[];
    public senderNote: string;
    public signers: DocumentSigner[];
    public readOnlyFields: SignerField[] = [];
    public redirectUrl: string;
    public senderAppendices: Appendix[] = [];
    public SharedAppendices: SharedAppendix[] = [];
    public shouldSignUsingSigner1AfterDocumentSigningFlow: boolean;
    public shouldEnableMeaningOfSignature: boolean;
}

export class DocumentSigner {
    public contactId: string;
    public sendingMethod: SendingMethod;
    public contactMeans: string;
    public contactName: string;
    public sealId: string;
    public signerFields: SignerField[];
    public signerAttachments: Attachment[];
    public linkExpirationInHours: number;
    public senderNote: string;
    public senderAppendices: Appendix[];
    public otpIdentification: string;
    public otpMode = OtpMode.None;
    public authenticationMode = AuthMode.None;
    public phoneExtension: string;
}

export class SharedAppendix {
    public signerIndexes: number[];
    public appendix: Appendix
}

export class Attachment {
    public name: string;
    public isMandatory: boolean;
}

export class SignerField {
    public templateId: string;
    public fieldName: string;
    public fieldValue: string;
}

export class Notification {
    public shouldSend: boolean;
    public shouldSendSignedDocument: boolean;
}

export class Appendix {
    public name: string;
    public base64file: string;
}