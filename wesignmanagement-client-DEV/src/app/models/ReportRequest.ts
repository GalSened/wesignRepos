import { SignatureType } from "./signature-type.enum";

export class ReportRequest {
    private readonly EMPTY_GUID = "00000000-0000-0000-0000-000000000000";
    public userEmail: string = null;
    public isExpired: boolean | null = null;
    public isProgramUsed: boolean | null = null;
    public monthsForAvgUse: number;
    public docUsagePercentage: number;
    public minDocs: number;
    public minSMS: number;
    public from: Date;
    public to: Date;
    public companyId: string;
    public programId: string;
    public groupIds: string[];
    public offset: number;
    public limit: number;
    public isCSV: boolean;
    public signatureTypes: SignatureType[];

    constructor(months: number = 3) {
        this.monthsForAvgUse = 3;
        this.docUsagePercentage = 0;
        this.minDocs = 0;
        this.minSMS = 0;
        this.monthsForAvgUse = months;
        this.companyId = null;
        this.programId = null;
        this.groupIds = [];
        this.offset = 0;
        this.limit = 20;
        this.isCSV = false;
        this.signatureTypes = [];
    }
}