import { Component, ElementRef, EventEmitter, HostListener, Input, OnDestroy, OnInit, Output, Renderer2 } from "@angular/core";
import { FLOW_STEP } from "@models/enums/flow-step.enum";
import { TextFieldType } from "@models/enums/text-field-type.enum";
import {
    CheckBoxField, ChoiceField, RadioField,
    PageDataResult, PageField, SignatureField, TextField, RadioFieldGroup
} from "@models/template-api/page-data-result.model";
import { Actions, ofType } from "@ngrx/effects";
import { Store } from "@ngrx/store";
import { SharedService } from "@services/shared.service";
import { TemplateApiService } from "@services/template-api.service";
import * as fieldActions from "@state/actions/fields.actions";
import { IAppState, AppState } from "@state/app-state.interface";
import { Subscription } from "rxjs";

import { DocumentApiService } from '@services/document-api.service';

import { StateProcessService } from '@services/state-process.service';
import { GroupAssignService } from '@services/group-assign.service';

import { SignatureFieldKind } from '@models/enums/signature-field-kind.enum';


@Component({
    selector: "[editpage]",
    templateUrl: "edit-page.component.html",
})

export class EditPageComponent implements OnInit, OnDestroy {
    public signerClassId: string;

    public get isDraggable(): boolean {
        return this.flowStep === FLOW_STEP.SELF_SIGN_PLACE_FIELDS
            || this.flowStep === FLOW_STEP.TEMPLATE_EDIT
            || this.flowStep === FLOW_STEP.TINY_SIGN_PLACE_FIELDS;
    }

    public get isResizable(): boolean {
        return this.flowStep === FLOW_STEP.SELF_SIGN_PLACE_FIELDS
            || this.flowStep === FLOW_STEP.TEMPLATE_EDIT
            || this.flowStep === FLOW_STEP.TINY_SIGN_PLACE_FIELDS
            || this.flowStep === FLOW_STEP.MULTISIGN_ASSIGN
            || this.flowStep === FLOW_STEP.ONLINE_SEND_GUIDE;
    }


    @Input() 
    public ocrHtml: any;

    @Input()
    public templateId: string;

    @Input()
    public pageNumber: number;

    @Input()
    public displayPageNumber: number;
    @Output() public changePageScroll = new EventEmitter<number>();

    // @Input()
    // public zmodel: { ZoomLevel: number, Bright: boolean }; // TODO - remove



    @Input()
    public pageData: PageDataResult;

    public textFields: TextField[] = [];
    public emailFields: TextField[] = [];
    public phoneFields: TextField[] = [];
    public numberFields: TextField[] = [];
    public dateFields: TextField[] = [];
    public timeFields: TextField[] = [];
    public multilineTextFields: TextField[] = [];
    public customFields: TextField[] = [];
    public checkBoxFields: CheckBoxField[] = [];
    public radioFields: RadioField[] = [];
    public radioGroupsNames: string[] = [];
    public signatureFields: SignatureField[] = [];
    public choiceFields: ChoiceField[] = [];


    public flowStep: number;

    public FLOW_STEP = FLOW_STEP;

    private storeSelectSub: Subscription;
    private actionsSub: Subscription;
    private pageSubs: Subscription;
    private appStore: AppState;
    private _selectedField: PageField;
    private isFieldLoaded: boolean = false;

