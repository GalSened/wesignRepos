import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { DocumentSigner } from '@models/document-api/document-create-request.model';


@Component({
  selector: 'sgn-note',
  templateUrl: './note.component.html',
  styles: []
})
export class NoteComponent implements OnInit {

  @Output() public hide = new EventEmitter<any>();
  @Input() signer : DocumentSigner;
  public note: string;

  constructor() { }

  ngOnInit() {
    this.note = this.signer.senderNote;
  }

  public send(){
    if(this.note && this.note != ""){
      this.signer.senderNote = this.note;
    }
    if (this.note=="")
    {
      this.signer.senderNote = undefined;
    }
    this.hide.emit();
  }

  public close(){
    this.hide.emit();
  }

}
