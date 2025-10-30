import { DocumentResendType } from '@models/enums/document-operations.enum';

export class DocumentResend {
    public type: DocumentResendType;
    public id: string;
    public contactId: string;
    public signerId:string;
    public documentName: string;
};