    public aspectRatio = true;
    constructor(
        public el: ElementRef,
        private renderer: Renderer2,
        private templateApi: TemplateApiService,
        private documentApi: DocumentApiService,
        private store: Store<IAppState>,
        private actions: Actions,
        private sharedService: SharedService,
        private stateService: StateProcessService,
        private assignService: GroupAssignService
    ) {
        this.storeSelectSub = this.store.select<any>('appstate').subscribe((state) => {
            this.appStore = state;
            this.signerClassId = state.SelectedSignerClassId;
            if (this.pageNumber) {
                this.isFieldLoaded = true;
                this.textFields = this.fieldsOfType(state, TextFieldType.Text);
                this.emailFields = this.fieldsOfType(state, TextFieldType.Email);
                this.phoneFields = this.fieldsOfType(state, TextFieldType.Phone);
                this.numberFields = this.fieldsOfType(state, TextFieldType.Number);
                this.dateFields = this.fieldsOfType(state, TextFieldType.Date);
                this.dateFields = this.sharedService.FormatDateTextToDOMDateFormat(this.dateFields);
                this.timeFields = this.fieldsOfType(state, TextFieldType.Time);
                this.customFields = this.fieldsOfType(state, TextFieldType.Custom);
                this.multilineTextFields = this.fieldsOfType(state, TextFieldType.Multiline);

                this.checkBoxFields = [...state.PageFields
                    .filter((tx) => tx instanceof CheckBoxField && tx.page === this.pageNumber && tx.templateId == this.templateId)
                    .map((tx) => tx as CheckBoxField)];

                this.radioFields = [...state.PageFields
                    .filter((tx) => tx instanceof RadioField && tx.page === this.pageNumber && tx.templateId == this.templateId)
                    .map((tx) => tx as RadioField)];

                this.radioGroupsNames = state.RadioGroupNames;

                this.choiceFields = [...state.PageFields
                    .filter((tx) => tx instanceof ChoiceField && tx.page === this.pageNumber && tx.templateId == this.templateId)
                    .map((tx) => tx as ChoiceField)];

                this.signatureFields = [...state.PageFields
                    .filter((tx) => tx instanceof SignatureField && tx.page === this.pageNumber && tx.templateId == this.templateId)
                    .map((tx) => tx as SignatureField)];

                if (state.SelectedField) {
                    this._selectedField = this.stateService.getSelectedPageField<PageField>(state, PageField);
                    //let selectedPageField = state.PageFields.find(a => a.name == state.SelectedField) as PageField;
                    let _allFields = (this.el.nativeElement as HTMLElement).querySelectorAll(".doc__field");
                    for (let f = 0; f < _allFields.length; f++) {
                        if (_allFields[f].hasAttribute("data-fieldname") && _allFields[f].getAttribute("data-fieldname") == state.SelectedField.FieldName) {
                            _allFields[f].classList.remove("is-not-active");
                            // _allFields[f].classList.add("is-active", "ws_animate-color-fade");
                            _allFields[f].classList.add("ws_animate-color-fade");
                        } else {
                            // _allFields[f].classList.remove("is-active", "ws_animate-color-fade");
                            _allFields[f].classList.remove("ws_animate-color-fade");
                            _allFields[f].classList.add("is-not-active");
                        }
                    }

                }
            }
            else {
                this.isFieldLoaded = false;
            }
            this.flowStep = state.FlowStep;
        });

        this.actionsSub = this.actions.pipe(ofType(fieldActions.FIELD_BUTTON_CLICKED))
            .subscribe((action: fieldActions.FieldButtonClickedAction) => {
                let isRtl = document.getElementsByTagName("html")[0].getAttribute("dir") == "rtl";
                const zoomRatio = parseFloat(
                    //this.el.nativeElement.parentElement.style.transform.split("(")[1].split(")")[0]);
                    this.el.nativeElement.style.zoom);

                const rect = this.el.nativeElement.getBoundingClientRect();
                let xlocation = action.payload.X + 100;

                let ylocation = action.payload.Y * zoomRatio;
                if (action.payload.X == 0) {
                    xlocation = (rect.left + 100)
                }

                if (!isRtl) {
                    if (xlocation + 100 > rect.right) {
                        xlocation = rect.right - 200;
                    }
                    if (xlocation < rect.left) {
                        xlocation = rect.left + 100;
                    }
                }
                else if (action.payload.X > 0) {
                    xlocation = rect.right - (xlocation * zoomRatio)
                    if (xlocation - 100 < 0) {
                        xlocation = 250 * zoomRatio * 2;
                    }
                }

                if (rect.top > ylocation) {
                    ylocation = rect.top + 10;
                }

                if (rect.top + rect.height < ylocation) {
                    ylocation = rect.top + rect.height - 5;
                }

                if (rect.top * zoomRatio < action.payload.Y && (rect.top + rect.height) > action.payload.Y) {
                    const event = {
                        clientX: xlocation * zoomRatio,
                        clientY: ylocation,
                        width: action.payload.Width,
                        height: action.payload.Height,
                        dataTransfer: {
                            getData() { return action.payload.FieldType; },
                        },
                        target: this.el.nativeElement,
                        GroupName: action.payload.GroupName,
                        Mandatory: action.payload.Mandatory
                    };
                    this.drop(event);
                }
            });
    }

    @HostListener("click", ["$event"])
    public pageSelected(event) {
        if (event.target.classList.contains("X_PAGE")) {
            this.store.dispatch(new fieldActions.SelectField({ SelectedField: { FieldName: "", TemplateId: "" } }));
            //this.store.dispatch(new fieldActions.SelectGroup({ SelectedRadioGroup: null }));
        }
    }

public onClick() {
    console.log("Page clicked");
}
    public onMouseEnter()
    {
        this.changePageScroll.emit(this.pageNumber);
        
    }
    @HostListener('window:beforeunload', ['$event'])
    beforeUnloadHander(event: any) {
        event.returnValue = 'You have unfinished changes!';
    }

