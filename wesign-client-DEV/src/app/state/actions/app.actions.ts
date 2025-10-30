import { FLOW_NAMES, FLOW_STEP } from "@models/enums/flow-step.enum";
import { PageField, SignatureField, TextField } from "@models/template-api/page-data-result.model";
import { Action } from "@ngrx/store";
import { userType } from '@models/enums/user-type.enum';
import { Signer1Credential, SignerAuthentication } from '@models/self-sign-api/signer-authentication.model';
import { CompanySigner1Details, User } from '@models/account/user.model';
import { UserProgram } from '@models/program/user-program.model';
import { DocumentsStatus } from '@models/document-api/documents-status.model';
import { documentCollections } from '@models/document-api/documentsCollection.model';
import { SignatureType } from '@models/enums/signature-type.enum';
import { userConfiguration } from '@models/account/user-configuration.model';

export const ENABLE_TABLETS : string = "[Tablets] Support"


export class TabletsSupportAction implements Action {
    public readonly type = ENABLE_TABLETS;
    constructor (readonly payload: {EnableTabletSupport : boolean}){}
}



export const ENABLE_SIGNER_AUTH : string = "[Signer] Auth Support"
export class SignerAuthAction implements Action {
    public readonly type = ENABLE_SIGNER_AUTH;
    public payload: {};
}

export const ENABLE_SIGNER_AUTH_DEFAULT: string = "[Signer] Default Auth Support Choice"
export class SignerAuthDefaultAction implements Action {
    public readonly type = ENABLE_SIGNER_AUTH_DEFAULT;
    public payload: {};
}



export const LOGIN: string = "[Login] Login";

export class LoginAction implements Action {
    public readonly type = LOGIN;
    constructor(readonly payload: { Token: string, RefreshToken: string, AuthToken : string }) {
    }
}

export const LOGOUT: string = "[Login] Logout";

export class LogoutAction implements Action {
    public readonly type = LOGOUT;
    public payload: {};
}

export const LANGUAGE_STATE: string = "[Language] Set Language State";

export class SetLangAction implements Action {
    public readonly type = LANGUAGE_STATE;
    constructor(readonly payload: { Language: string, IsRtl: boolean }) {
    }
}

export const FLOW_STATE = "[Flow] Set Flow State";

export class SetFlowAction implements Action {
    public readonly type = FLOW_STATE;
    constructor(readonly payload: {
        FlowName: FLOW_NAMES,
        FlowStep: FLOW_STEP,
    }) {

    }
}

export const DOCUMENTS_STATE = "[Documents] Set document number";

export class SetDocumentsAction implements Action {

    public readonly type = DOCUMENTS_STATE;

    constructor(readonly payload: { DocumentsNumber: number }) { }
}

export type DASHBOARD_VIEWS = "documents" | "templates" | "contacts";

export const DASHBOARD_VIEW = "[Documents] Set dashboard view";
export class SetDashboardViewAction implements Action {
    public readonly type = DASHBOARD_VIEW;
    constructor(readonly payload: { DashboardView: DASHBOARD_VIEWS }) { }
}

export const SIGNER_AUTH_STATE = "[Auth] Set signer authentication";

export class SetSignerAuthAction implements Action {

    public readonly type = SIGNER_AUTH_STATE;

    constructor(readonly payload: { signerAuth: Signer1Credential }) { }
}

export const SET_CURRENT_USER_DETAILS = "[Details] Set current user details";

export class SetCurrentUserDetailesAction implements Action {

    public readonly type = SET_CURRENT_USER_DETAILS;

    constructor(readonly payload: { userName: string, userType : userType}) { }
}


export const SET_CURRENT_USER_PROGRAM = "[Details] Set current user program";

export class SetCurrentUserProgram implements Action {

    public readonly type = SET_CURRENT_USER_PROGRAM;

    constructor(readonly payload: { program: UserProgram}) { }
}

export const SET_CURRENT_USER_CONFIGURATION = "[Details] Set current user configuration";

export class SetCurrentUserConfiguration implements Action {

