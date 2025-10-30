import { Attachment } from './../responses/attachment.model';
import { DocumentData } from './document-data.model';
import { DocumentOperation } from '../../enums/document-operation.enum';
import { DocumentColectionMode } from '../../enums/document-colection-mode.enum';
import { SignatureType } from '../../enums/signature-type.enum';
import { StoreOperationType } from '../../enums/store-operation-type.enum';
import { DocumentCollectionData } from '../responses/document-dollection-data.model';
import { OtpMode } from '../responses/document-collection-count.model';
import { SignatureFieldKind } from 'src/app/enums/signature-field-kind.enum';

export class AppState {
    Token :string;
    OauthDone:boolean;
    OauthNeeded:boolean;
    OauthCode : string;
    OTPCode: string;
    isSubmitted: boolean;
    isLastSigner: boolean;
    signerName : string;
    signerMeans: string;
    signerADName: string;
    signerAuthToken: string;
    companyLogo:string;
    seal: string;
    otpMode: OtpMode;
    areAllOtherSignersSigned: boolean;
    inEIDASSigngingFlow: boolean;
    isContainServerSignature: boolean;    
    signingType : SignatureType;
    selectedSignField: {
        name: string,
        documentId: string,
        type : SignatureType,
        kind:SignatureFieldKind,
    };
    documentsData: DocumentData[];
    operation: DocumentOperation;
    signerNotes: string;
    senderNotes: string;
    senderAppendices : string[];
    documentCollectionData : DocumentCollectionData;    
    attachments : Attachment[] = [];
    serverSignatureCredential : {
        certId : string,
        password : string,
        authToken : string,
    }
    
    isAttachmentshown: boolean;
    OpenAtthmentsFromRemote: boolean;
    OpenAppendicesFromRemote  : boolean;

    OpenNotesFromRemote: boolean;
    storeOperationType : StoreOperationType = StoreOperationType.None;
    currentJumpedField:{
        name:string,
        page:number,
        yLocation:number,
        displayDoc : number
    } = {page:1,yLocation: 0 , name :"", displayDoc:0};
    

    constructor(options?: {
        isSubmitted: boolean,
        selectedSignField: {
            name: string,
            documentId: string,
            type : SignatureType,
            kind : SignatureFieldKind,
        }
    }) {
        if (options) {
            this.isSubmitted = options.isSubmitted;
            this.selectedSignField = {
                name: options.selectedSignField.name,
                documentId: options.selectedSignField.documentId,
                type : options.selectedSignField.type,
                kind : options.selectedSignField.kind,

            };
            
        }
        else {
            this.selectedSignField = {
                name: "",
                documentId: "",
                type : 1,
                kind:0,
            },
            this.operation = DocumentOperation.Close
        }
        this.OauthDone = false;
        this.OauthNeeded = false;
        this.OauthCode ="";
        this.OTPCode = "";
        this.Token = "";
        this.inEIDASSigngingFlow = false;
        this.isAttachmentshown = false;
    }
}