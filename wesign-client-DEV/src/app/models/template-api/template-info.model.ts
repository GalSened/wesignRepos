import { BaseResult } from "@models/base/base-result.model";

export class TemplateInfo extends BaseResult {
    public templateId: string;
    public images: string[];
    public name: string;
    public userId: string;
    public userName: string;
    public timeCreated: Date;
    public singleLinkUrl: string;
}
