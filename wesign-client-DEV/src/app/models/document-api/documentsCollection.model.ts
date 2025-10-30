import { SignMode } from "@models/enums/sign-mode.enum";
import { signer } from './document-signer.model';
import { DocStatus } from '@models/enums/doc-status.enum';
import { User } from '@models/account/user.model';

export class documentCollections { // TODO - change class name
    documentCollectionId: string;
    name: string;
    documentStatus: DocStatus;
    mode: SignMode;
    documentsIds: string[];
    signers: signer[];
    creationTime: Date;
    signedTime: Date;
    isWillDeletedIn24Hours: boolean;
    user: User;
    shouldSignUsingSigner1AfterDocumentSigningFlow: boolean;
}