import { Component, ElementRef, Input, OnInit, ViewChild } from "@angular/core";
import { Router } from '@angular/router';
import { Store } from "@ngrx/store";
import * as fieldActions from "@state/actions/fields.actions";
import { IAppState } from "@state/app-state.interface";

@Component({
    selector: "sgn-field-button",
    template:`

    <button #elem class="ct-button--add-field" [ngClass]="signerClassId" (click)="onclick()" [draggable]="isDraggable" (dragstart)="ondrag($event)">
    <i-feather name="{{className}}"></i-feather>          
    <div class="ct-break--shorter"></div>          
    <span>{{title}}</span>
  </button>
    `
})

export class SidebarFieldButtonComponent implements OnInit {

    @ViewChild("elem", { static: true })
    public elem: ElementRef;

    @Input()
    public text: string;
    
    @Input()
    public signerClassId: string;
    
    @Input()
    public title: string;

    @Input()
    public className: string;

    @Input()
    public isDraggable: boolean = true;

    constructor(private store: Store<IAppState>, private router: Router) { }

    public ngOnInit() {
        /* TODO */
    }

    public ondrag(event) {
        event.dataTransfer.effectAllowed = "copy";
        event.dataTransfer.setData("text", this.title);
    }

    public onclick() {
        let height = this.text.includes("Signature") ? 100 : undefined;
        let mandatory = this.text.includes("Signature") && (this.router.url.includes("groupsign") || this.router.url.includes("template/edit"));
        this.store.dispatch(new fieldActions.FieldButtonClickedAction({ 
            FieldType: this.text,
            Y: this.elem.nativeElement.getBoundingClientRect().top,
            X : 0,
            Width : undefined,
            Height : height,
            GroupName: "",
            Mandatory: mandatory}));
    }
}
 