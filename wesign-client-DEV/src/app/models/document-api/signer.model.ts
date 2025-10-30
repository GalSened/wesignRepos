import { SendingMethod } from '@models/contacts/contact.model';

export class Signer {
    public ClassId: string; // css class + id for uniqueness
    public FullName: string;
    public DeliveryMeans: string;
    public DeliveryExtention = "+972";
    public DeliveryMethod: SendingMethod;    
    public ContactId: string;
}