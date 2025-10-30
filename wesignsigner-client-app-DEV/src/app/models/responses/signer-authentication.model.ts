
import { Signer1Credential } from './signer1-credential.model';
import { OtpDetails } from './otp-details.model';

export class SignerAuthentication {

    public signer1Credential: Signer1Credential;
    public otpDetails: OtpDetails;

    // public authenticationType :AuthenticationType;
    // public password : string;
    // public certificateId : string;
}

export enum AuthenticationType {
    None = 1,
    SmartCard = 2,
    Signer1 = 3,
}