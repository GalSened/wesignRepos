import { Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import { Attachment } from 'src/app/models/responses/attachment.model';
import { StateService } from 'src/app/services/state.service';

@Component({
  selector: 'app-attachment',
  templateUrl: './attachment.component.html',
  styleUrls: ['./attachment.component.scss']
})
export class AttachmentComponent implements OnInit {

  @ViewChild("fileInput", { static: true }) public el: ElementRef;
  @Input()  attachment: Attachment ; 
  public busy : boolean = false;  
  public file: any = null;
  public attachmentName: string ="";

  constructor(private stateService : StateService ) { }

  ngOnInit(): void {
    this.attachmentName =  this.attachment.isMandatory ? this.attachment.name + " *" : this.attachment.name ;
  }

  public fileUpload() {
    this.busy = true;
    if (this.el.nativeElement.files.length > 0) {
        this.file = this.el.nativeElement.files[0];
        const reader = new FileReader();
        reader.readAsDataURL(this.file);
        reader.onload = () => {
          this.attachment.base64File = reader.result.toString();          
        };
      }
    }
    
  public saveAttachment(){
      this.stateService.setAttachmentData(this.attachment.id, this.attachment);
   }

}
