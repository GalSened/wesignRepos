import { User } from "../models/user.model";

export interface IAppState {
    appState: AppState;
}

export interface AppState {
    Token: string;
    RefreshToken: string;
    IsLoggedIn: boolean;
    IsActivated : boolean;
    IsGhostUser : boolean;
    IsDevUser : boolean;
    UserEmail: string;
    ShouldShowCompanyForm : boolean;
    ShouldShowUserForm : boolean;
    ShouldShowProgramForm : boolean;
}

export enum AlertLevel {
    NONE = -1,
    SUCCESS = 0,
    ERROR = 1,
}