    public ngOnInit() {

        if (!this.isFieldLoaded){
            this.store.dispatch(new fieldActions.SelectField({ SelectedField: { FieldName: "", TemplateId: "" } }));
        }

        this.displayPageNumber = this.displayPageNumber || this.pageNumber;

        this.renderer.addClass(this.el.nativeElement, "X_PAGE");
        this.renderer.setStyle(this.el.nativeElement, "position", "relative");

        let _width = "auto";

        if (this.appStore.ShouldLoadFields) {
            this.loadPageDataFields();
        } else {
            //this.store.dispatch(new fieldActions.ShouldLoadFields({ shouldLoad: true }));
        }

        this.renderer.setStyle(this.el.nativeElement, "text-align", `left`);

        this.renderer.setStyle(this.el.nativeElement,
            "background-image", `url(${this.pageData.pageImage})`);
        this.renderer.setStyle(this.el.nativeElement, "width", `${this.pageData.pageWidth}px`);
        this.renderer.setStyle(this.el.nativeElement, "height", `${this.pageData.pageHeight}px`);

        _width = this.pageData.pageWidth + 'px';

        this.sharedService.notifyPageLoaded();

        this.renderer.setStyle(this.el.nativeElement, "width", _width);
    }

    public loadPageDataFields() {
        this.pageData.pdfFields.textFields.forEach((tf) => {
            tf.templateId = this.templateId;
            this.store.dispatch(
                new fieldActions.AddPageFieldAction({ PageField: Object.assign(new TextField(), tf) }));
            this.assignService.addField(Object.assign(new TextField(), tf));
        });

        this.pageData.pdfFields.checkBoxFields.forEach((cf) => {
            cf.templateId = this.templateId;
            this.store.dispatch(
                new fieldActions.AddPageFieldAction({ PageField: Object.assign(new CheckBoxField(), cf) }));
            this.assignService.addField(Object.assign(new CheckBoxField(), cf));
        });

        this.pageData.pdfFields.radioGroupFields.forEach((rgf) => {
            this.store.dispatch(new fieldActions.AddRadioGroupAction({ GroupName: rgf.name }));
            rgf.radioFields.forEach((rf) => {
                rf.groupName = rgf.name;
                rf.templateId = this.templateId;
                if (rgf.selectedRadioName === rf.name) rf.isDefault = true;
                this.store.dispatch(
                    new fieldActions.AddPageFieldAction({ PageField: Object.assign(new RadioField(), rf) }));
                this.assignService.addField(Object.assign(new RadioField(), rf));
            });
        });

        this.pageData.pdfFields.choiceFields.forEach((cf) => {
            cf.templateId = this.templateId;
            this.store.dispatch(
                new fieldActions.AddPageFieldAction({ PageField: Object.assign(new ChoiceField(), cf) }));
            this.assignService.addField(Object.assign(new ChoiceField(), cf));
        });

        this.pageData.pdfFields.signatureFields.forEach(
            (sf) => {
                sf.templateId = this.templateId;
                this.store.dispatch(
                    new fieldActions.AddPageFieldAction({ PageField: Object.assign(new SignatureField(), sf) }));
                this.assignService.addField(Object.assign(new SignatureField(), sf));
            });
    }

