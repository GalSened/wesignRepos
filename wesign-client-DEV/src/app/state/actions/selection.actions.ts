import { TemplateInfo } from "@models/template-api/template-info.model";
import { Action } from "@ngrx/store";

export const SELECT_SIGNER_CLASS_ID = "[Selection] Select signer class id";
export class SelectSignerClassId implements Action {
    public readonly type = SELECT_SIGNER_CLASS_ID;
    constructor(readonly payload: {classId: string}) {}
}

export const SELECT_TEMPLATE = "[Selection] Select template";
export class SelectTemplateAction implements Action {
    public readonly type = SELECT_TEMPLATE;
    constructor(readonly payload: {templateInfo: TemplateInfo}) {}
}

export const UNSELECT_TEMPLATE = "[Selection] Unselect template";
export class UnselectTemplateAction implements Action {
    public readonly type = UNSELECT_TEMPLATE;
    constructor(readonly payload: {templateInfo: TemplateInfo}) {}
}

export const MOVE_TEMPLATE = "[Selection] Move template";
export class MoveTemplateAction implements Action {
    public readonly type = MOVE_TEMPLATE;
    constructor(readonly payload: {index: number, direction: number}) {}
}


export const CLEAR_TEMPLATE_SELECTION = "[Selection] Clear template selection";
export class ClearTemplateSelectionAction implements Action {
    public readonly type = CLEAR_TEMPLATE_SELECTION;
    constructor(readonly payload: {}) {}
}

export type ACTIONS = SelectSignerClassId | SelectTemplateAction | UnselectTemplateAction | MoveTemplateAction | ClearTemplateSelectionAction;
