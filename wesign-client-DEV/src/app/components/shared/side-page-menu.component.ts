import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

@Component({
    selector: 'sgn-side-page-menu',
    template: `
        <aside>
            <menu>
                <ul *ngIf="links"  style="  display: grid; align-items: center;justify-content: center; ">
                    <li *ngFor="let link of links | keyvalue: keepOrder" 
                        id="{{link.value}}"
                        [routerLink]="link.value" 
                        routerLinkActive="is-active">
                        {{link.key | translate}}
                    </li>
                    <li *ngIf="menuAction" (click)="menuActionClick()">{{menuAction | translate}}</li>
                </ul>
            </menu>
        </aside>
    `
})

export class SidePageMenuComponent implements OnInit {
    @Input() links: { [key: string]: string[] };
    
    @Input() public menuAction: string;
    @Output() menuActionClicked = new EventEmitter<any>();

    constructor() { }

    ngOnInit() { }

    public keepOrder = (a, b) => {
        return a;
    }

    public menuActionClick(){
        var elmemnts = document.getElementsByClassName("is-active");
        let value =(<HTMLLinkElement>elmemnts[0]).id;        
        value = value.replace("./", "");
        this.menuActionClicked.emit(value);
    }
}