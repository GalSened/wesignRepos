import { Component, EventEmitter, Input, OnInit, Output, ElementRef } from '@angular/core';
import { KeyValue } from '@angular/common';

@Component({
    selector: 'sgn-custom-dropdown',
    templateUrl: 'custom-dropdown.component.html'
})

export class CustomDropDownComponent implements OnInit {
    @Input() options: string[];

    @Input() tabindex: number = 0;
    //@Input() options: KeyValue<number, string>;

    @Output() public currentSelectionChange = new EventEmitter<number>();
    @Output() public clickEvent = new EventEmitter<any>();
    // @Output() public nativeDragStart = new EventEmitter<any>();
    // @Output() public nativeDragEnd = new EventEmitter<any>();

    // private _currentSelection: SendingMethod;
    // get currentSelection() {
    //     return this._currentSelection;
    // }
    // @Input()
    // set currentSelection(value) {
    //     //if (value !== '' && value !== null && value !== undefined)
    //     this._currentSelection = value;
    // }
    @Input() public currentSelection: any;

    @Input() isDisabled: boolean = false;

    @Input() additionalClass: String = "";

    @Input() isNativeDraggable: boolean = false;

    public showDropDown: boolean = false;

     public size: number;


    constructor(private _elemRef: ElementRef) { }

    ngOnInit() {
        this.size = this.options ?
             Array.isArray(this.options) ? this.options.length :
                 Object.keys(this.options).length / 2 : 0;
      //  console.log(this.options)
    }

    public setCurrentSelection(option: KeyValue<any, any>) {
        this.currentSelection = option.key;
        this.showDropDown = false;
        this.currentSelectionChange.emit(option.key);
    }

    public clickEventFunc($event: any) {
        this.clickEvent.emit($event);
    }

    public outSideClick($event: any): void {
        this.showDropDown = false;
    }

    // public nativeDragStartFunc($event: any) {
    //     this.nativeDragStart.emit($event);
    // }

    // public nativeDragEndFunc($event: any) {
    //     this.nativeDragEnd.emit($event);
    // }

}