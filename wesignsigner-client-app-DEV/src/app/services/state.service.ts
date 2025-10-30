import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { DocumentColectionMode } from '../enums/document-colection-mode.enum';
import { DocumentOperation } from '../enums/document-operation.enum';
import { SignatureType } from '../enums/signature-type.enum';
import { StoreOperationType } from '../enums/store-operation-type.enum';
import { WeSignFieldType } from '../enums/we-sign-field-type.enum';
import { signatureField } from '../models/pdffields/field.model';
import { FieldRequest } from '../models/requests/fields-request.model';
import { Attachment } from '../models/responses/attachment.model';
import { OtpMode } from '../models/responses/document-collection-count.model';
import { DocumentCollectionData } from '../models/responses/document-dollection-data.model';
import { AppState } from '../models/state/app-state.model';
import { DocumentData } from '../models/state/document-data.model';
import { LiveEventsService } from './live-events.service';
import { SignatureFieldKind } from '../enums/signature-field-kind.enum';

@Injectable({
    providedIn: 'root'
})
export class StateService {

    private state = new AppState();
    private _state$ = new BehaviorSubject(this.state);
    private _signature$ = new Subject<signatureField>();

    constructor(private liveEventsService: LiveEventsService) { }

    get state$(): Observable<AppState> {
        return this._state$.asObservable();
      
    } 

    private setSignatureImage(documentId, signatureField) {

        let fieldData = new FieldRequest();
                            fieldData.fieldName = signatureField.name;
                            fieldData.fieldType = WeSignFieldType.SignatureField;
                            fieldData.fieldValue = signatureField.image;
                            this.setFieldData(documentId, fieldData);
                            this._signature$.next(signatureField); 
    }
   
    private setState(state: AppState): void {
        this._state$.next(state);
    }

    get singature$(): Observable<signatureField> {
        return this._signature$.asObservable();
    }

    private _showLoader: boolean;

    public get showLoader(): boolean {
        return this._showLoader;
    }
    public set showLoader(v: boolean) {
        this._showLoader = v;
    }

    private _showServerCredentialForm: boolean;

    public get showServerCredentialForm(): boolean {
        return this._showServerCredentialForm;
    }
    public set showServerCredentialForm(v: boolean) {
        this._showServerCredentialForm = v;
    }

    private _showDeclineForm: boolean;

    public get showDeclineForm(): boolean {
        return this._showDeclineForm;
    }
    public set showDeclineForm(v: boolean) {
        this._showDeclineForm = v;
    }


    setSignerSeal(seal :string ){
        this.state.storeOperationType =  StoreOperationType.SetSignerSeal;
        this.state.seal = seal;
        this.setState(this.state);
    }
    
    
    setSubmittion(isSubmitted: boolean) {
        this.state.storeOperationType =  StoreOperationType.SetSubmittion;
        this.state.isSubmitted = isSubmitted;
        this.setState(this.state);
    }

    setSigningType(signingType: SignatureType) {
        this.state.storeOperationType =  StoreOperationType.SetSigningType;
        this.state.signingType = signingType;
        this.setState(this.state);
    }

    setDocumentCollectionData(data: DocumentCollectionData) {
        this.state.storeOperationType =  StoreOperationType.SetDocumentCollectionData;
        this.state.documentCollectionData = data;
        this.setState(this.state);
    }

    setOtpMode(otpMode: OtpMode) {
        this.state.storeOperationType =  StoreOperationType.SetOtpRequired;
        this.state.otpMode = otpMode;
        this.setState(this.state);
    }

    setCompanyLogo(companyLogo: string) {
        this.state.storeOperationType =  StoreOperationType.SetCompanyLogo;
        this.state.companyLogo = companyLogo;
        this.setState(this.state);
    }

    setLastSigner(isLastSigner: boolean) {
        this.state.storeOperationType =  StoreOperationType.SetLastSigner;
        this.state.isLastSigner = isLastSigner;
        this.setState(this.state);
    }

    setSignerName(name: string) {
        this.state.storeOperationType =  StoreOperationType.SetSignerName;
        this.state.signerName = name;
        this.setState(this.state);
    }

    setSignerMeans(means: string) {
        this.state.storeOperationType =  StoreOperationType.SetSignerMeans;
        this.state.signerMeans = means;
        this.setState(this.state);
    }

