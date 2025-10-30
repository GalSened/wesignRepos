import { Directive, HostListener, ElementRef, Output, EventEmitter } from '@angular/core';

@Directive({ selector: '[clickOutside]' })
export class ClickOutsideDirective {
    constructor(private _elemRef: ElementRef) { }

    @Output()
    public clickOutside = new EventEmitter<any>();

    @HostListener('document:click', ['$event', '$event.target'])
    public onDocumentClick(event: any, targetElement: HTMLElement): void {
        if (!targetElement) return;

        if (!this._elemRef.nativeElement.contains(targetElement))
            this.clickOutside.emit(event);
    }
}