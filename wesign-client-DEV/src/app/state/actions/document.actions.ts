import { Signer } from '@models/document-api/signer.model';
import { Signer1FileSiging, UploadRequest, UploadRequests } from '@models/template-api/upload-request.model';
import { Action } from '@ngrx/store';

export const SET_FILE_UPLOAD_REQUEST = "[File] Set file upload request";
export class SetFileUploadRequestAction implements Action {
    public readonly type = SET_FILE_UPLOAD_REQUEST;
    constructor(readonly payload: { fileUploadRequest: UploadRequest }) { }
}

export const SET_MULTIPLE_FILES_UPLOAD_REQUEST = "[File] Set multiple files upload request";
export class SetMultipleFilesUploadRequestAction implements Action {
    public readonly type = SET_MULTIPLE_FILES_UPLOAD_REQUEST;
    constructor(readonly payload: { fileUploadRequests: UploadRequests }) { }
}

export const CLEAR_FILE_UPLOAD_REQUEST = "[File] Clear file upload request";
export class ClearFileUploadRequestAction implements Action {
    public readonly type = CLEAR_FILE_UPLOAD_REQUEST;
    constructor(readonly payload: {}) {}    
}

export const SET_SIGNERS = "[File] Set signers";
export class SetSigners implements Action {
    public readonly type = SET_SIGNERS;
    constructor(readonly payload: { Signers: Signer[] }) { }
}

export const SET_DOCUMENTNAME = "[File] Set document name";
export class SetDocumentName implements Action {
    public readonly type = SET_DOCUMENTNAME;
    constructor(readonly payload: { CurrentDocumentName: string }) { }
}



export const SET_CERTIICATE_FILE_UPLOAD= "[File] Set Cert File Upload";
export class SetCertificateFileUploadRequestAction implements Action {
    public readonly type = SET_CERTIICATE_FILE_UPLOAD;
    constructor(readonly payload: { signer1FileSiging: Signer1FileSiging }) { }
}

export type ACTIONS = ClearFileUploadRequestAction | SetFileUploadRequestAction | SetSigners | SetDocumentName|SetCertificateFileUploadRequestAction;