import { SignMode } from '@models/enums/sign-mode.enum';

export class UserSigningLinksResponse {
    public documentCollections: UserSigningLink[] = [];
}

export class UserSigningLink {
    public documentCollectionId: string;
    public name: string;
    public mode: SignMode;
    public creationTime: Date;
    public signingLink: string;
}