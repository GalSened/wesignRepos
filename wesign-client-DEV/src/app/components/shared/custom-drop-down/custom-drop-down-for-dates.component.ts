import { Component, OnInit, ElementRef, Output, EventEmitter, Input } from '@angular/core';
import { KeyValue } from '@angular/common';
import { Store } from '@ngrx/store';
import { AppState, IAppState } from '@state/app-state.interface';

@Component({
  selector: "app-custom-drop-down",
  templateUrl: "./custom-drop-down-for-dates.component.html",
  
})
export class CustomDropDownForDatesComponent implements OnInit {
  @Input() LastValidOptionIndex: number =0; 
  @Input() shouldDisable: boolean = false;
  @Input() options: any;
  @Input() tabindex: number = 0;
  @Input() public currentSelection: any;
  @Input() isDisabled: boolean = false;
  @Input() additionalClass: String = "";


  @Input() enableSearch: boolean = false;
  @Output() public currentSelectionChange = new EventEmitter<number>();
  @Output() public clickEvent = new EventEmitter<any>();

  public showDropDown: boolean = false;
  public appState: AppState;
  constructor(private _elemRef: ElementRef, private store: Store<IAppState>) { }

  ngOnInit() {  
    this.store.select<any>('appstate').subscribe((state: any) => {
      this.appState = state;
    });
  }

  public setCurrentSelection(option: KeyValue<any, any>) {
    //if(this.appState.IsActivated){
      this.currentSelection = option.key;
      this.showDropDown = false;
      this.currentSelectionChange.emit(option.key);


   // }
  }

  public clickEventFunc($event: any) {
   // if(this.appState.IsActivated){
      this.clickEvent.emit($event);      
   // }
  }

  public orderOriginal (a: any, b: any): number  {
    return 0
  }

  public outSideClick($event: any): void {
    //if(this.appState.IsActivated){
      this.showDropDown = false;      
    //}
  }
  
  public doSearchKeyUp($event)
  {

  }

}
