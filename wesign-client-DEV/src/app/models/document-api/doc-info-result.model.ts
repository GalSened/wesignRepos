import { BaseResult } from "@models/base/base-result.model";

export enum DocMode {
    SelfSign = 1,
    Workflow = 2,
    Online = 3,
}

export class DocInfoResult extends BaseResult {
    public documentId: string;
    public documentName: string;
    public pagesCount: number;
    // public DocumentName: string;
    // public Mode: DocMode;
    // public TemplateId: number;
}
