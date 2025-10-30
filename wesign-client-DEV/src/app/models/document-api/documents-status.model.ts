import { BaseResult } from "@models/base/base-result.model";
import { documentCollections } from "./documentsCollection.model";

export class DocumentsStatus extends BaseResult {
    public documentCollections: documentCollections[];
}