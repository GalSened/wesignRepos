import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Router, RoutesRecognized } from "@angular/router";
import { JwtHelperService } from "@auth0/angular-jwt";
import { ILanguage, LangList } from '@components/shared/languages.list';
import { User } from "@models/account/user.model";
import { BaseResult } from "@models/base/base-result.model";
import { UpdatePasswordRequest } from "@models/Users/update-password-request.model";
import { LoginResult } from "@models/Users/login-result.model";
import { SignUp } from "@models/Users/sign-up.model";
import { Store } from "@ngrx/store";
import { TranslateService } from "@ngx-translate/core";
import * as actions from "@state/actions/app.actions";
import { IAppState } from "@state/app-state.interface";
import { of, throwError } from 'rxjs';
import { catchError, filter, pairwise, tap } from "rxjs/operators";
import { ChangePasswordRequest } from '@models/Users/change-password-request.model';
import { AppConfigService } from './app-config.service';
import { UserStatus } from '@models/account/user-status.model';
import { RedirectLinkResult } from '@models/Users/redirect-link-result.model';
import { LogoutResult } from '@models/Users/logout-result.model';
import { groupsResponse } from '@models/managment/groups/management-groups.model';
import { updatePhoneModal } from '@models/Users/update-user-phone.modal';


@Injectable({ providedIn: 'root' })
export class UserApiService {

    private readonly JWT_TOKEN: string = 'JWT_TOKEN';
    private readonly REFRESH_TOKEN: string = 'REFRESH_TOKEN';
    private readonly AUTH_TOKEN: string = 'AUTH_TOKEN';
    public static readonly FREE_TRAIL_COMPANY_ID: string = "00000000-0000-0000-0000-000000000001";


    public get accessToken(): string {
        return this.storage.getItem(this.JWT_TOKEN) || localStorage.getItem(this.JWT_TOKEN);
    }

    public set accessToken(value: string) {
        if (value)
            this.storage.setItem(this.JWT_TOKEN, value);
    }

    public get refreshAccessToken(): string {
        return this.storage.getItem(this.REFRESH_TOKEN) || localStorage.getItem(this.REFRESH_TOKEN);
    }

    public set refreshAccessToken(value: string) {
        if (value)
            this.storage.setItem(this.REFRESH_TOKEN, value);
    }

    public get authToken(): string {
        if (localStorage.getItem(this.AUTH_TOKEN) === null && this.storage.getItem(this.AUTH_TOKEN) === null ) {
            return "";
          }
        return this.storage.getItem(this.AUTH_TOKEN) || localStorage.getItem(this.AUTH_TOKEN);
    }

    public set authToken(value: string) {
        if (value)
            this.storage.setItem(this.AUTH_TOKEN, value);

    }



    public storage: Storage = sessionStorage || localStorage;

    private userApi: string = "";

    private languages: ILanguage[] = LangList;

    constructor(private httpClient: HttpClient,
        private store: Store<IAppState>,
        private router: Router,
        private translate: TranslateService,
        private appConfigService: AppConfigService
    ) {
        this.userApi = this.appConfigService.apiUrl + "/users";

        if (this.accessToken) {
            if (!this.isTokenExpired(this.accessToken)) {
                store.dispatch(new actions.LoginAction({ Token: this.accessToken, RefreshToken: this.refreshAccessToken, AuthToken: this.authToken }));

                if (!window.location.href.includes("/oauth?code=") ) {
                    
                    
                
                if (!window.location.hash.substr(1).startsWith("/dashboard")) {
                    this.router.navigate(["dashboard"]);
                }
            }
            }
        }

        setInterval(() => {
            if (this.accessToken) {
                if (this.isTokenExpired(this.accessToken) && window.location.href.indexOf("/login") == -1) {
                    store.dispatch(new actions.LogoutAction());
                    this.removeTokens();
                    this.router.navigate(["login"]);
                }
            }
        }, 1000);

        this.router.events
            .pipe(
                filter((e: any) => e instanceof RoutesRecognized),
                pairwise(),
            ).subscribe((e: RoutesRecognized[]) => {
                sessionStorage.setItem("prev.url", e[0].url);
            });
    }


    public externalLogin(token: string) {
        return this.httpClient.post<LoginResult>(`${this.userApi}/externalLogin`,
            { token: token });
    }


    public GetUserGroups()
    {
        return this.httpClient.get<groupsResponse>(`${this.userApi}/groups`);
    }

    public SwitchGroup( groupId: string)
    {
        return this.httpClient.post<LoginResult>(`${this.userApi}/SwitchGroup/${groupId}`,{});
    }


    public login(email: string, password: string, persistent: boolean) {

        if (persistent) {
            this.storage = localStorage;
        } else {
            this.storage = sessionStorage;
        }
        return this.httpClient.post<LoginResult>(`${this.userApi}/login`,
            { Email: email, Password: password });
    }

    public resetPassword(email: string) {
        return this.httpClient.post<LoginResult>(`${this.userApi}/password`, { Email: email });
    }

    public resendOtp(token : string) {
        return this.httpClient.post<LoginResult>(`${this.userApi}/resendOtp`, { OtpToken: token });
    }

    public validateOtpflow(token : string,otpCote : string) {
        return this.httpClient.post<LoginResult>(`${this.userApi}/validateOtpflow`, { OtpToken: token, Code: otpCote });
    }

    public validateExpiredPasswordFlow(token: string, oldPassword: string, newPassword: string) {
        const updatePasswordRequest = new UpdatePasswordRequest();
        updatePasswordRequest.RenewPasswordToken = token;
        updatePasswordRequest.NewPassword = newPassword;
        updatePasswordRequest.OldPassword = oldPassword;
        return this.httpClient.post(`${this.userApi}/validateExpiredPasswordFlow`, updatePasswordRequest);
    }

