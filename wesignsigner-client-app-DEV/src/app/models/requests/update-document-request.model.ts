import { FieldRequest } from './fields-request.model';

export class UpdateDocumentRequest {
    public DocumentId: string;
    public Fields : FieldRequest[]=[];
}