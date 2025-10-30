import { DocumentCount } from './document-count.model';
import { DocumentCollectionData } from './document-dollection-data.model';
import { SenderData } from './sender-data.model';
import { SignerData } from './signer-data.model';

export class DocumentCollectionDataResponse {
    public documents: DocumentCount[] = [];
    public totalPagesCount: number;
    public documentCollection: DocumentCollectionData;
    public sender: SenderData;
    public signer: SignerData;
    public otpMode: OtpMode = OtpMode.None;
    public language: Language = Language.en;
    public oauthNeeded: boolean;
}

export enum OtpMode {
    None = 0,
    CodeRequired = 1,
    IdentificationRequired = 2,
    CodeAndIdentificationRequired = 3
}

export enum Language {
    en = 1,
    he = 2
}