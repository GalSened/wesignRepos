import { Component, OnInit } from "@angular/core";
import { Actions, ofType } from "@ngrx/effects";
import * as alertActions from "@state/actions/alert.actions";
import { AlertLevel } from "@state/app-state.interface";

@Component({
    selector: "sgn-alert-component",
    template: `
        <ng-container *ngIf="IsShown">
            <div *ngIf="CurrentAlertLevel != AlertLevel.NONE" [class.is-error]="CurrentAlertLevel == AlertLevel.ERROR"
            [class.is-confirm]="CurrentAlertLevel == AlertLevel.SUCCESS" class="ct-c-alert ct-animate-fadein">
                <span>{{Message}}</span>
                <button class="ct-button--icon ct-float-right" (click)="hideAlert()">
                <i-feather name="x"></i-feather>
                </button>
            </div>
        </ng-container>
    `,

//     <div class="ws_alert ws_text-center ws_is-shown__slide-down ws_animate-fadein"
//     *ngIf="CurrentAlertLevel != AlertLevel.NONE"
//     [class.alert--error-full]="CurrentAlertLevel == AlertLevel.ERROR"
//     [class.alert--confirm]="CurrentAlertLevel == AlertLevel.SUCCESS">
//     <h4>{{Message}}</h4>
//     <button class="ws_button--icon ws_float-right" (click)="hideAlert()">
//         <div class="ws_icon__remove--white"></div>
//     </button>
// </div>
})

export class AlertComponent implements OnInit {

    public AlertLevel = AlertLevel;
    public IsShown: boolean = false;
    public CurrentAlertLevel: AlertLevel;
    public Message: string;

    constructor(private actions$: Actions) { }

    public ngOnInit() {
        this.actions$.pipe(
            ofType(alertActions.SET_ALERT),
        ).subscribe((action: alertActions.SetAlertAction) => {
            this.IsShown = true;
            this.CurrentAlertLevel = action.payload.Level;
            this.Message = action.payload.Message;
            if(action.payload.ShouldAutoHide ){
                setTimeout(() => {
                    this.IsShown = false;
                }, 6000);
            }
        });
    }

    public hideAlert = () => { this.IsShown = false; }
}
