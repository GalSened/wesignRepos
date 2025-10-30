import { AppConfigService } from '@services/app-config.service';
import { Component, OnDestroy, OnInit, ViewChild, ViewEncapsulation } from "@angular/core";
import { FormControl, FormGroup, NgForm, Validators } from "@angular/forms";
import { Router } from "@angular/router";
import { LoginResult } from '@models/Users/login-result.model';
import { Store } from "@ngrx/store";
import { UserApiService } from "@services/user-api.service";
import * as actions from "@state/actions/app.actions";
import { IAppState } from "@state/app-state.interface";
import { Subscription } from "rxjs";
import { Errors } from '@models/error/errors.model';
import { TranslateService } from "@ngx-translate/core";
import { ConfigurationApiService } from '@services/configuration-api.service';
import { environment } from "../../../environments/environment"
import { Signer1Credential } from '@models/self-sign-api/signer-authentication.model';
import { SharedService } from '@services/shared.service';

@Component({

    selector: "sgn-login",
    templateUrl: "login.component.html",
})

export class LoginComponent implements OnInit, OnDestroy {
    public email: string;
    public password: string;
    public persistent: boolean = false;
    public errorMessage: string;
    public isBusy: boolean = false;
    public SSOisBusy: boolean = false;
    public enableRegister: boolean = false;
    public stateSubs: Subscription;
    public loginSubs: Subscription;
    public eyeIcon: string = "eye";
    public tiny = environment.tiny;
    public SSO: boolean = false;
    public ShowSSOOnly: boolean = false;
    public otpToken: string = "";
    public otpFlow: boolean = false;
    public expiredPassToken: string = "";
    public expiredPassFlow: boolean = false;
    @ViewChild("f", { static: true })
    public f: NgForm;

    constructor(
        private userApiService: UserApiService,
        private configurationApiService: ConfigurationApiService,
        private store: Store<IAppState>,
        private router: Router,
        private translate: TranslateService,
        private appConfigService: AppConfigService,
        private sharedService: SharedService
    ) {
    }

    public ngOnInit() {
        this.f.form = new FormGroup({
            email: new FormControl(''),
            password: new FormControl('', [Validators.pattern("^(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).*$")]),
        });
        
        this.stateSubs = this.store.select<any>('appstate').subscribe((state: any) => {
            if (state.IsLoggedIn) {
                this.router.navigate(["dashboard"]);
            }
        });
        this.configurationApiService.readInitConfiguration().subscribe(
            (data) => {
                this.ShowSSOOnly = data.enableShowSSOOnlyInUserUI;
                this.enableRegister = data.enableFreeTrailUsers
               

                if (data.shouldUseSignerAuth && !this.tiny) {
                    this.SSO = true;
                    this.store.dispatch(new actions.SignerAuthAction());
                }
                if (data.shouldUseSignerAuth && data.shouldUseSignerAuthDefault) {
                    this.store.dispatch(new actions.SignerAuthDefaultAction());
                }
            },
            (error) => {

            }
        )
    }


    onEnter() {
        this.doLogin();
    }

    public onLogoClicked() {
        this.clearLoginForm();
        this.expiredPassFlow = false;
    }

    public doLogin() {
        if (!this.f.form.valid) {
            //this.errorMessage = "ERROR.INPUT.E_CRED";
            return;
        }
        if (this.ShowSSOOnly) {
            return;
        }

        this.isBusy = true;

        this.loginSubs = this.userApiService.login(this.email, this.password, this.persistent).subscribe(
            (data: LoginResult) => {
                this.userApiService.accessToken = data.token;
                this.userApiService.refreshAccessToken = data.refreshToken;
                this.userApiService.authToken = data.authToken;
                if (data.authToken == "OTP" && data.refreshToken != "" && data.token == "") {
                    setTimeout(() => {
                        this.otpToken = data.refreshToken;
                        this.otpFlow = true;
                    }, 200);

                }
                else if(data.authToken == "EXPIRED_PASSWORD" && data.refreshToken != "" && data.token == "") {
                    setTimeout(() => {
                        this.expiredPassToken = data.refreshToken;
                        this.expiredPassFlow = true;
                    }, 200);
                }
                else {
                    this.store.dispatch(new actions.LoginAction({ Token: data.token, RefreshToken: data.refreshToken, AuthToken: data.authToken }));
                    if (this.userApiService.authToken != "" && this.userApiService.authToken != null) {
                        let auth = new Signer1Credential();
                        auth.signerToken = this.userApiService.authToken;
                        this.store.dispatch(new actions.SetSignerAuthAction({ signerAuth: auth }));
                    }
                    this.router.navigate(["dashboard"]);
                }
            }, (err) => {
                let result = new Errors(err.error);
                this.translate.get(result.errors && result.errors.status === 2 ? "ERROR.INPUT.E_CRED" : `SERVER_ERROR.${result.errorCode}`)
                    .subscribe(error => {
                        this.errorMessage = error;
                    });
                this.isBusy = false;
            },
            () => {
                this.isBusy = false;
            }
        );
    }

    public togglePersistent() {
        this.persistent = !this.persistent;
        //console.log(this.persistent);
    }
    public SSOLogin() {
        this.SSOisBusy = true;
        window.location.href = this.appConfigService.SSOLogin;
    }

    public onPasswordChangeTimeExpired() {
        this.clearLoginForm();
        this.expiredPassFlow = false;
    }

    public onPasswordChanged() {
        this.clearLoginForm();
        this.expiredPassFlow = false;
        this.sharedService.setSuccessAlert(this.translate.instant(`ERROR.OPERATION.4`));
    }

    private clearLoginForm() {
        this.errorMessage = "";
        this.password = undefined;
        this.f.resetForm();
    }

    public ngOnDestroy() {
        if (this.stateSubs != null) {
            this.stateSubs.unsubscribe();
        }
        if (this.loginSubs != null) {
            this.loginSubs.unsubscribe();
        }
    }

}
