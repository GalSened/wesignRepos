import { SignerStatus } from '@models/enums/signer-status.enum';
import { Contact, SendingMethod } from '@models/contacts/contact.model';
import { SignerAuthentication } from './signer-authentication.model';

export class signer {
    id: string;
    name: string;
    status: SignerStatus;
    timeSent: Date;
    timeViewed: Date;
    timeSigned: Date;
    timeRejected: Date;
    sendingMethod: SendingMethod;
    signerNote: string;
    userNote: string;
    attachments: string[]; // TODO - change 
    contact: Contact;
    signerAuthentication: SignerAuthentication;
    //rejectText: string;
}