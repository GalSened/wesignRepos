import { CanActivate, Router, ActivatedRouteSnapshot } from "@angular/router";
import { AppState, IAppState } from './state/app-state.interface';
import { Store } from '@ngrx/store';
import { Injectable } from '@angular/core';


@Injectable({ providedIn: "root" })
export class ManagementGuard implements CanActivate {
    
  private appState: AppState;

  constructor(private store: Store<IAppState>, private router: Router) {
    this.store.select<any>('appstate').subscribe((state: any) => {
        this.appState = state;
    });
}

  
    public canActivate(route: ActivatedRouteSnapshot) {
        switch (route.url[0].path) {
            case "login":
                return !this.appState.IsLoggedIn;
            default: {
                if (!this.appState.IsLoggedIn) {
                    this.router.navigate(["/login"]);
                }

                return this.appState.IsLoggedIn ;
            }

        }
    }
  }