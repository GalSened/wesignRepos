import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Modal } from '@models/modal/modal.model';

@Component({
  selector: 'sgn-pop-up-confirm',
  templateUrl: './pop-up-confirm.component.html',
  styles: []
})
export class PopUpConfirmComponent implements OnInit {
  @Input() public data: Modal = new Modal();
  @Output() public cancelEvent = new EventEmitter<any>();
  @Output() public submitEvent = new EventEmitter<any>();

  constructor() { }

  ngOnInit() {
  }

  confirm(){
    this.submitEvent.emit();
  }

  cancel(){
    this.cancelEvent.emit();
  }

}