    public updatePassword(password: string, token: string) {
        const updatePasswordRequest = new UpdatePasswordRequest();
        updatePasswordRequest.NewPassword = password;
        updatePasswordRequest.RenewPasswordToken = token;

        return this.httpClient.put<LoginResult>(`${this.userApi}/password`, updatePasswordRequest);
    }

    public changePassword(request: ChangePasswordRequest) {
        return this.httpClient.post<BaseResult>(`${this.userApi}/change`, request);
    }

    public signUp(request: SignUp) {
        return this.httpClient.post<LoginResult>(`${this.userApi}`, request);
    }

    public logoutServer() {
        
            return this.httpClient.get<LogoutResult>(`${this.userApi}/Logout`);
       
    }
    public activate(token: string) {
        return this.httpClient.put<LoginResult>(`${this.userApi}/activation`, { Token: token });
    }

    public logout() {
        if(this.authToken != "" && this.authToken != undefined)
        {
          this.logoutServer().subscribe({
            next:(result) =>{
                this.removeTokens();
                this.store.dispatch(new actions.LogoutAction());
                if(result.logoutURL.trim() != '')
                {
                    window.location.href = result.logoutURL;
                }
                else
                {
                    this.redirectForLogin();
                }
            },
            error:(err) => {
                this.removeTokens();
                this.store.dispatch(new actions.LogoutAction());
                    this.redirectForLogin();
            }
                    });
        }
        else
        {
            this.removeTokens();
            this.store.dispatch(new actions.LogoutAction());
            this.redirectForLogin();
        }
        
      
    }

    public redirectForLogin() {
        
        this.router.navigate(["login"]);
    }

    public resendActivationLink(email: string) {
        return this.httpClient.post<LoginResult>(`${this.userApi}/activation`, { Email: email });
    }

    public unSubscribeUser() {
        return this.httpClient.post<any>(`${this.userApi}/unsubscribeuser`, {});
    }

    public ChangePaymentRole() {
        return this.httpClient.post<RedirectLinkResult>(`${this.userApi}/changepaymentrule`, {});
    }
    public getCurrentUser(updateLanguage: boolean = false) {
        return this.httpClient.get<User>(`${this.userApi}`)
            .pipe(
                tap(r => { if (updateLanguage) { this.setUserLanguage(r.userConfiguration.language); } })

            );
    }

    public setUserLanguage(userLanguage: number) {
        let lang = this.languages.find(a => a.Enum == userLanguage);
        this.translate.use(lang.Code);
        document.getElementsByTagName("html")[0].setAttribute("dir", lang.Code === "he" ? "rtl" : "ltr");
        this.store.dispatch(new actions.SetLangAction({ Language: lang.Code, IsRtl: lang.IsRtl }));
    }

    public updateUser(user: User) {
        return this.httpClient.put<BaseResult>(`${this.userApi}`, user)
            .pipe(
                tap((_) => {
                    this.setUserLanguage(user.userConfiguration.language);
                }),
            );
    }


    public updatePhoneStartProcess(phone: string) {
        let updatePhoneRequest  = new updatePhoneModal();
        updatePhoneRequest.phoneNumber = phone;
        return this.httpClient.post<BaseResult>(`${this.userApi}/updatePhone`, updatePhoneRequest);
    }

    public updatePhoneValidateOtp(code: string) {
        let updatePhoneRequest  = new updatePhoneModal();
        updatePhoneRequest.code = code;
        return this.httpClient.post<BaseResult>(`${this.userApi}/UpdatePhoneValidateOtp`, updatePhoneRequest);
    }
    public updateToken() {
        if (!this.accessToken || !this.refreshAccessToken) {
            this.logout();
            this.redirectForLogin();
            return of([]);
        }
        return this.httpClient.post<any>(`${this.userApi}/refresh`, { JwtToken: this.accessToken, RefreshToken: this.refreshAccessToken, AuthToken: this.authToken })
            .pipe(tap((tokens: any) => {
                this.accessToken = tokens.token;
            }),
                catchError((error) => {
                    this.logout();
                    this.redirectForLogin();
                    return throwError(error);
                }));
    }

    public async getUserProgramStatus(): Promise<UserStatus> {
        let result: UserStatus = new UserStatus();

        const data = await this.getCurrentUser(false).toPromise();
        var date = new Date();
        const currentUTCTime = new Date(date.getTime() + date.getTimezoneOffset() * 60000);
        const expiredDate = new Date(data.program.expiredTime);
        result.isExpired = currentUTCTime.getTime() > expiredDate.getTime();
        result.isFreeTrial = data.companyId == UserApiService.FREE_TRAIL_COMPANY_ID;
        result.remeiningDocs = data.program.remainingDocumentsForMonth;
        return result;
    }


    private isTokenExpired(token: string): boolean {
        try {
            let isExpired = false;
            const helper = new JwtHelperService();
            const duration = helper.decodeToken(token).duration;
            const currentTime = (new Date().getTime() / 1000);

            if (currentTime > duration) {
                isExpired = true;
            }

            return isExpired;
        } catch (e) {
            this.accessToken = null;
        }
        return true;
    }

    public removeTokens() {
        sessionStorage.removeItem(this.JWT_TOKEN);
        localStorage.removeItem(this.JWT_TOKEN);
        localStorage.removeItem(this.JWT_TOKEN);

        sessionStorage.removeItem(this.REFRESH_TOKEN);
        localStorage.removeItem(this.REFRESH_TOKEN);

        localStorage.removeItem(this.AUTH_TOKEN);
        sessionStorage.removeItem(this.AUTH_TOKEN);
    }
}
