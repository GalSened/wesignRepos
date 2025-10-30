import { Action } from "@ngrx/store";
import { AlertLevel } from "@state/app-state.interface";

export const SET_ALERT = "[Alerts] Set alert";
export class SetAlertAction implements Action {
    public readonly type = SET_ALERT;
    constructor(readonly payload: {Level: AlertLevel, Message: string; ShouldAutoHide: boolean ;}) {}
}

export const CLEAR_ALERT = "[Alerts] Clear alert";
export class ClearAlertAction implements Action {
    public readonly type = CLEAR_ALERT;
    constructor(readonly payload: { }) {}
}

export const SET_BUSY_STATE = "[State] Set Busy state";
export class SetBusyStateAction implements Action {
    public readonly type = SET_BUSY_STATE;
    constructor(readonly payload: {
        IsBusy: boolean,
        Message: string;
    }) {}
}

export const PAGE_LOADED = "[State] Page loaded";
export class PageLoadedAction implements Action {
    public readonly type = PAGE_LOADED;
}

export type ACTIONS = SetAlertAction | ClearAlertAction | SetBusyStateAction | PageLoadedAction;
