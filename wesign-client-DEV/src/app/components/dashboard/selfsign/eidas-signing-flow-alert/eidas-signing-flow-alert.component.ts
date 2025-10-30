import { Component, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'sgn-eidas-signing-flow-alert',
  templateUrl: './eidas-signing-flow-alert.component.html',
  styles: [
  ]
})
export class EidasSigningFlowAlertComponent {
  @Output() public hide = new EventEmitter<any>();
  @Output() public sign = new EventEmitter<any>();

 
  public eidasStartProcess()
  {
    this.sign.emit();
  }
  public closePopUp()
  {
    this.hide.emit();
  }
}
