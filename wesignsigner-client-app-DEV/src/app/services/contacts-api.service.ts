import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { AppConfigService } from './app-config.service';
import { SignaturesImagesModel } from '../models/responses/signatures-images.model';
@Injectable({
  providedIn: 'root'
})
export class ContactsApiService {

  private contactsSignerApi :string;

  constructor(private httpClient: HttpClient,
              private router: Router,
              private appConfigService: AppConfigService) {
                this.contactsSignerApi = this.appConfigService.apiUrl + "/contacts";
  }  

  readSignaturesImages(token: string){
    return this.httpClient.get<SignaturesImagesModel>(`${this.contactsSignerApi}/signatures/${token}`);
  }

  updateSignaturesImages(token: string, input: SignaturesImagesModel){
    return this.httpClient.put(`${this.contactsSignerApi}/signatures/${token}`, input);
  }
}
