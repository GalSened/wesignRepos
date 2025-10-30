import { Component, EventEmitter, Input, Output } from '@angular/core';
import { SendingMethod } from '@models/contacts/contact.model';
import { SelectDistinctContactVideoConfrence, VideoConfrenceRequestDTO } from '@models/contacts/select-distinct-contact-video-confrence.model';
import { DocumentSigner } from '@models/document-api/document-create-request.model';
import { Errors } from '@models/error/errors.model';
import { TranslateService } from '@ngx-translate/core';
import { LinksApiService } from '@services/links-api.service';
import { SharedService } from '@services/shared.service';

@Component({
  selector: 'sgn-video-confrence',
  templateUrl: './video-confrence.component.html',
  styles: [
  ]
})
export class VideoConfrenceComponent {
  @Output() public hide = new EventEmitter<any>(); 
  @Input() signers:  SelectDistinctContactVideoConfrence[];
  @Input() documentName:  string;
  SendingMethod: typeof SendingMethod = SendingMethod;

  isSignerSelected : boolean = false;
  errorCodeMessage : string = "";
  public areAllContactsSelected: boolean = false;
  public isSubmitBusy : boolean = false;
  showErrorCodeMessage : boolean = false;
  constructor(private sharedService: SharedService,
    private linksApiService: LinksApiService,
    private translate: TranslateService,
  ) { }
  
  public close() {
    this.errorCodeMessage = "";
    this.showErrorCodeMessage = false;
    this.sharedService.setBusy(false);
    this.hide.emit();
  }

  public createZoomMeeting() {
    this.errorCodeMessage = "";
    this.showErrorCodeMessage = false;
    let selectedSigners = this.signers.filter((signer) => signer.selected);
   if(selectedSigners.length == 0) { 
    
    this.isSignerSelected = false;
    return;
   }

   this.sharedService.setBusy(true, "VIDEO_CONFRENCE_POPUP.CREATING_VIDEO_CONFERENCE");
   let videoConfrenceRequest = new VideoConfrenceRequestDTO();
   videoConfrenceRequest.VideoConferenceUsers = selectedSigners.map((signer) => {
      return {
        sendingMethod: signer.sendingMethod,
        means: signer.contactMeans,
        fullName: signer.contactName,
        phoneExtension: signer.phoneExtension
      }
   });
   videoConfrenceRequest.documentCollectionName = this.documentName;

   this.linksApiService.createVideoConference(videoConfrenceRequest).subscribe(
      (data) => {
       
        window.open(data.conferenceHostUrl, "_blank");
        this.sharedService.setBusy(false);
        this.hide.emit();
       
      },
      (error) => {
        this.showErrorCodeMessage = true;
       
        if (error.status == 0) {
          this.errorCodeMessage = this.translate.instant('SERVER_ERROR.429');
        } else {
          let result = new Errors(error.error);
          this.errorCodeMessage = this.translate.instant('SERVER_ERROR.' + result.errorCode);
        
        }
        this.sharedService.setBusy(false);
      },
      () => {
        
        
      }
    );
    
   
  }

  onRowClick(contact: SelectDistinctContactVideoConfrence, index: number) {
    contact.selected = !contact.selected ;
    this.isSignerSelected =  this.signers.filter((signer) => signer.selected).length > 0;
  
  }

  selected(event, contact: SelectDistinctContactVideoConfrence, index: number ) {
    contact.selected = !contact.selected ;
    this.isSignerSelected =  this.signers.filter((signer) => signer.selected).length > 0;
   
  }
  selectedAll(event) {
    this.areAllContactsSelected = event.target.checked;
    this.signers.forEach((contact) => {contact.selected = this.areAllContactsSelected;});
    this.isSignerSelected =this.areAllContactsSelected;
  }
}
