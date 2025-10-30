import { Component, EventEmitter, OnInit, Output } from '@angular/core';

@Component({
  selector: 'app-iedas-signing-flow',
  templateUrl: './iedas-signing-flow.component.html',
  styleUrls: ['./iedas-signing-flow.component.scss']
})
export class IedasSigningFlowComponent implements OnInit {
  @Output() public hide = new EventEmitter<any>();
  @Output() public sign = new EventEmitter<any>();

  public isBusy = false;

  constructor() { }

  ngOnInit(): void {
  }

  closePopUp() {
    this.hide.emit();
  }

  smartCardSigning() {
    this.isBusy = true;
    this.sign.emit();
  }

  public releaseSubmitButton() {
    this.isBusy = false;
  }
}