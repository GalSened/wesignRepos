import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

@Component({
  selector: 'app-alert',
  templateUrl: './alert.component.html',
  styleUrls: ['./alert.component.scss']
})
export class AlertComponent implements OnInit {

  @Input() public message : string = "";
  @Output() public closeAlertEvent = new EventEmitter<string>();
  @Input() isError: boolean = true;
  @Input() isConfirm: boolean = false;

  constructor() { }

  ngOnInit(): void {
    setTimeout(() => {
      this.closeAlert();
  }, 6000);
  }

  public closeAlert(){
    this.closeAlertEvent.emit();
  }

}