    // Where fields drop happens
    @HostListener("drop", ["$event"])
    public drop(event) {
        let classList = event.target.classList;
        if (event.currentTarget != null) {
            classList = event.currentTarget.classList;
        }
        if (!classList.contains("X_PAGE")) {
            return false;
        }
        const droppedType = event.dataTransfer.getData("text");
        try {
            const moveData = JSON.parse(droppedType);
            if (moveData && moveData.type == "move" && moveData.element) {

            //    console.log(event);

                (this.el.nativeElement as HTMLElement).firstElementChild.appendChild(document.getElementById(moveData.element));

                const draggableElem = document.getElementById(moveData.element);
                this.moveField(event, this._selectedField, moveData.gapx, moveData.gapy, draggableElem);

                event.dataTransfer.clearData();
            }
        } catch (e) {

            let translatedDroppedType = this.getDropTextType(droppedType)
            switch (translatedDroppedType) {
                case "Text":                
                case "Email":
                case "Phone":
                case "Number":
                case "Date":
                case "Time":
                case "Custom":
                case "Multiline":
                    const textField = new TextField();
                    textField.textFieldType = TextFieldType[`${translatedDroppedType}`];
                    textField.templateId = this.templateId;
                    textField.signerId = this.signerClassId;
                    textField.mandatory = event.Mandatory;
                    this.positionNewField(event,translatedDroppedType, textField);
                    this.store.dispatch(new fieldActions.AddPageFieldAction({ PageField: textField }));
                    if (this.flowStep === FLOW_STEP.MULTISIGN_ASSIGN) {
                        this.assignService.addField(textField);
                    }
                    break;
                case "Checkbox":
                    const checkBoxField = new CheckBoxField();
                    checkBoxField.templateId = this.templateId;
                    checkBoxField.signerId = this.signerClassId;
                    checkBoxField.mandatory = event.Mandatory;
                    this.positionNewField(event,translatedDroppedType, checkBoxField);
                    this.store.dispatch(new fieldActions.AddPageFieldAction({ PageField: checkBoxField }));
                    if (this.flowStep === FLOW_STEP.MULTISIGN_ASSIGN) {
                        this.assignService.addField(checkBoxField);
                    }
                    break;
                case "Radio":
                    const radioField = new RadioField();
                    radioField.templateId = this.templateId;
                    radioField.signerId = this.signerClassId;
                    radioField.mandatory = event.Mandatory;
                    if (event.GroupName) {
                        radioField.groupName = event.GroupName;//this.makeid();
                    }
                    else {
                        radioField.groupName = `Group_${this.makeid()}`;//this.makeid();
                    }

                    this.store.dispatch(new fieldActions.AddRadioGroupAction({ GroupName: radioField.groupName }));
                    this.positionNewField(event, translatedDroppedType,radioField);
                    this.store.dispatch(new fieldActions.AddPageFieldAction({ PageField: radioField }));
                    if (this.flowStep === FLOW_STEP.MULTISIGN_ASSIGN) {
                        this.assignService.addField(radioField);
                    }
                    break;
                case "Choice":
                    const choiceField = new ChoiceField();
                    choiceField.options = ["option"];
                    choiceField.templateId = this.templateId;
                    choiceField.signerId = this.signerClassId;
                    choiceField.mandatory = event.Mandatory;
                    this.positionNewField(event, translatedDroppedType,choiceField);
                    this.store.dispatch(new fieldActions.AddPageFieldAction({ PageField: choiceField }));
                    if (this.flowStep === FLOW_STEP.MULTISIGN_ASSIGN) {
                        this.assignService.addField(choiceField);
                    }
                    break;
                case "Signature":
                    const signatureField = new SignatureField();
                    signatureField.templateId = this.templateId;
                    signatureField.signerId = this.signerClassId;                    
                    signatureField.signatureKind = SignatureFieldKind.Simple;
                    signatureField.mandatory = true;
                    event.height = 100;
                    if (this.appStore && this.appStore.defaultSigningType) {
                        signatureField.signingType = this.appStore.defaultSigningType;
                    }
                    this.positionNewField(event, translatedDroppedType,signatureField);
                    this.store.dispatch(new fieldActions.AddPageFieldAction({ PageField: signatureField }));
                    if (this.flowStep === FLOW_STEP.MULTISIGN_ASSIGN) {
                        this.assignService.addField(signatureField);
                    }
                    break;
                case "Initials":
                case "SignatureInitials":
                    const signatureInitialsField = new SignatureField();
                    signatureInitialsField.templateId = this.templateId;
                    signatureInitialsField.signerId = this.signerClassId;
                    signatureInitialsField.signatureKind = SignatureFieldKind.Initials;
                    signatureInitialsField.mandatory = true;
                    event.height = 100;
                    if (this.appStore && this.appStore.defaultSigningType) {
                        signatureInitialsField.signingType = this.appStore.defaultSigningType;
                    }
                    this.positionNewField(event,translatedDroppedType, signatureInitialsField);
                    this.store.dispatch(new fieldActions.AddPageFieldAction({ PageField: signatureInitialsField }));
                    if (this.flowStep === FLOW_STEP.MULTISIGN_ASSIGN) {
                        this.assignService.addField(signatureInitialsField);
                    }
                    break;
            }
        }

    }

    private getDropTextType(droppedType)
    {
        let isRtl = document.getElementsByTagName("html")[0].getAttribute("dir") == "rtl";
        if(isRtl)
        {
            switch(droppedType)
                {
                    case "טקסט":
                     return "Text"
                    case "דוא\"ל":
                        return "Email";
                    case "טלפון":
                        return "Phone";
                    case "מספר":
                        return "Number";
                    case "תאריך" :
                        return "Date";
                    case "שעה" :
                    return "Time";
                    case "Custom":
                    return "Custom";
                    case "פסקה":
                    return  "Multiline";
                    case "תיבת סימון":
                    return "Checkbox";
                        case  "רדיו":
                        return "Radio";
                        case "רשימה":
                        return "Choice";
                        case "חתימה":
                     return  "Signature";
                        case "ראשי תיבות":
                    return "Initials";
                }
        }
        return droppedType;
    }
    @HostListener("dragover", ["$event"])
    public dragover(event) {
        //console.log("drag over")
        if (!event.target.classList.contains("X_PAGE")) {
            return false;
        }
        event.preventDefault();
    }

