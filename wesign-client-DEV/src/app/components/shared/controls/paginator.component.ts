import { Component, EventEmitter, Input, OnDestroy, OnInit, Output } from "@angular/core";
import { Store } from '@ngrx/store';
import { AppState, IAppState } from '@state/app-state.interface';
import { Subscription } from 'rxjs';
import { LangList } from '../languages.list';

@Component({
    selector: "[paginator]",

    template: `
        <button class="ct-button--icon"  (click)="toPrevPage()">    
            <i-feather name="chevron-left"></i-feather>
        </button>
        
        <ng-container *ngIf="!this.IsRtl" >
            <input type="number" [(ngModel)]="currentPage"  (change)="changeManualy()" >

            <span> / </span><span>{{last}}</span>
        </ng-container>

        <ng-container *ngIf="this.IsRtl" >
        
            <span>{{last}}</span>
            <span> / </span>
            <input type="number" [(ngModel)]="currentPage" (change)="changeManualy()">
            </ng-container>
        <button class="ct-button--icon" (click)="toNextPage()">
            <i-feather name="chevron-right"></i-feather>
        </button>       
`

})

export class PaginatorComponent implements OnInit, OnDestroy{

    @Input()
    public currentPage: number = 1;

    @Input()
    public last: number = 1;

    @Output()
    public changed: EventEmitter<number> = new EventEmitter<number>();
    private createSub: Subscription;
    public IsRtl : Boolean = false;
    constructor(   private store: Store<IAppState>,) {
        /* TODO */
        this.last = this.last < 1 ? 1 : this.last;
        
    }
    ngOnDestroy(): void {
        if(this.createSub){
            this.createSub.unsubscribe();
        }
    }
    ngOnInit(): void {
        this.createSub =  this.store.select<any>('appstate').subscribe((state: AppState) =>{            
            if( state)
            {
                this.IsRtl = state.IsRtl
            }
        });
    }
    private FixIfNeedded()
    {
        if(this.currentPage  > this.last)
        {
            this.currentPage  = this.last;
        }
        if(this.currentPage  < 0)
        {
            this.currentPage  = 1;
        }
    }

    public toPrevPage() {
        if (this.currentPage > 1) {
            this.currentPage -= 1;
            this.FixIfNeedded();
            this.changed.emit(this.currentPage);
        }
    }

    public toNextPage() {
        if (this.currentPage < this.last) {
            this.currentPage += 1;
            this.FixIfNeedded();
            this.changed.emit(this.currentPage);
        }
    }

    public changeManualy()
    {
        if(this.currentPage <= 0 )
        {
            this.currentPage  = 1;
            this.FixIfNeedded();
            this.changed.emit(this.currentPage);
        }
        else if (this.currentPage < this.last) {
            this.FixIfNeedded();
            this.changed.emit(this.currentPage);
        }
        else if(this.currentPage >= this.last)
        {
            this.currentPage  = this.last;
            this.FixIfNeedded();
            this.changed.emit(this.currentPage);
        }
        
    }
}
