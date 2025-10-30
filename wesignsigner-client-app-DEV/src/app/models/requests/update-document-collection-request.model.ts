import { DocumentOperation } from "../../enums/document-operation.enum";
import { Attachment } from "../responses/attachment.model";
import { SignerAuthentication } from "../responses/signer-authentication.model";
import { UpdateDocumentRequest } from "./update-document-request.model";



export class UpdateDocumentCollectionRequest {

    public SignerNote :string;
    public SignerAttachments : Attachment[]=[];
    public SignerAuthentication : SignerAuthentication;
    public Documents: UpdateDocumentRequest[]=[];
    public Operation:DocumentOperation;
    public UseForAllFields: boolean;

}
