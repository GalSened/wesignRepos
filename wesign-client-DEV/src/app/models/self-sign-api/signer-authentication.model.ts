export class SignerAuthentication {    
    signer1Credential: Signer1Credential ;
}

export enum AuthenticationType {
    None = 1,
    SmartCard = 2,
    Signer1 = 3,
}

export class Signer1Credential{
    password: string;
    certificateId: string;
    signerToken:string;
}