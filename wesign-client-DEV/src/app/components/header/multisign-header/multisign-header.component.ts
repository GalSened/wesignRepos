import { Component } from "@angular/core";
import { Store } from "@ngrx/store";

import { FLOW_STEP } from "@models/enums/flow-step.enum";
import { IAppState, AppState } from "@state/app-state.interface";
import { Observable } from "rxjs";

@Component({
    selector: "sgn-multisign-header",
    templateUrl: "multisign-header.component.html",
})

export class MultisignHeaderComponent {

    public appState$: Observable<AppState>;

    public FLOW_STEP = FLOW_STEP;

    constructor(private store: Store<IAppState>) {
        this.appState$ = this.store.select<any>('appstate');
    }
}
