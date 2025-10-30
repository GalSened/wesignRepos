import { DocStatus } from '@models/enums/doc-status.enum';
import { ReportFrequency } from '@models/enums/report-frequency';

export class ReportRequest {
    public from: Date;
    public to: Date;
    public groupIds: string[];
    public documentStatuses: DocStatus[];
    public includeDistributionDocs: boolean;
    public offset: number;
    public limit: number;
    public isCsv: boolean;
    public frequency: ReportFrequency;

    constructor() {
        this.groupIds = [];
        this.documentStatuses = [];
        this.from = this.getOneYearBefore();
        this.to = new Date();
        this.includeDistributionDocs = false;
        this.offset = 0;
        this.limit = 20;
        this.isCsv = false;
        this.frequency = null;
    }

    private getOneYearBefore(): Date {
        const now = new Date();
        return new Date(now.getFullYear() - 1, now.getMonth(), now.getDate(), now.getHours(), now.getMinutes(), now.getSeconds());
    }
}