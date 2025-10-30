import { Component, EventEmitter, OnInit, Output } from '@angular/core';

@Component({
  selector: 'app-smart-card-alert',
  templateUrl: './smart-card-alert.component.html',
  styleUrls: ['./smart-card-alert.component.scss']
})
export class SmartCardAlertComponent implements OnInit {

  @Output() public hide = new EventEmitter<any>();
  @Output() public sign = new EventEmitter<any>();

  isBusy = false;

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

  releaseSubmitButton() {
    this.isBusy = false;
  }
}