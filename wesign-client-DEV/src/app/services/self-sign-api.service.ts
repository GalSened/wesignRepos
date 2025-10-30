import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SelfSignCreateResult } from '@models/self-sign-api/self-sign-create-result.model';
import { BaseResult } from '@models/base/base-result.model';
import { Signer1FileSiging, UploadRequest } from '@models/template-api/upload-request.model';
import { UpdateSelfSignRequest } from '@models/self-sign-api/update-self-sign.model';
import { AppConfigService } from './app-config.service';
import { SignerAuthentication } from '@models/self-sign-api/signer-authentication.model';
import { SelfSignUpdateDocumentResult } from '@models/self-sign-api/self-sign-update-document-result.model';
import { SplitDocumentProcess } from '@models/self-sign-api/split-document-process.model';
import { IdentityCheckFlow } from '@models/self-sign-api/identity-check-flow.model';

@Injectable({ providedIn: 'root' })
export class SelfSignApiService {

    private selfSignApi: string = "";

    //private selfSignApi = environment.apiUrl + "/selfsign";

    constructor(private httpClient: HttpClient,
        private appConfigService: AppConfigService) {
        this.selfSignApi = this.appConfigService.apiUrl + "/selfsign";
    }

    public createSelfSignDocument(request: UploadRequest) {
        return this.httpClient.post<SelfSignCreateResult>(`${this.selfSignApi}`, request);
    }

    public updateSelfSignDocument(request: UpdateSelfSignRequest) {
        return this.httpClient.put<SelfSignUpdateDocumentResult>(`${this.selfSignApi}`, request);
    }

    public updateGovSelfSignDocument(request: UpdateSelfSignRequest) {
        return this.httpClient.put<SelfSignUpdateDocumentResult>(`${this.selfSignApi}/gov`, request);
    }

    public deleteSelfSignDocument(documentCollectionId: string) {
        return this.httpClient.delete(`${this.selfSignApi}/${documentCollectionId}`);
    }

    public CheckidentityFlowEIDASSign(request: IdentityCheckFlow) {
        return this.httpClient.post<SplitDocumentProcess>(`${this.selfSignApi}/CheckidentityFlowEIDASSign`, request);
    }

    public downloadSmartCardDesktopClientInstaller() {
        return this.httpClient.get(`${this.selfSignApi}/download/smartcard`, {
            observe: "response",
            responseType: "arraybuffer",
        });
    }

    public SignUsingSigner1(request: Signer1FileSiging) {
        return this.httpClient.post(`${this.selfSignApi}/sign`, request, {
            observe: "response",
            responseType: "arraybuffer",
        });
    }

    public verifySigner1Credential(request: SignerAuthentication) {
        return this.httpClient.post(`${this.selfSignApi}/sign/verify`, request);
    }
}