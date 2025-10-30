import { Action } from '@ngrx/store';
import { AppState } from './app-state.interface';
import { LoginAction } from './app.action';

const initialState: AppState = {
    IsLoggedIn: false,
    Token: "",
    RefreshToken: "",
    IsActivated : false, 
    IsGhostUser : false,
    IsDevUser : false,
    UserEmail : "",
    ShouldShowProgramForm : false,
    ShouldShowUserForm : false,
    ShouldShowCompanyForm : false
};

export function appStateReducer(state: AppState = initialState,
    action: Action ){

    switch (action.type) {
        case "[Login] Login":
            return {
                ...state,
                IsLoggedIn: true,
                IsActivated: false,
                IsGhostUser: false,
                IsDevUser : false,
                UserEmail : (action as LoginAction).payload.Email,
                ShouldShowProgramForm : false,
                ShouldShowUserForm : false,
                ShouldShowCompanyForm : false,
                Token: (action as LoginAction).payload.Token,
                RefreshToken: (action as LoginAction).payload.RefreshToken
            };
        case "[Login] GhostLogin":
            return {
                ...state,
                IsLoggedIn: true,
                IsActivated: false,
                IsGhostUser: true,
                IsDevUser : false,
                UserEmail : (action as LoginAction).payload.Email,
                Token: (action as LoginAction).payload.Token,
                RefreshToken: (action as LoginAction).payload.RefreshToken
            };
        case "[Login] DevLogin":
            return {
                ...state,
                IsLoggedIn: true,
                IsActivated: false,
                IsGhostUser: false,
                IsDevUser : true,
                UserEmail : (action as LoginAction).payload.Email,
                Token: (action as LoginAction).payload.Token,
                RefreshToken: (action as LoginAction).payload.RefreshToken
            };
        case "[Login] Logout":
            return initialState;
        case "[License] Activate":
            return {
                ...state,
                IsLoggedIn: true,
                IsActivated: true,
                Token: (action as LoginAction).payload.Token,
                RefreshToken: (action as LoginAction).payload.RefreshToken
            };
        case "[Form] HideProgramForm":
            return{
                ...state,
                ShouldShowCProgramForm: false
            };
        case "[Form] HideUserForm":
            return{
                ...state,
                ShouldShowUserForm: false
            };
            case "[Form] HideCompanyForm":
        return{
            ...state,
            ShouldShowCompanyForm: false
        };
        default:
            return state;
    }
}
