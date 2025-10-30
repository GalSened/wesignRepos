import { Directive, HostListener, Output, EventEmitter, Input } from '@angular/core';

@Directive({ selector: '[togglePass]' })
export class TogglePasswordDirective {

    constructor() { }

    @Input() public togglePass: HTMLElement;
    @Input() public eyeIconProp: string;
    @Output() eyeIconPropChange: EventEmitter<string> = new EventEmitter();

    @HostListener('click', ['$event', '$event.target'])
    public onElementClick(event: any): void {
        if (!this.togglePass || this.isEnterClick(event)) return;

        event.preventDefault();
        let type = this.togglePass.getAttribute('type');
        this.togglePass.setAttribute('type', type === 'password' ? 'text' : 'password');

        if (this.eyeIconProp) {
            this.eyeIconPropChange.emit(this.eyeIconProp == 'eye' ? 'eye-off' : 'eye');
        }
    }

    isEnterClick(event) {
        return event.screenX == 0 && event.screenY == 0;
    }
}