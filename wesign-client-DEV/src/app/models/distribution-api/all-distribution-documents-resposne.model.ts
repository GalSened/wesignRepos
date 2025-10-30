import { signer } from '@models/document-api/document-signer.model';
import { DocStatus } from '@models/enums/doc-status.enum';

export class AllDistributionDocumentsResposne {
    public documentCollections: DistributionDocument[] = [];
}

export class DistributionDocument {
    public documentCollectionId: string = "";
    public distributionId: string = "";
    public name: string = "";
    public creationTime: Date;
    public isWillDeletedIn24Hours: boolean = false;
    public user: string = "";
}

export class AllDistributionDocumentsExpandedResposne {
    public totalPending: number = 0;
    public totalSigned: number = 0;
    public totalServerSigned: number = 0;
    public totalDeclined: number = 0;
    public totalFailed: number = 0;
    public totalCreatedButNotSent: number = 0;
    public totalViewed: number = 0;
    public documentCollections: DistributionDocumentExpanded[] = [];
    public shouldSignUsingSigner1AfterDocumentSigningFlow: boolean = false;

}

export class DistributionDocumentExpanded extends DistributionDocument {
    public documentStatus: DocStatus;
    public documentsIds: string[];
    public signer: signer;
}