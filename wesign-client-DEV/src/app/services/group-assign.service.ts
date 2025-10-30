import { Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject, Subject, Observable } from 'rxjs';
import { Contact } from '@models/contacts/contact.model';
import { AssignedField, CheckBoxField, ChoiceField, PageField, RadioField, RadioFieldGroup, SignatureField, TextField } from '@models/template-api/page-data-result.model';
import { template } from '@angular-devkit/core';

@Injectable()//{ providedIn: 'root' }
export class GroupAssignService implements OnDestroy {
    

    private fieldsMap = new Map<string, PageField[]>(); // key(string) = signerClassId

    private _orderedWorkflow: boolean = true;
   

    public set isOrderedWorkflow(owf: boolean) {
        this._orderedWorkflow = owf;
    }
    public get isOrderedWorkflow() {
        return this._orderedWorkflow;
    }

    private _useMeaningOfSignature: boolean = false;
    public set useMeaningOfSignature(owf: boolean) {
        this._useMeaningOfSignature = owf;
    }
    public get useMeaningOfSignature() {
        return this._useMeaningOfSignature;
    }

    private _documentNotes: string = "";
    public set documentNotes(text: string) {
        this._documentNotes = text;
    }
    public get documentNotes() {
        return this._documentNotes;
    }


    public signersNotes: Map<string, string> = new Map<string, string>();
    public addSignerNotes(signerId: string, signerNotes: string) {
        this.signersNotes.set(signerId, signerNotes);
    }
    public getSignerNotes(signerId: string) {
        return this.signersNotes.get(signerId);
    }

    public SignerOtpIdentifications: Map<string, string> = new Map<string, string>();
    public addSignerOtpIdentifications(signerId: string, signerNotes: string) {
        this.SignerOtpIdentifications.set(signerId, signerNotes);
    }
    public getSignerOtpIdentifications(signerId: string) {
        return this.SignerOtpIdentifications.get(signerId);
    }

    private groupSignBS = new Subject();
    public groupSignObserv = this.groupSignBS.asObservable();

    private groupFieldsBS = new BehaviorSubject(this.fieldsMap);
    public groupFieldsObserv = this.groupFieldsBS.asObservable();

    private showEditModalBS = new Subject<boolean>();
    public showEditModalObserv = this.showEditModalBS.asObservable();

    constructor() { }

    public updateGroupSignAction(type: boolean) {
        this.groupSignBS.next(type);
    }

    public sendFieldsMap(): void {
        this.groupFieldsBS.next(this.fieldsMap);
    }

    public toggleEditModal(status: boolean) {
        this.showEditModalBS.next(status);
    }

    public updateFieldsMap(fm: Map<string, PageField[]>) {
        this.fieldsMap = fm;
        this.sendFieldsMap();
    }

    public addFieldFromStorage(assignedField: PageField, isFirst: boolean){
            // const _key = assignedField.contact.id + "~" + assignedField.contact.name;
            const _key = assignedField.signerId;
            const f: PageField[] = this.fieldsMap.get(_key) || [];

            const index = f.findIndex(a => a === assignedField);
            if (isFirst){
                this.fieldsMap.clear();
                f.forEach(element => {
                    f.pop();
                });
            }
        
            // if not exist
            if (index < 0){
                f.push(assignedField);
                let x: PageField[] = this.fieldsMap.get(undefined) || [];
                let otherFields = x.filter(x=>x.name!=assignedField.name);
                this.fieldsMap.set(undefined, otherFields);

               
            }
            else
                f[index] = assignedField;
    
            
            this.fieldsMap.set(_key, this.removeDuplication(f));
    
            this.fieldsMap.forEach((list,key, _) =>{
                if(key && key != _key )
                {
                   let item =  list.filter( x=>x.name!=assignedField.name);
                   this.fieldsMap.set(key, item);
                }
            });
        
            this.sendFieldsMap();
    }

    public addField(assignedField: PageField) {
        // const _key = assignedField.contact.id + "~" + assignedField.contact.name;
        const _key = assignedField.signerId;
        const f: PageField[] = this.fieldsMap.get(_key) || [];

        const index = f.findIndex(a => a === assignedField);
    
        // if not exist
        if (index < 0){
            f.push(assignedField);
            let x: PageField[] = this.fieldsMap.get(undefined) || [];
            let otherFields = x.filter(x=>x.name!=assignedField.name);
            this.fieldsMap.set(undefined, otherFields);
        }
        else
            f[index] = assignedField;

        this.fieldsMap.set(_key, f);

        this.fieldsMap.forEach((list,key, _) =>{
            if(key && key != _key )
            {
               let item =  list.filter( x=>x.name!=assignedField.name);
               this.fieldsMap.set(key, item);

            }
        });
    
        this.sendFieldsMap();
    }

