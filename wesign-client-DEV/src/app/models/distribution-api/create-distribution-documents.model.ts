import { BaseSigner } from './read-signers-from-file-response.model';

export class CreateDistributionDocuments {
    public name: string = "";
    public templateId: string = "";
    public signers: BaseSigner[] = [];
    public signDocumentWithServerSigning: boolean = false;
}