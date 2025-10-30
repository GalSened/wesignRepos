import { FLOW_NAMES, FLOW_STEP } from "@models/enums/flow-step.enum";
import { PageField, RadioFieldGroup } from "@models/template-api/page-data-result.model";
import { TemplateInfo } from "@models/template-api/template-info.model";
import { DASHBOARD_VIEWS } from "./actions/app.actions";
import { userType } from '@models/enums/user-type.enum';
import { Signer1Credential, SignerAuthentication } from '@models/self-sign-api/signer-authentication.model';
import { Signer1FileSiging, UploadRequest, UploadRequests } from '@models/template-api/upload-request.model';
import { Signer } from '@models/document-api/signer.model';
import { UserProgram } from '@models/program/user-program.model';
import { DocumentsStatus } from '@models/document-api/documents-status.model';
import { documentCollections } from '@models/document-api/documentsCollection.model';
import { CompanySigner1Details } from '@models/account/user.model';
import { SignatureType } from '@models/enums/signature-type.enum';
import { userConfiguration } from '@models/account/user-configuration.model';

export interface IAppState {
    appState: AppState;
} 

export interface AppState {
    IsLoggedIn: boolean;
    AuthToken:string;
    Token: string;
    RefreshToken: string;
    Language: string;
    IsRtl: boolean;
    FlowName: FLOW_NAMES;
    FlowStep: FLOW_STEP;
    CurrentUserName:string;
    DocumentsNumber: number; // TODO - check if needed
    CurrentDocumentName: string; // TODO - check if needed
    PageFields: PageField[];
    RadioGroupNames: string[];
    SelectedField: { FieldName: string, TemplateId: string };
    SelectedSignField: { FieldName: string, TemplateId: string };
    RadioSelectd: {FieldName:string, TemplateId: string , Group: string },
    DashboardView: DASHBOARD_VIEWS;
    SelectedTemplates: TemplateInfo[];
    HeaderStyles: string;
    SelfSignSignerAuth: Signer1Credential;
    FileUploadRequest: UploadRequest;
    FileUploadRequests: UploadRequests;
    Signers: Signer[];
    EnableFreeTrailUsers : boolean;
    EnableTabletSupport: boolean;
    EnableUseSignerAuth: boolean;
    UseSignerAuthByDefault: boolean;
    SelectedSignerClassId : string;
    currentUserType : userType;
    program: UserProgram;
    userConfiguration: userConfiguration;
    documentsStatus:documentCollections[];
    documentsCollectionId: string;
    IntersetctFiled: PageField;
    lastAction:string;
    signer1FileSiging : Signer1FileSiging;
    companySigner1Details: CompanySigner1Details;
    enableDisplaySignerNameInSignature: boolean;
    enableMeaningOfSignature: boolean;
    shouldSignEidasSignatureFlow: boolean;
    shouldSignUsingSigner1AfterDocumentSigningFlow: boolean;
    enableVisualIdentityFlow : boolean;    
    ShouldSendWithOTPByDefault: boolean;
    defaultSigningType : SignatureType;
    ShouldLoadFields: boolean;
    ShouldFetchInfoOnCurrentUser: boolean;
    enableVideoConferenceFlow : boolean;    
}

export enum AlertLevel {
    NONE = -1,
    SUCCESS = 0,
    ERROR = 1,
}
