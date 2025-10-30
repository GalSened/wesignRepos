import { Component, OnInit } from "@angular/core";
import { Store } from "@ngrx/store";
import { IAppState, AppState } from "@state/app-state.interface";
import { Observable } from "rxjs";

@Component({
    selector: "sgn-header-component",
    templateUrl: "header.component.html",
})

export class HeaderComponent implements OnInit {

    //public showHeaderMenu: boolean = false;

    public appState$: Observable<AppState>;

    constructor(private store: Store<IAppState>) {

        this.appState$ = this.store.select<any>('appstate');
        //this.store.select("appstate").subscribe((data: IAppState)=>{console.log(data)})
    }

    public ngOnInit() {
        /* TODO */
    }

}
