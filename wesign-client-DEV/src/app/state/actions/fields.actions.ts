import { SignatureType } from '@models/enums/signature-type.enum';
import { RadioField, PageField } from "@models/template-api/page-data-result.model";
import { Action } from "@ngrx/store";

export const SET_DOCUMENT_NAME = "[Fields] Set document name";
export class SetDocumentNameAction implements Action {
    public readonly type = SET_DOCUMENT_NAME;
    constructor(readonly payload: { CurrentDocumentName: string }) { }
}

export const SELECT_FIELD = "[Fields] Select field";
export class SelectField implements Action {
    public readonly type = SELECT_FIELD;
    // constructor(readonly payload: { SelectedField: string, TemplateId: string }) { }
    constructor(readonly payload: { SelectedField: { FieldName: string, TemplateId: string } }) { }
}




export const ADD_PAGE_FIELD = "[Fields] Add page field";
export class AddPageFieldAction implements Action {
    public readonly type = ADD_PAGE_FIELD;
    constructor(readonly payload: { PageField: PageField }) { }
}

export const ADD_PAGE_FIELDS = "[Fields Add page fields";
export class AddPageFieldsAction implements Action {
    public readonly type = ADD_PAGE_FIELDS;
    constructor(readonly payload: {PageFields: PageField[]}) { }
}

export const REMOVE_PAGE_FIELD = "[Fields] Remove page field";
export class RemovePageFieldAction implements Action {
    public readonly type = REMOVE_PAGE_FIELD;
    // constructor(readonly payload: { SelectedField: string, TemplateId: string }) { }
    constructor(readonly payload: { SelectedField: { FieldName: string, TemplateId: string } }) { }
}

export const SHOULD_LOAD_FIELDS = "Should Load Fields";
export class ShouldLoadFields implements Action {
    public readonly type = SHOULD_LOAD_FIELDS;
    constructor(readonly payload: {shouldLoad: boolean}){}
}

// export const UPDATE_PAGE_FIELD = "[Fields] Update page field";
// export class RemovePageFieldAction implements Action {
//     public readonly type = UPDATE_PAGE_FIELD;
//     // constructor(readonly payload: { SelectedField: string, TemplateId: string }) { }
//     constructor(readonly payload: { SelectedField: { FieldName: string, TemplateId: string } }) { }
// }

export const RADIO_SELECTED = "[Radio] selected";
export class RadioSelectdAction implements Action {
    public readonly type = RADIO_SELECTED;
    constructor(readonly payload: {RadioSelectd: {FieldName:string, TemplateId: string , Group: string }}) { }
}

export const RADIO_UNSELECTED = "[Radio] Unselected";
export class UnselectRadio implements Action {
    public readonly type = RADIO_UNSELECTED;
    constructor(readonly payload: {RadioUnselected: {FieldName:string, TemplateId: string , Group: string }}) { }
}



export const CLEAR_FIELD_STATE = "[Fields] Clear all fields";
export class ClearAllFieldsAction implements Action {
    public readonly type = CLEAR_FIELD_STATE;
    constructor(readonly payload: {}) { }
}

export const START_SIGN_FIELD = "[Fields] Start sign field";
export class StartSignFieldAction implements Action {
    public readonly type = START_SIGN_FIELD;
    // constructor(readonly payload: { SignFieldName: string, TemplateId: string }) { }
    constructor(readonly payload: { SelectedSignField: { FieldName: string, TemplateId: string } }) { }
}
export const FIELD_INTERSECT = "[Fields] Find field Intersect";
export class FieldsIntersectAction implements Action {
    public readonly type = FIELD_INTERSECT;
    // constructor(readonly payload: { SignFieldName: string, TemplateId: string }) { }
    constructor(readonly payload: { Field :PageField}) { }
}


export const CANCEL_SIGN_FIELD = "[Fields] Cancel sign field";
export class CancelSignFieldAction implements Action {
    public readonly type = CANCEL_SIGN_FIELD;
    constructor(readonly payload: {}) { }
}

export const SET_SIGN_FIELD_IMAGE = "[Fields] Set sign field image";
export class SetSignFieldImageAction implements Action {
    public readonly type = SET_SIGN_FIELD_IMAGE;
    constructor(readonly payload: { SelectedSignField: { FieldName: string, SignFieldImage: string, TemplateId: string, Type:SignatureType } }) { }
}

export const FIELD_BUTTON_CLICKED = "[Fields] Field button clicked";
export class FieldButtonClickedAction implements Action {
    public readonly type = FIELD_BUTTON_CLICKED;
    constructor(readonly payload: { FieldType: string, Y: number , X:number ,Width:number, Height:number, GroupName:string , Mandatory : boolean}) { }
}

export const SELECT_GROUP = "[Fields] Radio group selected";
export class SelectGroup implements Action {
    public readonly type = SELECT_GROUP;
    constructor(readonly payload: { SelectedRadioGroup: string }) { }
}

export const ADD_RADIO_GROUP = "[Fields] Add radio group";
export class AddRadioGroupAction implements Action {
    public readonly type = ADD_RADIO_GROUP;
    constructor(readonly payload: { GroupName: string }) { }
}

export type ACTIONS = SetDocumentNameAction | SelectField | AddPageFieldAction | RemovePageFieldAction
    | ClearAllFieldsAction | StartSignFieldAction | SetSignFieldImageAction | CancelSignFieldAction
    | SelectGroup | AddRadioGroupAction | RadioSelectdAction | FieldsIntersectAction | ShouldLoadFields;
