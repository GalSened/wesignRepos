import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { BaseResult } from "@models/base/base-result.model";
import { AddContactResult } from "@models/contacts/add-contact-result.model";
import { ContactFilter } from "@models/contacts/contact-filter.model";
import { Contact } from "@models/contacts/contact.model";
import { ContactsResult } from "@models/contacts/contacts-result.model";
import { Contacts } from '@models/contacts/contacts.model';
import { BatchRequest } from '@models/document-api/batch-request.model';
import { AppConfigService } from './app-config.service';
import { SignaturesImagesModel } from '@models/contacts/signature-images-result.model';
import { ContactsGroup, ContactsGroupsResponse } from '@models/contacts/contacts-groups-result.model';

@Injectable()
export class ContactApiService {

    private contactApi : string = "";

    constructor(private httpClient: HttpClient,
        private appConfigService: AppConfigService) {
        this.contactApi = this.appConfigService.apiUrl + "/contacts";
    }

    public getContacts(contactFilter: ContactFilter) {
        return this.httpClient.get<ContactsResult>(`${this.contactApi}?key=${contactFilter.key}&` +
            `offset=${contactFilter.offset}&limit=${contactFilter.limit}&includeTabletMode=${contactFilter.includeTabletMode}`, { observe: "response" });
    }

    public addContact(contact: Contact) {
        return this.httpClient.post<AddContactResult>(`${this.contactApi}`, contact);
    }

    public addContacts(base64:Contacts){
        return this.httpClient.post<AddContactResult>(`${this.contactApi}/bulk`,base64,{ observe: "response" })
    }

    public saveContact(contact: Contact) {
        return this.httpClient.put<BaseResult>(`${this.contactApi}/${contact.id}`, contact);
    }

    public deleteContact(contactId: number) {
        return this.httpClient.delete<BaseResult>(`${this.contactApi}/${contactId}`);
    }

    public getContactById(contactId: string) {
        return this.httpClient.get<Contact>(`${this.contactApi}/${contactId}`);
    }

    public deleteContactsBatch(contactBatchRequest :BatchRequest)
    {
        return this.httpClient.put<string[]>(`${this.contactApi}/deletebatch`,contactBatchRequest );
    }


    public getAllSaveSignatureForSelfSignContact(documentCollectionId :string)
    {
        return this.httpClient.get<SignaturesImagesModel>(`${this.contactApi}/signatures/${documentCollectionId}` );
    }


    public updateSignaturesImages( input: SignaturesImagesModel){
        return this.httpClient.put(`${this.contactApi}/signatures`, input);
      }
    

      // contacts groups 

      public readContactsGroups(key: string, offset : number, limit : number) {
        return this.httpClient.get<ContactsGroupsResponse>(`${this.contactApi}/Groups?key=${key}&` +
            `offset=${offset}&limit=${limit}`, { observe: "response" });
    }
    public createContactsGroup(contactsGroup : ContactsGroup ){
        return this.httpClient.post<BaseResult>(`${this.contactApi}/Group`,contactsGroup);
    }

    public DeleteContactsGroup(id : string ){
        return this.httpClient.delete<BaseResult>(`${this.contactApi}/Group/${id}`);
    }
    public UpdateContactGroup(contactsGroup : ContactsGroup ){
        return this.httpClient.put<BaseResult>(`${this.contactApi}/Group/${contactsGroup.id}`,contactsGroup);
    }
    public readGroup(contactsGroup: ContactsGroup){
        return this.httpClient.get<ContactsGroup>(`${this.contactApi}/Group/${contactsGroup.id}`);
    }

}