    setSignerADName(ADName : string, authToken : string) {
        this.state.storeOperationType =  StoreOperationType.SetSignerADName;
        this.state.signerADName = ADName;
        this.state.signerAuthToken = authToken;
        this.setState(this.state);
    }

    setAreAllOtherSignersSigned(areAllOtherSignersSigned: boolean) {
        this.state.storeOperationType =  StoreOperationType.SetAreAllOtherSignersSigned;
        this.state.areAllOtherSignersSigned = areAllOtherSignersSigned;
        this.setState(this.state);
    }

    setIsContainServerSignature(isContainServerSignature: boolean) {
        this.state.storeOperationType =  StoreOperationType.SetIsContainServerSignature;
        this.state.isContainServerSignature = isContainServerSignature;
        this.setState(this.state);
    }

    setSetEIDASSigningFlow(inEIDASSigngingFlow: boolean) {
        this.state.storeOperationType =  StoreOperationType.SetEIDASSigngingFlow;
        this.state.inEIDASSigngingFlow = inEIDASSigngingFlow;
        this.setState(this.state);
    }
    setAttachments(attachments: Attachment[]) {
        this.state.storeOperationType =  StoreOperationType.SetAttachments;
        this.state.attachments = attachments;
        this.setState(this.state);
    }

    setselectedSignField(selectedSignField: { name: string, documentId: string, type: SignatureType, kind: SignatureFieldKind }) {
        this.state.storeOperationType =  StoreOperationType.SetselectedSignField;
        this.state.selectedSignField.name = selectedSignField.name;
        this.state.selectedSignField.documentId = selectedSignField.documentId;
        this.state.selectedSignField.type = selectedSignField.type;
        this.state.selectedSignField.kind = selectedSignField.kind;
       
        this.setState(this.state);

        let metaViewPort = document.querySelector('meta[name="viewport"]'); 
        if(metaViewPort)   
        {
            if(selectedSignField.name == "")    
            {
                metaViewPort.setAttribute('content', "width=device-width, initial-scale=1.0, minimum-scale=1.0"); 
            }
            else
            {
                metaViewPort.setAttribute('content', "width=device-width, initial-scale=1.0, maximum-scale=1.0"); 
            }
        }

    }

    setDocumentsData(documentsDataState: DocumentData[]) {
        this.state.storeOperationType =  StoreOperationType.SetDocumentsData;
        this.state.documentsData = documentsDataState;
        this.setState(this.state);
    }

    setAttachmentData(attachmentId: string, attachment: Attachment) {
        this.state.storeOperationType =  StoreOperationType.SetAttachmentData;
        let attachmentData = this.state.attachments.find(
            x => x.id == attachmentId
        );
        attachmentData = attachment;
        this.setState(this.state);
    }

  
    setFieldData(documentId: string, field: FieldRequest) {
        this.state.storeOperationType =  StoreOperationType.SetFieldData;
        if (this.state.documentsData == null) {
            return;
        }
        let documentData = this.state.documentsData.find(
            x => x.documentId == documentId
        );
        if (field.fieldType == WeSignFieldType.SignatureField) {
            let sig = documentData.pdfFields.signatureFields.find(
                x => x.name == field.fieldName
            );
            sig.image = field.fieldValue;
            if (sig.image){
                // Update signature image
                this._signature$.next(sig); 
            }
        }
        if (field.fieldType == WeSignFieldType.CheckBoxField) {
            //console.log(field.fieldValue);
            let textArray = documentData.pdfFields.checkBoxFields.filter(
                x => x.description == field.fieldDescription
            );
            textArray.forEach(text => text.isChecked = field.fieldValue.toLowerCase() == "true"? true: false);
            //console.log(this.state.documentsData);
        }
        if (field.fieldType == WeSignFieldType.TextField) {
            //console.log(field.fieldValue);
            let textArray = documentData.pdfFields.textFields.filter(
                x => x.description == field.fieldDescription
            );
            textArray.forEach(text => text.value = field.fieldValue);
            //console.log(this.state.documentsData);
        }
        if (field.fieldType == WeSignFieldType.ChoiceField) {
            let choice = documentData.pdfFields.choiceFields.find(
                x => x.name == field.fieldName
            );
            choice.selectedOption = field.fieldValue;
        }
        if (field.fieldType == WeSignFieldType.RadioGroupField) {
            let radioGroup = documentData.pdfFields.radioGroupFields.find(
                x => x.name == field.fieldName
            );
            radioGroup.selectedRadioName = field.fieldValue;
        }

        
        this.setState(this.state);
        if (this.state.documentCollectionData && this.state.documentCollectionData.mode == DocumentColectionMode.Online) {
            this.liveEventsService.init();
            this.liveEventsService.setFieldData(this.state.Token, documentId, field);
        }
    }

