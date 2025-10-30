import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

@Component({
  selector: 'app-meaning-of-signature',
  templateUrl: './meaning-of-signature.component.html',
  styleUrls: ['./meaning-of-signature.component.scss']
})
export class MeaningOfSignatureComponent implements OnInit {

  @Input() signerName : string;
  meansSelectedValue: string= "";
  constructor() { }


  @Output() public cancelEvent = new EventEmitter<any>();
  @Output() public okEvent = new EventEmitter<string>();
  
  public meaningOptions=  ["I approve this document", "I have reviewed this document", "I am responsible for this document", "I am the author of this document"];
  ngOnInit(): void {
  }
  

  onSelected(value)
  {
    this.meansSelectedValue = value;

  }
  continue()
  {
      this.okEvent.emit(this.meansSelectedValue);
  }

  cancel(){
        this.cancelEvent.emit();
  }
}
