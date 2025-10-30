import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { DocumentOperation } from 'src/app/enums/document-operation.enum';
import { Errors } from 'src/app/models/error/errors.model';
import { UpdateDocumentCollectionRequest } from 'src/app/models/requests/update-document-collection-request.model';
import { AppConfigService } from 'src/app/services/app-config.service';
import { DocumentsService } from 'src/app/services/documents.service';
import { LiveEventsService } from 'src/app/services/live-events.service';
import { StateService } from 'src/app/services/state.service';

@Component({
  selector: 'app-decline',
  templateUrl: './decline.component.html',
  styleUrls: ['./decline.component.scss']
})
export class DeclineComponent implements OnInit {

  @Input() public token: string = "";
  public declineMessage: string = "";
  public showErrorMessage: boolean;
  public errorMessage: string;
  public isEmptyDeclineMessage: boolean;

  constructor(private documentsService: DocumentsService, private stateService: StateService, private router: Router, private appConfigService: AppConfigService,
    private liveEventsService: LiveEventsService, private translate : TranslateService) { }

  ngOnInit(): void {
  }

  public cancel() {
    this.isEmptyDeclineMessage = false;
    this.showErrorMessage = false;
    this.stateService.showDeclineForm = false;
  }

  public decline() {
    this.isEmptyDeclineMessage = this.declineMessage == "";
    this.showErrorMessage = this.declineMessage == "";
    if (this.declineMessage == "") {
      return;
    }
    this.stateService.showLoader = true;

    let updateDocumentCollectionRequest = new UpdateDocumentCollectionRequest();
    updateDocumentCollectionRequest.Operation = DocumentOperation.Decline;
    updateDocumentCollectionRequest.Documents = [];
    updateDocumentCollectionRequest.SignerNote = this.declineMessage;

    //console.log(updateDocumentCollectionRequest);
    this.documentsService.updateDocument(this.token, updateDocumentCollectionRequest)
      .subscribe(
        (res) => {
          this.stateService.showDeclineForm = false; 
          this.stateService.showLoader = false;
          this.liveEventsService.signerDecline(this.token);
          this.router.navigate(["/decline"]);
          if(res.redirectUrl != undefined && res.redirectUrl != ""){
            let redirectTimeout = this.appConfigService.redirectTimeoutconfig;
            setTimeout(() => {
              window.location.href = res.redirectUrl;               
            }, redirectTimeout);  
          }          
        },
        (err) => {
          this.stateService.showDeclineForm = false; 
          this.stateService.showLoader = false;
          this.showErrorMessage = true;
          let result = new Errors(err.error);
          this.errorMessage = this.translate.instant('SERVER_ERROR.'+result.errorCode) ;
          //this.errorMessage = err.error.errors.error[0];
        }
      );
  }

  showDeclineForm(): boolean {
    return this.stateService.showDeclineForm;
  }
}
