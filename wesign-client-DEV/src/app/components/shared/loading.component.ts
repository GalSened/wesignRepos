import { Component, OnInit } from "@angular/core";
import { Actions, Effect, ofType } from "@ngrx/effects";
import { Action } from "@ngrx/store";
import * as alertActions from "@state/actions/alert.actions";
import { Observable } from "rxjs";
import { tap } from "rxjs/operators";

@Component({
    selector: "sgn-loading",
    template:
        `
        <div class="ct-c-modal " *ngIf="IsBusy">
            <div class="modal__overlay">
                <div class="ct-c-loader is-full" [class.ct-is-hidden]="!IsBusy" [class.ct-is-shown]="IsBusy">
                <span>{{Message}}</span>
                <div></div>
                <div></div>
                <div></div>
                </div>
            </div>
        </div>
        `
})

export class LoadingComponent implements OnInit {

    public IsBusy: boolean = false;
    public Message: string = "";

    constructor(private actions$: Actions) { }

    public ngOnInit() {
        this.actions$.pipe(
            ofType(alertActions.SET_BUSY_STATE),
        ).subscribe((action: alertActions.SetBusyStateAction) => {
            this.IsBusy = action.payload.IsBusy;
            this.Message = action.payload.Message;
        });

    }
}
