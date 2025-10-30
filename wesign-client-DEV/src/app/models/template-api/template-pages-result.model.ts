import { BaseResult } from "@models/base/base-result.model";

export class TemplatePagesResult extends BaseResult {
    public templateId: string;
    public templateName: string;
    public pagesCount: number;
}
