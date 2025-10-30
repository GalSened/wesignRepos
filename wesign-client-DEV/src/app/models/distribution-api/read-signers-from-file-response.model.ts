import { Contact, OtpContact, SendingMethod } from '@models/contacts/contact.model';

export class ReadSignersFromFileResponse {
    public signers: BaseSigner[] = [];
}

export class BaseSigner {
    public signerMeans: string = "";
    public fullName: string = "";
    public phoneExtension : string = "";
    public signerSecondaryMeans : string = "";
    public shouldSendOTP : boolean = false;
    public fields: FieldNameToValuePair[] = [];

    constructor(contact : OtpContact){
        this.fullName = contact.name;
        if(contact.defaultSendingMethod == SendingMethod.EMAIL)
            {
                this.signerMeans =   contact.email;
                this.signerSecondaryMeans =   contact.phone;
            }
            else
            {
                this.signerMeans =   contact.phone;
                this.signerSecondaryMeans =   contact.email;
            }
        
        this.shouldSendOTP = contact.shouldSendOTP;
        this.phoneExtension = contact.phoneExtension;
        }

}

export class FieldNameToValuePair {
    public fieldName: string = "";
    public fieldValue: string = "";
}