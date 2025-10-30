
import { Component, ViewEncapsulation } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { Store } from "@ngrx/store";
import { UserApiService } from "@services/user-api.service";
import * as actions from "@state/actions/app.actions";
import { IAppState } from "@state/app-state.interface";
import { Signer1Credential } from '@models/self-sign-api/signer-authentication.model';
import { LoginResult } from '@models/Users/login-result.model';

@Component({
    selector: "sgn-activate",
    templateUrl: "activate.component.html",   
})

export class ActivateComponent {
    constructor(private userApiService: UserApiService, private activatedRoute: ActivatedRoute,
        private store: Store<IAppState>, private router: Router) {

        this.activatedRoute.params.subscribe((data) => {
            this.userApiService.activate(data.guid).subscribe((res: LoginResult) => {
                this.userApiService.accessToken = res.token;
                this.userApiService.refreshAccessToken = res.refreshToken;
                this.userApiService.authToken = res.authToken;
                this.store.dispatch(new actions.LoginAction({ Token: res.token, RefreshToken: res.refreshToken, AuthToken : res.authToken }));

                if(this.userApiService.authToken != "" && this.userApiService.authToken != null)
                {
                    let auth = new Signer1Credential();
                    auth.signerToken = this.userApiService.authToken;
                    this.store.dispatch(new actions.SetSignerAuthAction({ signerAuth: auth }));
                }
                
                this.router.navigate(["dashboard"]);
            }, (err) => {
                this.router.navigate(["login"]);
            }
            );
        });
    }
}
