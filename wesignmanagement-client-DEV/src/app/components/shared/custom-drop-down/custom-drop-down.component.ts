import { Component, OnInit, ElementRef, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { KeyValue } from '@angular/common';
import { AppState, IAppState } from 'src/app/state/app-state.interface';
import { Store } from '@ngrx/store';

@Component({
  selector: 'app-custom-drop-down',
  templateUrl: './custom-drop-down.component.html',
  styleUrls: ['./custom-drop-down.component.css']
})
export class CustomDropDownComponent implements OnInit, OnChanges {
  @Input() options: any;
  @Input() tabindex: number = 0;
  @Input() public currentSelection: any;
  @Input() isDisabled: boolean = false;
  @Input() additionalClass: String = "";
  @Input() enableSearch: boolean = false;
  @Input() multiSelect: boolean = false;
  @Output() public currentSelectionChange = new EventEmitter<any>();
  @Output() public clickEvent = new EventEmitter<any>();
  public filteredOptions: {};
  public filterValue: string;
  public showDropDown: boolean = false;
  public appState: AppState;
  public selectedItems = [];
  public selectedOptionsText: string = '';
  constructor(private _elemRef: ElementRef, private store: Store<IAppState>) { }
  
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['options']) {
      this.selectedOptionsText = this.getSelectedOptionsText();
    }
    if (changes['currentSelection']) {
      this.selectedOptionsText = this.getSelectedOptionsText();
    }
  }

  ngOnInit() {
    this.store.select<any>('appstate').subscribe((state: any) => {
      this.appState = state;
      this.showDropDown = false;
      if (this.enableSearch)
        this.filterValue = "";
      this.filteredOptions = this.options ? { ...this.options } : [];
      this.selectedOptionsText = this.getSelectedOptionsText();
    });
    if (this.multiSelect) {
      this.selectedItems = Array.isArray(this.currentSelection) ? this.currentSelection : [];
    } else {
      this.selectedItems = [];
    }
  }

  public clearSelection($event: MouseEvent) {
    $event.stopPropagation(); // Prevent dropdown from closing
    if (this.multiSelect) {
      this.selectedItems = [];
    } else {
      this.currentSelection = null;
    }
    this.currentSelectionChange.emit(this.multiSelect ? this.selectedItems : this.currentSelection);
    this.selectedOptionsText = this.getSelectedOptionsText();
  }

  public filterOptions() {
    this.filteredOptions = this.options ? { ...this.options } : [];
    if (this.filterValue.length > 0) {
      let filteredObj = {};
      Object.keys(this.filteredOptions).forEach(key => {
        if (this.filteredOptions[key].toLowerCase().includes(this.filterValue.toLowerCase())) {
          filteredObj[key] = this.filteredOptions[key];
        }
      });
      this.filteredOptions = filteredObj;
    }
  }

  public setCurrentSelection(option: KeyValue<any, any>) {
    if (this.appState.IsActivated) {
      if (this.multiSelect) {
        this.toggleSelection(option.key);
      } else {
        this.currentSelection = option.key;
        this.showDropDown = false;
        this.currentSelectionChange.emit(this.currentSelection);
      }
    }
    this.selectedOptionsText = this.getSelectedOptionsText();
  }

  public toggleSelection(optionKey: any) {
    if (this.selectedItems.includes(optionKey)) {
      this.selectedItems = this.selectedItems.filter(key => key !== optionKey);
    } else {
      this.selectedItems.push(optionKey);
    }
    this.selectedItems = this.selectedItems.filter(key => key in this.options);
    this.currentSelectionChange.emit(this.selectedItems);
  }

  public isSelected(optionKey: any): boolean {
    if (this.multiSelect) {
      return this.selectedItems.includes(optionKey);
    } else {
      return this.currentSelection === optionKey;
    }
  }

  public getSelectedOptionsText(): string {
    if (this.multiSelect) {
      this.selectedItems = this.selectedItems.filter(key => key in this.options);
      return this.selectedItems.length > 0 ? this.selectedItems.map(key => this.options[key]).join(', ') : '';
    } else {
      return this.options ? this.options[this.currentSelection] : '';
    }
  }

  public clickEventFunc($event: any) {
    if (this.appState.IsActivated) {
      this.clickEvent.emit($event);
      if (this.enableSearch && this.filterValue.length === 0)
        this.filteredOptions = this.options ? { ...this.options } : [];
    }
  }

  public outSideClick($event: any): void {
    if (this.appState.IsActivated) {
      this.showDropDown = false;
    }
  }
  public doSearchKeyUp($event) {

  }

}
