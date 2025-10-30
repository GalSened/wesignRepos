import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot } from "@angular/router";
import { Store } from "@ngrx/store";
import { UserApiService } from "@services/user-api.service";
import { IAppState, AppState } from "@state/app-state.interface";

@Injectable({ providedIn: "root" })
export class CanActivateGuard implements CanActivate {

    private appState: AppState;

    constructor(private store: Store<IAppState>, private userApiService: UserApiService, private router: Router) {
        this.store.select<any>('appstate').subscribe((state: any) => {
            this.appState = state;
        });
    }

    public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot) {

        switch (route.url[0].path) {
            case "login":
                return !this.appState.IsLoggedIn;    
            case "externallogin":        
                return !this.appState.IsLoggedIn;    
            default: {
                if (!this.appState.IsLoggedIn) {
                    //console.log("Here!");
                    this.router.navigate(["/login"]);
                }

                return this.appState.IsLoggedIn;
            }

        }

    }
}
