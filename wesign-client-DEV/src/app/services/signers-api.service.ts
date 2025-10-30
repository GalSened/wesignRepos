import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { AppConfigService } from './app-config.service';
import { ReplaceSignerWithDetailsRequest } from '@models/document-api/replace-signer-with-details-request';

@Injectable({ providedIn: 'root' })
export class SignersApiService {
    private signersApi: string = "";

    constructor(private httpClient: HttpClient,
        private appConfigService: AppConfigService) {
            this.signersApi = this.appConfigService.apiUrl + "/signers";
    }

    public replaceSignerWithDetails(collectionId: string, signerId: string, request: ReplaceSignerWithDetailsRequest) {
        return this.httpClient.put(`${this.signersApi}/${collectionId}/signer/${signerId}/replace`, request);
    }
}