import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-alert',
  templateUrl: './alert.component.html',
  styleUrls: ['./alert.component.css']
})
export class AlertComponent implements OnInit {

  public title : string = "";
  @Input() public itemName: string = "";
  @Input() public itemType: string = "";
  @Input() public operationType: string = "";
  
  @Output() public removeDeletionAlert = new EventEmitter<number>();
  @Output() public deleteOperation = new EventEmitter<number>();

  constructor() { }

  ngOnInit(): void {
    this.title = this.operationType == "delete" ? "Delete" : 
                 this.operationType == "update" ? "Update" : "";
  }
  public hideAlert() {
    this.removeDeletionAlert.emit()
  }

  public doOperation(){
    this.deleteOperation.emit();
  }

  cancel() {
    this.hideAlert();
  }

}
