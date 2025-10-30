import { Component, EventEmitter, Input, Output } from "@angular/core";

@Component({
    selector: "[paginator]",
    template: `
        <button class="ws_button--icon">
            <div class="ws_icon__prev" (click)="toPrevPage()"></div>
        </button>
        <span>{{currentPage}} / {{last}}</span>
        <div class="ws_pagination ws_float-right">
            <button class="ws_button--icon" (click)="toNextPage()">
                <div class="ws_icon__next"></div>
            </button>
        </div>
    `,
})

export class PaginatorComponent {

    @Input()
    public currentPage: number = 1;

    @Input()
    public last: number = 1;

    @Output()
    public changed: EventEmitter<number> = new EventEmitter<number>();

    constructor() {
        this.last = this.last < 1 ? 1 : this.last;
    }

    public toPrevPage() {
        if (this.currentPage > 1) {
            this.currentPage -= 1;
            this.changed.emit(this.currentPage);
        }
    }

    public toNextPage() {
        if (this.currentPage < this.last) {
            this.currentPage += 1;
            this.changed.emit(this.currentPage);
        }
    }

    public reset(){
        this.currentPage = 1;
    }
}
