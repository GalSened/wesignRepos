import { environment } from 'src/environments/environment';
import { Router, RoutesRecognized } from "@angular/router";
import { HttpClient } from "@angular/common/http";
import { Store } from "@ngrx/store";
import { IAppState } from '../state/app-state.interface';
import { JwtHelperService } from "@auth0/angular-jwt";
import { catchError, filter, pairwise, tap } from "rxjs/operators";
import { LoginResult } from '../models/results/login-result.model';
import { Injectable } from '@angular/core';
import { Filter } from '../models/filter.model';
import { UsersResult } from '../models/results/users-result.model';
import { throwError, of } from 'rxjs';
import { LogoutAction, LoginAction, GhostLoginAction } from '../state/app.action';
import { AppConfigService } from './app-config.service';
import { AddUser, UpdateUserModel } from '../models/add-user';
import { UpdateUserType } from '../models/update-user-type.model';
import { CreateHtmlTemplate } from '../models/create-html-template.model';


@Injectable({ providedIn: 'root' })
export class UsersApiService {
    private readonly JWT_TOKEN: string = 'JWT_TOKEN';
    private readonly REFRESH_TOKEN: string = 'REFRESH_TOKEN';
    // private readonly LICENSE_ACTIVATION: string = 'LICENSE_ACTIVATION';

    private usersApi: string = this.appConfigService.apiUrl + "/Users";
    public storage: Storage = localStorage || sessionStorage;

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



    constructor(private httpClient: HttpClient,
        private appConfigService: AppConfigService,
        private store: Store<IAppState>,
        private router: Router) {

        if (this.accessToken && !this.isTokenExpired(this.accessToken)) {
            store.dispatch(new LoginAction({ Token: this.accessToken, RefreshToken: this.refreshAccessToken , Email: ""}));

            if (!window.location.hash.substr(1).startsWith("/dashboard")) {
                this.router.navigate(["dashboard", "companies"]);
            }
        }
        setInterval(() => {
            if (this.accessToken && this.isTokenExpired(this.accessToken)) {
                store.dispatch(new LogoutAction());
                this.removeTokens();
                this.router.navigate(["login"]);
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

    public setApiUsers(baseUrl: string) {
        this.usersApi = baseUrl;
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

    private removeTokens() {
        sessionStorage.removeItem(this.JWT_TOKEN);
        localStorage.removeItem(this.JWT_TOKEN);

        sessionStorage.removeItem(this.REFRESH_TOKEN);
        localStorage.removeItem(this.REFRESH_TOKEN);
    }

    public login(userEmail: string, password: string) {

        return this.httpClient.post<LoginResult>(`${this.usersApi}/login`,
            { Email: userEmail, Password: password });
    }

    public Read(filter: Filter) {
        return this.httpClient.get<UsersResult>(`${this.usersApi}?key=${filter.key}&` +
            `offset=${filter.offset}&limit=${filter.limit}`, { observe: "response" });
    }
    public ReadAddUsersInCompany(companyId : string) {
        return this.httpClient.get<UsersResult>(`${this.usersApi}/UsersCompany/${companyId}`, { observe: "response" });
    }

    public create(user: AddUser, password: string) {
        if (user.UserType != 4) {
            return this.httpClient.post(`${this.usersApi}`, user);
        } else {
            return this.httpClient.post(`${this.usersApi}`, { 'UserType': user.UserType, 'UserEmail': user.userEmail, 'Password': password });
        }
    }

    update(id: string, updateUserModel: UpdateUserModel) {
        return this.httpClient.put(`${this.usersApi}/${id}`, updateUserModel);
    }

    delete(userId: string) {
        return this.httpClient.delete(`${this.usersApi}/${userId}`);
    }

    public resetPassword(newPassword: string) {
        return this.httpClient.put(`${this.usersApi}`, { NewPassword: newPassword });
    }

    public logout() {
        this.removeTokens();
        this.store.dispatch(new LogoutAction());
        this.router.navigate(["login"]);
    }

    public updateToken() {
        if (!this.accessToken || !this.refreshAccessToken) {
            this.logout();
            return of([]);
        }
        return this.httpClient.post<any>(`${this.usersApi}/refresh`, { JwtToken: this.accessToken, RefreshToken: this.refreshAccessToken })
            .pipe(tap((tokens: any) => {
                this.accessToken = tokens.token;
            }),
                catchError((error) => {
                    this.logout();
                    return throwError(error);
                }));
    }

    public resendResetPassword(userId: string) {
        return this.httpClient.get(`${this.usersApi}/password/${userId}`);    
    }

    public readTemplates(userId: string){
        return this.httpClient.get(`${this.usersApi}/templates/${userId}`);    
    }

    public createHtmlTemplate(request : CreateHtmlTemplate){
        return this.httpClient.post<any>(`${this.usersApi}/templates`, request);
    }

}