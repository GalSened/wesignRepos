import { BaseSigner } from '@models/distribution-api/read-signers-from-file-response.model';
import validator from "validator";

export enum SendingMethod {
    SMS = 1,
    EMAIL = 2,
    TABLET = 3
}

export class Seal {
    public id: string = "";
    public name: string = "";
    public base64Image: string = "";
}

export class Contact {
    public id: string;
    public name: string = "";
    public email: string = "";
    public phone: string = "";
    public phoneExtension: string = "";
    public defaultSendingMethod: SendingMethod = 2;
    public seals: Seal[] = [];
    public searchTag: string = "";

    public toString() {
        const res = [];
        if (this.name)
            res.push(this.name);

        if (this.email)
            res.push(this.email);

        if (this.phone)
            res.push(this.phone);


        const str = res.join(" ");
        return str;
    }
    constructor(){}
    

}

export class OtpContact extends Contact{
    public shouldSendOTP : boolean = false;

    constructor(contact : Contact){
        super()
        if(contact){
            this.id = contact.id;
            this.name = contact.name;
            this.email = contact.email;
            this.phone = contact.phone;
            this.phoneExtension = contact.phoneExtension;
            this.defaultSendingMethod = contact.defaultSendingMethod;
            this.seals = contact.seals;
            this.searchTag = contact.searchTag;
        }
    }
}


