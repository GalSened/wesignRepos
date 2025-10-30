import { FLOW_STEP } from "@models/enums/flow-step.enum";
import { RadioField, SignatureField } from "@models/template-api/page-data-result.model";
import * as appActions from "../actions/app.actions";
import * as fieldActions from "../actions/fields.actions";
import * as selectActions from "../actions/selection.actions";
import * as styleActions from "../actions/style.actions";
import * as documentActions from "../actions/document.actions";
import { IAppState, AppState } from "../app-state.interface";
import { userType } from '@models/enums/user-type.enum';
import { Signer } from '@models/document-api/signer.model';
import { User } from '@models/account/user.model';
import { Signer1FileSiging, SigningFileType, UploadRequests } from '@models/template-api/upload-request.model';
import { SignatureType } from '@models/enums/signature-type.enum';


const initialState: AppState = {
    CurrentDocumentName: "",
    DashboardView: "documents",
    DocumentsNumber: 0, // TODO REMOVE
    FlowName: "none",
    FlowStep: FLOW_STEP.NONE,
    IsLoggedIn: false,
    IsRtl: false,
    Language: "en",
    CurrentUserName: "",
    RadioGroupNames: [],
    PageFields: [],
    // SelectedField: "",
    SelectedField: { FieldName: "", TemplateId: "" },
    //SelectedRadioGroup: "",
    SelectedTemplates: [],
    // SignFieldName: "",
    SelectedSignField: { FieldName: "", TemplateId: "" },
    RadioSelectd: { FieldName: "", TemplateId: "", Group: "" },
    Token: "",
    AuthToken: "",
    RefreshToken: "",
    //UserType: userType.Basic,
    HeaderStyles: "",
    SelfSignSignerAuth: null,
    FileUploadRequest: null,
    FileUploadRequests: null,
    Signers: [],
    EnableFreeTrailUsers: false,
    EnableTabletSupport: false,
    EnableUseSignerAuth: false,
    UseSignerAuthByDefault: false,
    SelectedSignerClassId: "",
    currentUserType: userType.Basic,
    program: null,
    userConfiguration: null,
    documentsStatus: [],
    documentsCollectionId: "",
    IntersetctFiled: null,
    lastAction: "",
    signer1FileSiging: {
        FileName: "", Base64File: "",
        SigingFileType: SigningFileType.NONE,
        Signer1Credential: null
    },
    companySigner1Details: null,
    enableDisplaySignerNameInSignature: false,
    enableMeaningOfSignature: false,
    shouldSignEidasSignatureFlow: false,
    shouldSignUsingSigner1AfterDocumentSigningFlow: false,
    enableVisualIdentityFlow: false,
    defaultSigningType: SignatureType.Graphic,
    ShouldSendWithOTPByDefault: false,
    ShouldLoadFields: true,
    ShouldFetchInfoOnCurrentUser: false,
    enableVideoConferenceFlow: false
};