    public returnFieldToNotAssignedUsers(field : PageField){
        if( (this.fieldsMap.get(undefined) || [] ).filter(x => x.name == field.name).length > 0)
        {
           return;
        }

        this.fieldsMap.forEach((values ,  key , _ )=> {
            
            this.fieldsMap.set(key, values.filter(x => x.name != field.name));
        });

        const f: PageField[] = this.fieldsMap.get(undefined) || [];
        field.signerId = undefined;
        f.push(field);
    }

    // public removeField(assignedField: AssignedField) {
    public removeField(assignedField: PageField) {
        //const _key = assignedField.contact.id + "~" + assignedField.contact.name;
        const _key = assignedField.signerId;
        if (this.fieldsMap.has(_key)) {
            let fieldsArr = this.fieldsMap.get(_key);
            if (fieldsArr) {
                // fieldsArr = fieldsArr.filter(f => f.name != assignedField.name);
                let fieldsArrAssign = fieldsArr.filter(f => f.name == assignedField.name && f.page == assignedField.page && f.templateId == assignedField.templateId);
                if(fieldsArrAssign.length > 0){
                    fieldsArr = fieldsArr.filter(f => f !== fieldsArrAssign[0]);
                }
                
                if (fieldsArr.length == 0)
                    this.fieldsMap.delete(_key);
                else
                    this.fieldsMap.set(_key, fieldsArr);
            }
        }
    }

    ngOnDestroy(): void {
        //this.groupSignBS.next(null);
        this.groupSignBS.complete();
        this.groupFieldsBS.complete();
        this.fieldsMap = null;
    }



    setAllRadiosInGroupToSigner(signerId, groupName){
         if (this.fieldsMap.has(signerId)) {
             let newMapping = new Map<string, PageField[]>();
             newMapping.set(signerId, this.fieldsMap.get(signerId));
             this.fieldsMap.forEach((value: PageField[], key: string) => {
             if(key != signerId){
                 let radios = value.filter(x => x instanceof RadioField && x.groupName == groupName);
                 let otherFields = value.filter(x => (x instanceof RadioField && x.groupName != groupName) || !(x instanceof RadioField));
                 otherFields = this.removeDuplication(otherFields);
                 newMapping.set(key, otherFields);
                 let fieldsArr = this.fieldsMap.get(signerId);
                 let fieldsArr2 = newMapping.get(signerId);
                 if(fieldsArr2 != undefined)
                 {
                    fieldsArr = fieldsArr.concat(fieldsArr2);
                 }
                 fieldsArr = fieldsArr.concat(radios);
                 fieldsArr = this.removeDuplication(fieldsArr);
                 newMapping.set(signerId, fieldsArr);    
             }              
             });
           
             this.updateFieldsMap(newMapping);
         }
    }
    
    //Set all fields type to signer except from signature fields
    setFieldsToSigner(signerId: any, templateId) {
        if (this.fieldsMap.has(signerId)) {
            let newMapping = new Map<string, PageField[]>();
            newMapping.set(signerId, this.fieldsMap.get(signerId));
            let fieldsArr = this.fieldsMap.get(signerId);
            this.fieldsMap.forEach((value: PageField[], key: string) => {
                
                if(key != signerId){
                    let otherSignatureFields = value.filter(x => x instanceof SignatureField)
                    let otherFields = value.filter(x=> !(x instanceof SignatureField))
                   
                    newMapping.set(key, otherSignatureFields);
                    
                    fieldsArr = fieldsArr.concat(otherFields);
                   
                    fieldsArr = this.removeDuplication(fieldsArr);
                    newMapping.set(signerId, fieldsArr);   
                }
            });    
            this.updateFieldsMap(newMapping);
        }
    }
    
    setSignatureFieldsToSigner(signerId: any, templateId) {
        if (this.fieldsMap.has(signerId)) {
            let newMapping = new Map<string, PageField[]>();
            newMapping.set(signerId, this.fieldsMap.get(signerId));
            let fieldsArr = this.fieldsMap.get(signerId);
            this.fieldsMap.forEach((value: PageField[], key: string) => {
                if(key != signerId){
                    let otherSignatureFields = value.filter(x => x instanceof SignatureField)
                    let otherFields = value.filter(x=> !(x instanceof SignatureField))
                    
                    newMapping.set(key, otherFields);
                    
                    fieldsArr = fieldsArr.concat(otherSignatureFields);
                    
                    fieldsArr = this.removeDuplication(fieldsArr);
                    newMapping.set(signerId, fieldsArr);                    
                }
            });    
            this.updateFieldsMap(newMapping);
        }
    }

    //Remove equals objects(all properties equals). and not remove duplication by a key.
    removeDuplication(fieldsArr: PageField[]) {
        let newArr  = [];

        fieldsArr.forEach(x => 
        {
          
           let index =  newArr.findIndex(a => a.name == x.name);
           if(index < 0)
           {
             newArr.push(x);
           }
           else
           {
               let item = newArr[index];
               if(x.page != item.page ||x.x != item.x || x.y != item.y ||
                x.width != item.width ||   x.height != item.height ||
                x.templateId != item.templateId)
                {
                    newArr.push(x);
                }
                
           }
        }


        );

        return newArr;
    }

}