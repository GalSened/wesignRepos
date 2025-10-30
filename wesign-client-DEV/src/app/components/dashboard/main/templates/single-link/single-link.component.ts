import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import {  DocumentSigner } from '@models/document-api/document-create-request.model';
import { TemplateSingleLink } from '@models/template-api/template-single-link.model';
import { LinksApiService } from '@services/links-api.service';


@Component({
  selector: 'sgn-single-link',
  templateUrl: './single-link.component.html',
  styles: [
  ]
})
export class SingleLinkComponent implements OnInit{



  constructor(  private linksApiService: LinksApiService,) { 
    this.currSigner = new DocumentSigner();


  }
  ngOnInit(): void {
   
  }

 @Input() selectedSingleLink: string;
 @Input() templateId: string;

  @Input()  currSigner: DocumentSigner;
    @Output() public done = new EventEmitter<string>();
   
    showAttachments: boolean = true ;
    
    
    
    Close() {
        this.done.emit();
    }
  copyMessage() {
    const selBox = document.createElement('textarea');
    selBox.style.position = 'fixed';
    selBox.style.left = '0';
    selBox.style.top = '0';
    selBox.style.opacity = '0';
    selBox.value = this.selectedSingleLink;
    document.body.appendChild(selBox);
    selBox.focus();
    selBox.select();
    document.execCommand('copy');
    document.body.removeChild(selBox);
}
public setInfoAndOpenView()
{
 

}

approve(){
  let templateSingleLink = new TemplateSingleLink();
  templateSingleLink.templateId = this.templateId;
  
  
  if( this.currSigner.signerAttachments == null ||  this.currSigner.signerAttachments.length == 0)
  {
    templateSingleLink.singleLinkAdditionalResources =[];
  }
  else
  {
  templateSingleLink.singleLinkAdditionalResources = this.currSigner.signerAttachments.map((x) => {
    return {
      
      templateId: this.templateId,
      type: 1,
      data: x.name,
      isMandatory: x.isMandatory
    }
  
  })
  }
  this.linksApiService.updateSingleLinkAttachments(templateSingleLink).subscribe((res) => {},(err) => {},() => {
    this.showAttachments = false;
  
  });
}
hide(){
  this.showAttachments = false;

}
}