export function appStateReducer(state: AppState = initialState,
    action: appActions.ACTIONS | fieldActions.ACTIONS | selectActions.ACTIONS | styleActions.ACTIONS | documentActions.ACTIONS): AppState {
    state.lastAction = action.type;
    switch (action.type) {
        case appActions.ENABLE_TABLETS:
            return {
                ...state,                
                EnableTabletSupport: (action as appActions.TabletsSupportAction).payload.EnableTabletSupport
            };

        case appActions.SET_SIGNER_AUTH_DEFAULT:
            return {
                ...state,

                ShouldSendWithOTPByDefault: (action as appActions.SetSignerAuthDefault).payload.shouldSendWithOTPByDefault,
                enableVisualIdentityFlow: (action as appActions.SetSignerAuthDefault).payload.enableVisualIdentityFlow,
                defaultSigningType: (action as appActions.SetSignerAuthDefault).payload.defaultSigningType,
                enableVideoConferenceFlow : (action as appActions.SetSignerAuthDefault).payload.enableVideoConferenceFlow,
            };
        case appActions.FETCH_CURRENT_USER_INFO:
                return {...state,
                    ShouldFetchInfoOnCurrentUser: (action as appActions.FetchCurrentUserInfo).payload.needToFetch,

            };
        case appActions.ENABLE_SIGNER_AUTH:
            return {
                ...state,
                EnableUseSignerAuth: true
            };
        case appActions.ENABLE_SIGNER_AUTH_DEFAULT:
            return {
                ...state,
                UseSignerAuthByDefault: true
            };

        case appActions.LOGIN:
            return {
                ...state,
                IsLoggedIn: true,
                Token: (action as appActions.LoginAction).payload.Token,
                RefreshToken: (action as appActions.LoginAction).payload.RefreshToken,
                AuthToken: (action as appActions.LoginAction).payload.AuthToken
            };
        case appActions.SET_CURRENT_USER_DETAILS:
            state.CurrentUserName = (action as appActions.SetCurrentUserDetailesAction).payload.userName;
            state.currentUserType = (action as appActions.SetCurrentUserDetailesAction).payload.userType;
            return {
                ...state
            }
        case appActions.LOGOUT:
            return initialState;
        case fieldActions.SELECT_FIELD:
            state.SelectedField = (action as fieldActions.SelectField).payload.SelectedField;
            return {
                ...state,
                ...action.payload,
            };
        case fieldActions.FIELD_INTERSECT:
            state.IntersetctFiled = (action as fieldActions.FieldsIntersectAction).payload.Field
            return { ...state };
        case appActions.FLOW_STATE:
        case appActions.LANGUAGE_STATE:
        case appActions.DASHBOARD_VIEW:
        case appActions.DOCUMENTS_STATE:
        case fieldActions.SET_DOCUMENT_NAME:
        case fieldActions.START_SIGN_FIELD:
            state.SelectedSignField = (action as fieldActions.StartSignFieldAction).payload.SelectedSignField;
            return {
                ...state,
                ...action.payload,
            };
        case fieldActions.CANCEL_SIGN_FIELD:
            state.SelectedSignField = { FieldName: "", TemplateId: "" };
            return { ...state };
        case fieldActions.ADD_PAGE_FIELD:
            // need to check duplications 
            state.PageFields.push((action as fieldActions.AddPageFieldAction).payload.PageField);
            return {
                ...state,
            };
        case fieldActions.ADD_PAGE_FIELDS:
            // need to check duplications 
            state.PageFields = state.PageFields.concat(((action as fieldActions.AddPageFieldsAction).payload.PageFields));
            return {
                ...state,
            };
        case fieldActions.SHOULD_LOAD_FIELDS:
            state.ShouldLoadFields = (action as fieldActions.ShouldLoadFields).payload.shouldLoad;
            return {
                ...state,
            };
        case fieldActions.REMOVE_PAGE_FIELD:
            state.PageFields = state.PageFields.filter(
                (t) => t.name !== (action as fieldActions.RemovePageFieldAction).payload.SelectedField.FieldName
                //& t.templateId !== (action as fieldActions.RemovePageFieldAction).payload.SelectedField.TemplateId
            );
            return {
                ...state,
            };
        case fieldActions.CLEAR_FIELD_STATE:
            state.PageFields = [];
            return { ...state };
        case fieldActions.SET_SIGN_FIELD_IMAGE:
            const fieldName = (action as fieldActions.SetSignFieldImageAction).payload.SelectedSignField.FieldName;
            const image = (action as fieldActions.SetSignFieldImageAction).payload.SelectedSignField.SignFieldImage;
            const templeteId = (action as fieldActions.SetSignFieldImageAction).payload.SelectedSignField.TemplateId;
            const type = (action as fieldActions.SetSignFieldImageAction).payload.SelectedSignField.Type;
            const signFields = state.PageFields.filter((pf) => pf instanceof SignatureField && pf.name === fieldName && pf.templateId == templeteId);
            state.SelectedSignField = { FieldName: "", TemplateId: "" };
            signFields.forEach(x => {
                (x as SignatureField).image = image;
                (x as SignatureField).signingType = type;
            });


            return { ...state };
        case fieldActions.ADD_RADIO_GROUP:
            let gname = (action as fieldActions.AddRadioGroupAction).payload.GroupName;
            if (!state.RadioGroupNames.includes(gname))
                state.RadioGroupNames.push(gname);
            return {
                ...state,
            };
        case selectActions.SELECT_SIGNER_CLASS_ID:
            state.SelectedSignerClassId = (action as selectActions.SelectSignerClassId).payload.classId;
            return { ...state };
        case appActions.SET_DOCUMENT_LIST_STATUS:
            state.documentsStatus = (action as appActions.SetDocumentListStatus).payload.documentsStatus;
            return { ...state };
        case appActions.SET_DOCUMENT_COLLECTION_TO_VIEW:
            state.documentsCollectionId = (action as appActions.SetSelectedDocumentCollectionToView).payload.documentsCollectionId;
            return { ...state };
        case selectActions.SELECT_TEMPLATE:
            state.SelectedTemplates.push((action as selectActions.SelectTemplateAction).payload.templateInfo);
            return { ...state };
        case selectActions.UNSELECT_TEMPLATE:
            const templateInfo = (action as selectActions.UnselectTemplateAction).payload.templateInfo;
            const templateId = templateInfo.templateId;
            const tIndex = state.SelectedTemplates.findIndex((t) => t.templateId === templateId);
            if (tIndex > -1) {
                state.SelectedTemplates.splice(tIndex, 1);
            }
            return { ...state };
        case selectActions.MOVE_TEMPLATE:
            const index = (action as selectActions.MoveTemplateAction).payload.index;
            const direction = (action as selectActions.MoveTemplateAction).payload.direction;
            let deleted = state.SelectedTemplates.splice(index, 1);
            state.SelectedTemplates.splice(index + direction, 0, deleted.shift());
            return { ...state };
        case selectActions.CLEAR_TEMPLATE_SELECTION:
            state.SelectedTemplates = [];

            return { ...state };
        case styleActions.STYLE_HEADER_CLASSES:
            state.HeaderStyles = (action as styleActions.StyleHeaderClassesAction).payload.Classes;
            return { ...state };
        case appActions.LANGUAGE_STATE:
            return {
                ...state,
                Language: (action as appActions.SetLangAction).payload.Language,
                IsRtl: (action as appActions.SetLangAction).payload.IsRtl
            };
        case appActions.SIGNER_AUTH_STATE:
            return {
                ...state,
                SelfSignSignerAuth: (action as appActions.SetSignerAuthAction).payload.signerAuth
            };
        case documentActions.SET_FILE_UPLOAD_REQUEST:
            return {
                ...state,
                FileUploadRequest: (action as documentActions.SetFileUploadRequestAction).payload.fileUploadRequest
            }
        case documentActions.SET_MULTIPLE_FILES_UPLOAD_REQUEST:
            return {
                ...state,
                FileUploadRequests: (action as documentActions.SetMultipleFilesUploadRequestAction).payload.fileUploadRequests
            }
        case documentActions.SET_CERTIICATE_FILE_UPLOAD:
            return {
                ...state,
                signer1FileSiging: (action as documentActions.SetCertificateFileUploadRequestAction).payload.signer1FileSiging
            }
        case documentActions.CLEAR_FILE_UPLOAD_REQUEST:
            state.FileUploadRequest = null;
            state.FileUploadRequests = new UploadRequests();
            return { ...state };
        case documentActions.SET_SIGNERS:
            return {
                ...state,
                Signers: (action as documentActions.SetSigners).payload.Signers
            }
        case documentActions.SET_DOCUMENTNAME:
            return {
                ...state,
                CurrentDocumentName: (action as documentActions.SetDocumentName).payload.CurrentDocumentName
            }
        case appActions.SET_CURRENT_USER_PROGRAM:
            return {
                ...state,
                program: (action as appActions.SetCurrentUserProgram).payload.program
            }

        case appActions.SET_CURRENT_USER_CONFIGURATION:
            return {
                ...state,
                userConfiguration: (action as appActions.SetCurrentUserConfiguration).payload.userConfiguration
            }

        case appActions.SET_COMPANY_SIGNER1_DETAILS:
            return {
                ...state,
                companySigner1Details: (action as appActions.SetCompanySigner1Details).payload.companySigner1Details
            }
        
        case appActions.SET_DISPLAY_SIGNER_NAME_IN_SIGNATURE:
            return {
                ...state,
                enableDisplaySignerNameInSignature: (action as appActions.SetDisplaySignerNameInSignature).payload.enableDisplaySignerNameInSignature
            }
        case appActions.SET_ENABLE_SIGN_EIDAS_SIGNATURE_FLOW:
            return {
                        ...state,
                        shouldSignEidasSignatureFlow: (action as appActions.SetEnableSignEidasSignatureFlow).payload.enableSignEidasSignatureFlow
            }                        
        case appActions.SET_SHOULD_SIGN_USING_SIGNER1_AFTER_DOCUMMENT_SIGNING_FLOW:
            return {
                ...state,
                shouldSignUsingSigner1AfterDocumentSigningFlow: (action as appActions.SetShouldSignUsingSigner1AfterDocumentSigningFlow).payload.shouldSignUsingSigner1AfterDocumentSigningFlow
            }
        case appActions.SET_DISPLAY_MEANING_OF_SIGNATURE:
                return {
                        ...state,
                        enableMeaningOfSignature: (action as appActions.SetDisplayMeaningOfSignature).payload.enableMeaningOfSignature
                }
        case fieldActions.RADIO_SELECTED:
            let xxx = state.PageFields.filter((pf) => pf instanceof RadioField);
            let xxxx = xxx.map((pf) => pf as RadioField);
            let selected = (action as fieldActions.RadioSelectdAction).payload.RadioSelectd;
            xxxx.forEach(element => {
                if (element.groupName == selected.Group) {
                    if (element.name == selected.FieldName) {
                        element.isDefault = true;
                    } else {
                        element.isDefault = false;
                    }
                }
            });
            return {
                ...state,
                RadioSelectd: (action as fieldActions.RadioSelectdAction).payload.RadioSelectd
            }
        case fieldActions.RADIO_UNSELECTED:
            let radFields = state.PageFields.filter((pf) => pf instanceof RadioField);
            let mappedRadFields = radFields.map((pf) => pf as RadioField);
            let unselected = (action as fieldActions.UnselectRadio).payload.RadioUnselected;
            mappedRadFields.forEach(element => {
                if (element.groupName == unselected.Group) {
                    if (element.name == unselected.FieldName) {
                        element.isDefault = false;
                    }
                }
            });
            return {
                ...state,
                RadioSelectd: { FieldName: "", TemplateId: "", Group: "" }
            }

        default:
            return state;
    }
}
