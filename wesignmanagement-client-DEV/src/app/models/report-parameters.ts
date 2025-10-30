import { SignatureType } from "./signature-type.enum";

export class ReportParameters {
    public offset: number;
    public limit: number;
    public programUtilizationHistoryKey: string;
    public isProgramUtilizationExpired: boolean;
    public monthsForAvgUse: number;
    public programId: string;
    public docsUsagePercentage: number;
    public companyId: string;
    public groupIds: string[];
    public minDocs: number;
    public minSms: number;
    public isProgramUsed: boolean;
    public userEmail: string;
    public signatureTypes: SignatureType[];
}