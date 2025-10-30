import { Action } from "@ngrx/store";

export const TEXT_CHANGED = "[Sync] Set alert";
export class TextChangedAction implements Action {
    public readonly type = TEXT_CHANGED;
    constructor(readonly payload: { Description: string; Text: string; }) {}
}

export type ACTIONS = TextChangedAction;
