import { PDFFields } from '@models/template-api/update-template-request.model';
import { DocumentOperations } from '@models/enums/document-operations.enum';
import { SignerAuthentication } from './signer-authentication.model';

export class UpdateSelfSignRequest {
    public documentCollectionId: string;
    public documentId: string;
    public fields: PDFFields;
    public operation: DocumentOperations;
    public name: string;
    public useForAllFields: boolean;
    public signerAuthentication : SignerAuthentication = new SignerAuthentication();

    constructor() {
        this.fields = new PDFFields();
    }
}