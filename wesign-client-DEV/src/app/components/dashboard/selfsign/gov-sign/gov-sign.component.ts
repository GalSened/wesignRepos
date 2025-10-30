import { Component, OnDestroy, OnInit } from '@angular/core';
import { Signer1FileSiging } from '@models/template-api/upload-request.model';
import { AppState, IAppState } from '@state/app-state.interface';
import { Observable, Subscription } from 'rxjs';
import { SharedService } from '@services/shared.service';
import { StateProcessService } from '@services/state-process.service';
import { SelfSignApiService } from '@services/self-sign-api.service';
import { Store } from '@ngrx/store';
import { ActivatedRoute, Router } from '@angular/router';
import { SmartCardSigningService } from '@services/smart-card-signing.service';
import { UserApiService } from '@services/user-api.service';
import { TranslateService } from '@ngx-translate/core';
import { DocumentOperations } from '@models/enums/document-operations.enum';
import { UpdateSelfSignRequest } from '@models/self-sign-api/update-self-sign.model';
import { SelfSignUpdateDocumentResult } from '@models/self-sign-api/self-sign-update-document-result.model';
import { Errors } from '@models/error/errors.model';
import { PDFFields } from '@models/template-api/update-template-request.model';
import { SignatureField } from '@models/template-api/page-data-result.model';
import { HttpClient } from '@angular/common/http';
import { SignatureType } from '@models/enums/signature-type.enum';
import { SignatureFieldKind } from '@models/enums/signature-field-kind.enum';
import { DocumentApiService } from '@services/document-api.service';

@Component({
  selector: 'sgn-gov-sign',
  templateUrl: './gov-sign.component.html',
  styles: [
  ]
})
export class GovSignComponent implements OnInit, OnDestroy {
  public name: string;
  public state$: Observable<AppState>;
  public isBusy: boolean = false;
  public showOpenLink: boolean = false;
  public collectionId: string;
  public documentId: string;
  public signerToken: string;
  private signer1FileSiging: Signer1FileSiging;
  private getStateSnapshotSubscription: Subscription;
  private updateSelfSignDocumentSubscription: Subscription;

  subscription: any;

  constructor(private translate: TranslateService, private sharedService: SharedService,
    private stateService: StateProcessService, private selfSignApiService: SelfSignApiService,
    private store: Store<IAppState>, private router: Router, private smartCardServiceApi: SmartCardSigningService,
    private stateProcess: StateProcessService, private route: ActivatedRoute, private userApiService: UserApiService,
    private httpClient: HttpClient, private documentApiService: DocumentApiService) { }

  public ngOnInit(): void {
    this.state$ = this.stateService.getState();
    this.state$.subscribe(
      (state: AppState) => {
        this.signer1FileSiging = state.signer1FileSiging;
        this.name = this.limitFileName(this.signer1FileSiging.FileName);
        this.signer1FileSiging.Signer1Credential = state.SelfSignSignerAuth;
      });
    this.route.params.subscribe(res => {
      this.collectionId = res.colid;
      this.documentId = res.docid;
    });
    this.signerToken = this.userApiService.authToken;
    this.smartCardServiceApi.Init(this.signerToken);
  }

  public ngOnDestroy(): void {
    this.selfSignApiService.deleteSelfSignDocument(this.collectionId).subscribe(res => {
    });
  }

  public accept() {
    this.showOpenLink = true;
  }

  public closePopUp() {
    this.showOpenLink = false;
  }

  public signFile(): Promise<any> {
    return new Promise((resolve, reject) => {
      this.getStateSnapshotSubscription = this.stateProcess.getStateSnapshot().subscribe((state: AppState) => {
        this.isBusy = true;
        this.sharedService.setBusy(true, "DOCUMENT.SIGNING");

        const request = new UpdateSelfSignRequest();
        request.signerAuthentication.signer1Credential = state.SelfSignSignerAuth;

        // Get the file from the state
        const file = state.signer1FileSiging.Base64File;
        if (!file) {
          reject();
          return;
        }

        request.documentCollectionId = this.collectionId;
        request.documentId = this.documentId;
        request.name = this.name;
        request.operation = DocumentOperations.Close;

        this.sharedService.setBusy(true, "DOCUMENT.SIGNING");
        this.updateSelfSignDocumentSubscription = this.selfSignApiService.updateGovSelfSignDocument(request).subscribe(
          (selfSignUpdateDocumentResult: SelfSignUpdateDocumentResult) => {
            this.documentStartSmartCardSigning(resolve, reject, selfSignUpdateDocumentResult.token, request);
          },
          (error) => {
            this.sharedService.setBusy(false);
            let ex = new Errors(error.error);
            this.sharedService.setErrorAlert(ex);
            reject();
          }
        );
      });
    });
  }

