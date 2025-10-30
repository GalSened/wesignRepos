import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { StateService } from '../services/state.service';

@Component({
  selector: 'app-confirm-message-pop-up',
  templateUrl: './confirm-message-pop-up.component.html',
  styleUrls: ['./confirm-message-pop-up.component.scss']
})
export class ConfirmMessagePopUpComponent implements OnInit {

  @Input() public title = "";
  @Input() public message = "";
  @Input() public message2 = "";
  @Input() public messageBold = "";
  
  @Output("closePopup") closePopupFun: EventEmitter<any> = new EventEmitter();
  constructor(  private translate: TranslateService, private stateService: StateService) { }

  ngOnInit(): void {
  }

  public Close()
  {
    this.closePopupFun.emit();
  }
}