    public ngOnDestroy() {
        this.store.dispatch(new fieldActions.ShouldLoadFields({ shouldLoad: true }));
        //sessionStorage.removeItem("FIELDS_ARRAY");
        if (this.storeSelectSub)
            this.storeSelectSub.unsubscribe();
        if (this.actionsSub)
            this.actionsSub.unsubscribe();
        if (this.pageSubs)
            this.pageSubs.unsubscribe();
    }



    private fieldsOfType(state: AppState, type: TextFieldType): TextField[] { // TODO - use state service
        return [...state.PageFields
            .filter(
                (tx) => tx instanceof TextField && tx.page === this.pageNumber && tx.templateId == this.templateId
                    && tx.textFieldType === type)
            .map((tx) => tx as TextField),
        ];
    }

    private positionNewField(event,typeName, field: PageField) {
        //const zoomRatio = parseFloat(this.el.nativeElement.parentElement.style.transform.split("(")[1].split(")")[0]);
        const zoomRatio = parseFloat(this.el.nativeElement.style.zoom);

        let isRtl = document.getElementsByTagName("html")[0].getAttribute("dir") == "rtl";

        const targetRect = this.el.nativeElement.getBoundingClientRect();
        if (isRtl) {
            field.x = ((event.clientX / zoomRatio)) / targetRect.width - 0.13;
            if(field.x < 0)
            {
                field.x = 0;
            }

        }
        else {

            field.x = ((event.clientX / zoomRatio - targetRect.left) / targetRect.width);
        }
        if ((field.x + 0.16) > 1) {
            field.x =  1 - (event.width == undefined ? 0.16 : (event.width / targetRect.width));
        }

        field.y = ((event.clientY / zoomRatio - targetRect.top) / targetRect.height);
        field.width = event.width == undefined ? 0.16 : (event.width / targetRect.width);
        field.height = event.height == undefined ? (0.047 / 2) : (event.height / targetRect.height);
        field.page = this.pageNumber;
        field.name = `${typeName}_${this.makeid()}`;
        field.description = field.name;
        if( field.y + field.height > 1 )
        {
            field.y = 1 - field.height;
        }

        this.store.dispatch(new fieldActions.SelectField({ SelectedField: { FieldName: field.name, TemplateId: field.templateId } }));
    }

    private moveField(event, field: PageField, gapx: number, gapy: number, draggableElem) {
        const
            // zoomRatio = parseFloat(this.el.nativeElement.parentElement.style.transform.split("(")[1].split(")")[0]),
            currentzoom = parseFloat(this.el.nativeElement.style.zoom),
            zoomRatio = 1,//parseFloat(this.el.nativeElement.style.transform.split("(")[1].split(")")[0]),
            pageRec = this.el.nativeElement.getBoundingClientRect(),
            dragRec = draggableElem.getBoundingClientRect();

        let elemPositionX = (event.clientX - gapx),
            elemPositionY = (event.clientY - gapy);


        // X positioning 
        if ((elemPositionX / zoomRatio - pageRec.left) + dragRec.width > pageRec.width) {
            elemPositionX -= ((elemPositionX / zoomRatio - pageRec.left) + dragRec.width) - pageRec.width;
        }
        else if ((elemPositionX / zoomRatio - pageRec.left) < 1) {
            elemPositionX -= (elemPositionX / zoomRatio - pageRec.left);
        }
        // Y positioning
        if ((elemPositionY / zoomRatio - pageRec.top) + dragRec.height > pageRec.height) {
            elemPositionY -= ((elemPositionY / zoomRatio - pageRec.top) + dragRec.height) - pageRec.height;
        }
        else if ((elemPositionY / zoomRatio - pageRec.top) < 1) {
            elemPositionY -= (elemPositionY / zoomRatio - pageRec.top);
        }
        {

            field.x = ((elemPositionX / zoomRatio - pageRec.left) / pageRec.width);
            field.y = ((elemPositionY / zoomRatio - pageRec.top) / pageRec.height);

        }
        field.page = this.pageNumber;
    }

    private makeid() {
        let text = "";
        const possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        for (let i = 0; i < 5; i++) {
            text += possible.charAt(Math.floor(Math.random() * possible.length));
        }

        return text;
    }
}
