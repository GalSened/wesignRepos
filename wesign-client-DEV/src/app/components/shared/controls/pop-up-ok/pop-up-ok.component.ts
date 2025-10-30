import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Modal } from '@models/modal/modal.model';

@Component({
  selector: 'sgn-pop-up-ok',
  templateUrl: './pop-up-ok.component.html',
  styles: [
  ]
})
export class PopUpOkComponent implements OnInit {

  @Input() public data: Modal = new Modal();  
  @Output() public submitEvent = new EventEmitter<any>();

  constructor() { }

  ngOnInit() {
  }

  confirm(){
    this.submitEvent.emit();
  }
  
}
