import { Action } from "@ngrx/store";

export const STYLE_HEADER_CLASSES = "[Style] Set header classes";
export class StyleHeaderClassesAction implements Action {
    public readonly type = STYLE_HEADER_CLASSES;
    constructor(readonly payload: { Classes: string; }) { }
}

export type ACTIONS = StyleHeaderClassesAction;