    public readonly type = SET_CURRENT_USER_CONFIGURATION;

    constructor(readonly payload: { userConfiguration: userConfiguration}) { }
}

export const SET_COMPANY_SIGNER1_DETAILS = "[Details] Set company signer1 details";

export class SetCompanySigner1Details implements Action {

    public readonly type = SET_COMPANY_SIGNER1_DETAILS;

    constructor(readonly payload: { companySigner1Details: CompanySigner1Details}) { }
}

export const SET_DISPLAY_SIGNER_NAME_IN_SIGNATURE = "[Details] Set display signer name in signature";

export class SetDisplaySignerNameInSignature implements Action {
    
    public readonly type = SET_DISPLAY_SIGNER_NAME_IN_SIGNATURE;
    
    constructor(readonly payload: { enableDisplaySignerNameInSignature: boolean}) {};
}

export const SET_DISPLAY_MEANING_OF_SIGNATURE = "[Details] Set display meaning of signature";

export class SetDisplayMeaningOfSignature implements Action {
    
    public readonly type = SET_DISPLAY_MEANING_OF_SIGNATURE;
    
    constructor(readonly payload: { enableMeaningOfSignature: boolean}) {};
}

export const SET_ENABLE_SIGN_EIDAS_SIGNATURE_FLOW = "[Details] Set enable sign eidas signature flow";

export class SetEnableSignEidasSignatureFlow  implements Action {
    
    public readonly type = SET_ENABLE_SIGN_EIDAS_SIGNATURE_FLOW;
    
    constructor(readonly payload: { enableSignEidasSignatureFlow: boolean}) {};
}

export const SET_SHOULD_SIGN_USING_SIGNER1_AFTER_DOCUMMENT_SIGNING_FLOW = "[Details] Set should sign using signer1 after document signing flow";

export class SetShouldSignUsingSigner1AfterDocumentSigningFlow implements Action {

    public readonly type = SET_SHOULD_SIGN_USING_SIGNER1_AFTER_DOCUMMENT_SIGNING_FLOW;

    constructor(readonly payload: { shouldSignUsingSigner1AfterDocumentSigningFlow: boolean}) { }
}

export const SET_DOCUMENT_LIST_STATUS = "[Details] Set Document list status";

export class SetDocumentListStatus implements Action {

    public readonly type = SET_DOCUMENT_LIST_STATUS;

    constructor(readonly payload: { documentsStatus: documentCollections[]}) { }
}

export const SET_DOCUMENT_COLLECTION_TO_VIEW = "[Details] Set Document collection for Document View";

export class  SetSelectedDocumentCollectionToView implements Action {

    public readonly type = SET_DOCUMENT_COLLECTION_TO_VIEW;

    constructor(readonly payload: { documentsCollectionId: string}) { }
}


export const SET_SIGNER_AUTH_DEFAULT = "[Details] Set Default Auth for Signer";

export class  SetSignerAuthDefault implements Action {

    public readonly type = SET_SIGNER_AUTH_DEFAULT;

    constructor(readonly payload: { enableVisualIdentityFlow:boolean,  shouldSendWithOTPByDefault: boolean, 
        defaultSigningType : SignatureType, enableVideoConferenceFlow : boolean}) { }
}

export const FETCH_CURRENT_USER_INFO = "[Details] fetch current user";

export class  FetchCurrentUserInfo implements Action {

    public readonly type = FETCH_CURRENT_USER_INFO;

    constructor(readonly payload: { needToFetch:boolean}) { }
}








export type ACTIONS = TabletsSupportAction | LoginAction | LogoutAction | SetLangAction | SetFlowAction | SetDocumentsAction | SetSignerAuthAction | SetCurrentUserDetailesAction | 
SetCurrentUserProgram | SetDocumentListStatus | SetSelectedDocumentCollectionToView | SetCompanySigner1Details | SetShouldSignUsingSigner1AfterDocumentSigningFlow | SetSignerAuthDefault | 
FetchCurrentUserInfo; 