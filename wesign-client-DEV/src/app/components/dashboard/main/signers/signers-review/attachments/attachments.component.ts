import { Component, ElementRef, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { Attachment, DocumentSigner } from '@models/document-api/document-create-request.model';

@Component({
  selector: 'sgn-attachments',
  templateUrl: './attachments.component.html',
  styles: []
})
export class AttachmentsComponent implements OnInit {

  @Output() public hide = new EventEmitter<any>();
  @Output() public approve = new EventEmitter<any>();
  @Input() signer : DocumentSigner;
  signerAttachments : Attachment[];
  currIndex: number = 0;
  
  
  constructor() { }

  ngOnInit() {
    if(!this.signer.signerAttachments){
      this.signerAttachments =[];
    }
    else{
      this.signerAttachments = this.signer.signerAttachments.filter(x=>x.name);
    }
    this.signerAttachments.push(new Attachment());
  }

  public send($event) {
    $event.preventDefault();
    $event.stopPropagation();
    if(!this.signerAttachments[this.signerAttachments.length-1].name){
      this.signerAttachments.pop();
    }
    if (this.signerAttachments.length > 0) {
      this.signer.signerAttachments = this.signerAttachments.filter(x=>x.name != '');
    }
    else{
      this.signer.signerAttachments = [];
    }
    this.approve.emit();
  }

  public close(){
    this.hide.emit();
  }

  public updateIsMandatory(index) {
    this.signerAttachments[index].isMandatory = (<HTMLInputElement>document.getElementById(index)).checked;
  }
  
  public addAttachment(){
    if( !this.isAttachmentEmpty(this.signerAttachments.length-1)){
      this.signerAttachments.push(new Attachment());
      this.currIndex ++;
    }
  }

  private isAttachmentEmpty(index){
    return !this.signerAttachments[index].name;
  }

  public removeAttachment(index){
    if(index == this.currIndex && this.isAttachmentEmpty(index)){
      return;
    }
    let lastIndex = this.signerAttachments.length-2;
    lastIndex = lastIndex == 0 ? 1 : lastIndex;
    if(lastIndex > 0){
      this.signerAttachments[index] = this.signerAttachments[lastIndex];
      if(this.isAttachmentEmpty(this.signerAttachments.length-1)){
        this.signerAttachments.pop();
        this.currIndex --;
      }
      this.signerAttachments.pop();
      this.signerAttachments.push(new Attachment());
    }
  }
}
