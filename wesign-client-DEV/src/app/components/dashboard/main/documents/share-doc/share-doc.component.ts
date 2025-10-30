import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ShareRequest } from '@models/document-api/share-request.model';
import { Errors } from '@models/error/errors.model';
import { DocumentApiService } from '@services/document-api.service';
import { SharedService } from '@services/shared.service';

@Component({
  selector: 'sgn-share-doc',
  templateUrl: './share-doc.component.html',
  styles: []
})
export class ShareDocComponent implements OnInit {
  
  @Output() public hide = new EventEmitter<any>();
  @Input() docId:string;
  name: string;
  means: string;
  isBusy: boolean;
  submitted: boolean;
  hasError : boolean = false;

  constructor(private documentApiService: DocumentApiService, private sharedService: SharedService) { }

  ngOnInit() {
  }

  public close(){
    this.hide.emit();
  }

  public send(){
    this.submitted = true;
    if(!this.name||this.name==""||!this.means||this.means==""){
      return;
    }
    this.isBusy = true;
    let input = new ShareRequest();
    input.documentCollectionId = this.docId;
    input.signerName = this.name;
    input.signerMeans = this.means;
    this.documentApiService.shareDocument(input).subscribe(
      (data) =>{
        this.sharedService.setSuccessAlert("DOCUMENT.SHARED_DOCUMENT_SUCCESSFULLY");
        this.submitted = false;
        this.isBusy = false; 
        this.hasError = false;
        this.hide.emit();
      },
      (err)=>{
        this.sharedService.setErrorAlert(new Errors(err.error));
        this.hasError = true;
        this.submitted = false;
        this.isBusy = false; 
      }
    );

  }
}
