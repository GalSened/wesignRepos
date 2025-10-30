import { Attachment } from './attachment.model';

export class SignerData {
    public name: string;
    public isLastSigner: boolean;
    public areAllOtherSignersSigned: boolean;
    public seal: string;
    public attachments: Attachment[] = [];
    public means: string;
    public adName: string;
    public authToken: string;
    public note: string;
}