  private documentStartSmartCardSigning(resolve: (value: any) => void, reject: (reason?: any) => void, token: string, request: UpdateSelfSignRequest) {
    this.subscription = this.smartCardServiceApi.getSmartCardSigningResultEvent()
      .subscribe(({ isSuccess: isSuccess, xml: xmlContent, fileName: fileName }) => {
        if (isSuccess) {
          try {
            // 1. Base64-decode to get raw text
            const newFileName = fileName ? fileName + ".signed" : "FileNameNotFound.xml.signed";

            const binaryString = atob(xmlContent);  // Decode base64
            const uint8Array = new Uint8Array(binaryString.length);
            for (let i = 0; i < binaryString.length; i++) {
                uint8Array[i] = binaryString.charCodeAt(i);
            }
            const decoder = new TextDecoder('utf-8');
            const xmlString = decoder.decode(uint8Array);

            // 2. Parse for validation (optional, but helps catch malformed XML)
            const parser = new DOMParser();
            const xmlDoc = parser.parseFromString(xmlString, "text/xml");
            if (xmlDoc.getElementsByTagName("parsererror").length > 0) {
              throw new Error("Invalid XML format");
            }

            // 3. Create a Blob representing the XML content
            const blob = new Blob([xmlString], { type: "application/xml" });

            // 4. Create a hidden <a> element & programmatically click it to trigger download
            const a = document.createElement("a");
            a.href = URL.createObjectURL(blob);
            a.download = newFileName;  // "mySignedDoc.xml", for example
            a.style.display = "none";
            document.body.appendChild(a);
            a.click();

            // Clean up
            URL.revokeObjectURL(a.href);
            document.body.removeChild(a);

            this.documentSignedSuccessfully(resolve);

          } catch (err) {
            this.sharedService.setErrorAlert(this.translate.instant("SERVER_ERROR.149"));
            this.router.navigate(["dashboard", "main"]);
          }

          this.subscription.unsubscribe();
          this.sharedService.setBusy(false);
          this.isBusy = false;
          this.closePopUp();

        }
        else {
          this.sharedService.setBusy(false);
          this.sharedService.setErrorAlert(new Errors());
          reject();
        }
      },
        (error) => {
          console.error('Error occurred:', error);
          this.sharedService.setBusy(false);
          this.sharedService.setErrorAlert(new Errors());
          reject(error);
        });

    this.sharedService.setBusy(true, "DOCUMENT.SIGNING");
    this.smartCardServiceApi.sign(request.documentId, token, true);
    setTimeout(() => {
      this.sharedService.setBusy(false);
    }, 180000);
  }

  private documentSignedSuccessfully(resolve: (value: any) => void) {
    this.sharedService.setSuccessAlert("DOCUMENT.SAVED_SUCCESSFULY");
    this.sharedService.setBusy(false);
    resolve(undefined);
    this.showOpenLink = false;
    this.router.navigate(["dashboard", "success", "selfsign"]);
  }

  private limitFileName(fileName: string, maxLength: number = 50) {
    // Extract the file extension
    const fileExtension = fileName.substring(fileName.lastIndexOf('.'));

    // Ensure there's an extension and that it's valid
    if (fileExtension && fileExtension.length > 0) {
      // Get the part of the file name before the extension
      const baseFileName = fileName.substring(0, fileName.lastIndexOf('.'));

      // If the base file name is longer than the max length minus the extension length
      if (baseFileName.length > maxLength - fileExtension.length) {
        // Truncate the base file name to fit within the maxLength
        return baseFileName.substring(0, maxLength - fileExtension.length) + fileExtension;
      }
    }

    // If no truncation needed, return the original file name
    return fileName;
  }
}