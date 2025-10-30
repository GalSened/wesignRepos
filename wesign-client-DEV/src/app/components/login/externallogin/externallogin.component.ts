import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { Store } from '@ngrx/store';
import { UserApiService } from '@services/user-api.service';
import { IAppState } from '@state/app-state.interface';
import * as actions from "@state/actions/app.actions";
import { Signer1Credential } from '@models/self-sign-api/signer-authentication.model';
import { LoginResult } from '@models/Users/login-result.model';


@Component({
  selector: 'sgn-externallogin',
  templateUrl: './externallogin.component.html',
  styles: []
})
export class ExternalloginComponent implements OnInit {

  public tiny : boolean = false;
  
  constructor(private activatedRoute: ActivatedRoute, private router: Router,
    private userApiService: UserApiService, private store: Store<IAppState>) {
    this.activatedRoute.params.subscribe((params) => {
         if(params.guid)
         {
         this.userApiService.externalLogin(params.guid).subscribe(
          (data: LoginResult) => {
            this.userApiService.accessToken = data.token;
            this.userApiService.refreshAccessToken = data.refreshToken;
            this.userApiService.authToken = data.authToken;

            this.store.dispatch(new actions.LoginAction({ Token: data.token, RefreshToken: data.refreshToken, AuthToken : data.authToken }));
            if(this.userApiService.authToken != "" && this.userApiService.authToken != null)
            {
              let auth = new Signer1Credential();
              auth.signerToken = this.userApiService.authToken;
              this.store.dispatch(new actions.SetSignerAuthAction({ signerAuth: auth }));
            }
            this.router.navigate(["dashboard"]);
        }, (err) => {
          this.router.navigate(["/login"]);
        },
        () => {
           
        });
        }
        else
        {
          this.router.navigate(["/login"]);
        }
    });
  
}

  ngOnInit() {
  }

}
