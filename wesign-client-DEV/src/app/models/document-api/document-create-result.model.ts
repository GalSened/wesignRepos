export class DocumentCreateResult{
    public documentCollectionId: string;
    public signerLinks : SignerLink[];
}

export class SignerLink{
    public signerId: string;
    public link: string;
}