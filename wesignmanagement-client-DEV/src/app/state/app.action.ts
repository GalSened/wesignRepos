import { Action } from "@ngrx/store";

export const LOGIN: string = "[Login] Login";

export class LoginAction implements Action {
    public readonly type = LOGIN;
    constructor(readonly payload: { Token: string, RefreshToken: string , Email: string }) {
    }
}

export const GHOST_LOGIN: string = "[Login] GhostLogin";

export class GhostLoginAction implements Action {
    public readonly type = GHOST_LOGIN;
    constructor(readonly payload: { Token: string, RefreshToken: string , Email: string}) {
    }
}

export const Dev_LOGIN: string = "[Login] DevLogin";

export class DevLoginAction implements Action {
    public readonly type = Dev_LOGIN;
    constructor(readonly payload: { Token: string, RefreshToken: string, Email: string }) {
    }
}

export const LOGOUT: string = "[Login] Logout";

export class LogoutAction implements Action {
    public readonly type = LOGOUT;
    public payload: {};
}

export const License: string = "[License] Activate";

export class ActivateLicenseAction implements Action {
    public readonly type = License;
    constructor(readonly payload: { Token: string, RefreshToken: string }) {
    }
}

export const HideProgramForm: string = "[Form] HideProgramForm";

export class HideProgramFormAction implements Action {
    public readonly type = HideProgramForm;
    constructor() {
    }
}

export const HideUserForm: string = "[Form] HideUserForm";

export class HideUserFormAction implements Action {
    public readonly type = HideUserForm;
    constructor() {
    }
}

export const HideCompanyForm: string = "[Form] HideCompanyForm";

export class HideCompanyFormAction implements Action {
    public readonly type = HideCompanyForm;
    constructor() {
    }
}

export type ACTIONS = LoginAction | LogoutAction | ActivateLicenseAction | GhostLoginAction | HideProgramFormAction | HideUserFormAction | HideCompanyFormAction;
