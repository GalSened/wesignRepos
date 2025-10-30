import { Component, OnInit } from "@angular/core";
import { FLOW_STEP } from "@models/enums/flow-step.enum";
import { Store } from "@ngrx/store";
import { IAppState, AppState } from "@state/app-state.interface";
import { Observable } from "rxjs";

@Component({
    selector: "sgn-selfsign-header",
    templateUrl: "selfsign-header.component.html",
})

export class SelfSignHeaderComponent {

    public appState$: Observable<AppState>;

    public FLOW_STEP = FLOW_STEP;

    constructor(private store: Store<IAppState>) {
        this.appState$ = this.store.select<any>('appstate');
    }
}