    setOperation(operation: DocumentOperation) {
        this.state.storeOperationType =  StoreOperationType.SetOperation;
        this.state.operation = operation;
        this.setState(this.state);
    }

    setSignerNotes(newSignerNotes: string) {
        this.state.storeOperationType =  StoreOperationType.SetSenderNotes;
        this.state.signerNotes = newSignerNotes;
        this.setState(this.state);
    }

    setSenderNotes(senderNotes: string) {
        this.state.storeOperationType =  StoreOperationType.SetSenderNotes;
        this.state.senderNotes = senderNotes;
        this.setState(this.state);
    }

    setSenderAppendices(senderAppendices: string[]) {
        this.state.storeOperationType =  StoreOperationType.SetSenderAppendices;
        this.state.senderAppendices = senderAppendices;
        this.setState(this.state);
    }


    setServerSignatureCredential(serverSignatureCredential: { certId: string, password: string , authToken: string}) {
        this.state.storeOperationType =  StoreOperationType.SetServerSignatureCredential;

        this.state.serverSignatureCredential = {
            certId: serverSignatureCredential.certId,
            password: serverSignatureCredential.password,
            authToken: serverSignatureCredential.authToken
        };
        this.setState(this.state);
    }
    setAttachmentshown(){
        this.state.storeOperationType =  StoreOperationType.SetAttachmentshown;
        this.state.isAttachmentshown = true;
        this.setState(this.state);
    }
    openAtthmentsFromRemote(show : boolean){
        this.state.storeOperationType =  StoreOperationType.OpenAtthmentsFromRemote;
        this.state.OpenAtthmentsFromRemote = show;
        if(show){
            this.state.isAttachmentshown = true;
        }
        
        this.setState(this.state);
    }

    openNotesFromRemote(show : boolean){
        this.state.storeOperationType =  StoreOperationType.OpenNotesFromRemote;
        this.state.OpenNotesFromRemote = show;
        this.setState(this.state);
    }


    closeSaveSignatureForFutureUse(){
        this.state.storeOperationType =  StoreOperationType.CloseSaveSignatureForFutureUse;        
        this.setState(this.state); 
    }

    clearStoreOperationType(){
        this.state.storeOperationType =  StoreOperationType.None;        
        this.setState(this.state); 
    }

    openAppendicesFromRemote(show : boolean){
        this.state.storeOperationType =  StoreOperationType.OpenAppendicesFromRemote;
        this.state.OpenAppendicesFromRemote = show;
        this.setState(this.state);
    }


    SetCurrentJumpedField(currentJumpedField)
    {
         this.state.storeOperationType =  StoreOperationType.SetCurrentJumpedField;
         this.state.currentJumpedField = currentJumpedField;
         this.setState(this.state);
    }
    

    SetOauthNeeded(){
        this.state.storeOperationType =  StoreOperationType.SetOauthNeeded;
         this.state.OauthNeeded = true;
         this.setState(this.state);
    }
   
    SetOauthDone(code){
        this.state.storeOperationType =  StoreOperationType.SetOauthDone;
        this.state.OauthNeeded = false;
         this.state.OauthDone = true;
         this.state.OauthCode = code;
         this.setState(this.state);
    }

    SetOTPCode(code){
        this.state.storeOperationType =  StoreOperationType.SetOTPCode;
        this.state.OTPCode = code;         
         this.setState(this.state);
    }
    
    SetDocuementToken(token)
    {
        this.state.storeOperationType =  StoreOperationType.SetDocToken;
        this.state.Token = token;         
         this.setState(this.state);
    }

}
