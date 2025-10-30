import { AfterViewInit, Component, ElementRef, HostListener, Input, OnDestroy, OnInit, Renderer2, ChangeDetectorRef, ViewChild } from "@angular/core";
import { Router } from '@angular/router';
import { FLOW_STEP } from "@models/enums/flow-step.enum";
import { TextFieldType } from '@models/enums/text-field-type.enum';
import { CheckBoxField, ChoiceField, RadioField, TextField } from "@models/template-api/page-data-result.model";
import { Actions, ofType } from '@ngrx/effects';
import { Store } from "@ngrx/store";
import { GroupAssignService } from '@services/group-assign.service';
import { StateProcessService } from '@services/state-process.service';
import * as fieldActions from "@state/actions/fields.actions";
import * as syncActions from "@state/actions/sync.actions";
import { AppState, IAppState } from "@state/app-state.interface";
import { Subscription } from "rxjs";
import { BaseFieldComponent } from "./base-field.component";

@Component({
    selector: "[textField]",
    templateUrl: "text-field.component.html",
})

export class TextFieldComponent extends BaseFieldComponent implements OnInit, AfterViewInit, OnDestroy {

    @Input()
    public data: TextField | CheckBoxField | RadioField | ChoiceField;

    public flowStep: FLOW_STEP;

    public fieldType: string = "text";
    public placeholder: string = "";
    public icon: string = "type"
    public htmlPattern: string = null;
    public formatOptions: object = {};//Cleave js 
    public fieldTypeForDuplication: string = "Text";
    public isValidField: boolean = false;
    @ViewChild("currentInput") public inputUIElement: ElementRef;
    protected storeSelectSubs: Subscription[] = [];
    public isvalue: string = "BVXCF";
    isDocView: boolean;
    public isHidden: boolean = false;

    constructor(
        elRef: ElementRef,
        renderer: Renderer2,
        protected router: Router,
        protected store: Store<IAppState>,
        protected changeDetectorRef: ChangeDetectorRef,
        protected stateProcess: StateProcessService,
        protected assignService: GroupAssignService,
        protected actions: Actions
    ) {
        super(elRef, renderer);

        this.storeSelectSubs.push(this.store.select<any>('appstate').subscribe((state) => {
            this.flowStep = state.FlowStep;
        }));
    }

    public onSelected() {
        this.store.dispatch(new fieldActions.SelectField({ SelectedField: { FieldName: this.data.name, TemplateId: this.data.templateId } }));
    }
    public onRightClick($event)
    {
        if(this.showNavs && this.flowStep != 2)
        {
            this.editField();
        }
        return false;
    }

    public ngAfterViewInit() {
        const parentRect = this.elRef.nativeElement.parentElement.parentElement.getBoundingClientRect();

        if (this.data instanceof TextField || this.data instanceof ChoiceField) {
            this.renderer.setStyle(this.elRef.nativeElement, "width", `${parentRect.width * this.data.width}px`);
            this.renderer.setStyle(this.elRef.nativeElement, "height", `${parentRect.height * this.data.height}px`);
        }

        this.positionElement(this.data);
        this.changeDetectorRef.detectChanges();


    }

    public ngOnInit() {
        if (this.data instanceof TextField) {
            this.fieldTypeForDuplication = TextFieldType[this.data.textFieldType];
            this.isHidden = this.data.isHidden;
        }

        this.storeSelectSubs.push(this.store.select<any>('appstate').subscribe((state) => {
            if (this.flowStep === FLOW_STEP.SELF_SIGN_SIGN || this.flowStep === FLOW_STEP.ONLINE_SEND_GUIDE) {
                //   this.elRef.nativeElement.scrollIntoView({ behavior: "smooth", block: "center", inline: "nearest" });
                this.renderer.addClass(this.elRef.nativeElement, "is-active");
            } else {
                this.renderer.removeClass(this.elRef.nativeElement, "is-active");
            }


        }));

        this.storeSelectSubs.push(this.stateProcess.getState().subscribe(
            (x: AppState) => {

                this.showNavs = false;
                if (x.SelectedField.FieldName == this.data.name && x.SelectedField.TemplateId == this.data.templateId) {
                    this.showNavs = true;
                }
                let mainUrl = this.router.url;
                this.isDocView = mainUrl.includes("docview");
                if (this.isDocView) {
                    if(this.fieldType == "date")
                    {
                        this.fieldType = "text";
                    }
                    this.showNavs = false;
                    this.isvalue = this.data.value;
                }

                if (x.lastAction == fieldActions.FIELD_INTERSECT) {
                    if (this.filedIntercect(this.data, x.IntersetctFiled)) {
                        this.inputUIElement.nativeElement.focus();
                        this.inputUIElement.nativeElement.classList.add("ct-animate-blink")
                        this.inputUIElement.nativeElement.style.zIndex = 2;

                        setTimeout(() => {
                            this.inputUIElement.nativeElement.classList.remove("ct-animate-blink");
                            this.inputUIElement.nativeElement.style.zIndex = 1;
                        }, 3000);

                    }
                }

            }
        ));

        this.actions.pipe(ofType(syncActions.TEXT_CHANGED))
            .subscribe((res: syncActions.TextChangedAction) => {
                if (this.data.description == res.payload.Description) {
                    this.data.value = res.payload.Text;
                }
            });
    }

    public ngOnDestroy() {
        if (this.storeSelectSubs)
            this.storeSelectSubs.forEach(s => s.unsubscribe());
    }

    public get isResizable() {
        return this.flowStep === FLOW_STEP.TEMPLATE_EDIT || this.flowStep === FLOW_STEP.SELF_SIGN_PLACE_FIELDS || this.flowStep === FLOW_STEP.MULTISIGN_ASSIGN;
        //return true;
    }

    public get isEditable() {
        return this.flowStep === FLOW_STEP.SELF_SIGN_SIGN || this.flowStep === FLOW_STEP.ONLINE_SEND_GUIDE ||
            this.flowStep == FLOW_STEP.SELF_SIGN_PLACE_FIELDS || this.flowStep === FLOW_STEP.MULTISIGN_ASSIGN || this.flowStep === FLOW_STEP.TEMPLATE_EDIT;
        //return true;
    }


    public onStopMoving(event) {
      //  console.log("onStopMoving")
        this.updateElementPosition(this.data);

    }

    @HostListener("rzStop", ["$event"])
    public onStopResizing(event) {
        this.updateElementPosition(this.data);
    }



    //@HostListener("dragend", ["$event"])
    public ondragend(event) {
      //  console.log("drag end")
        this.positionElement(this.data);
        this.fixWidth();
    }

    public fixWidth() {
        // leave empty
    }

    //@HostListener("ondragstart", ["$event"])
    public ondrag(event) {
        this.onSelected();
        event.dataTransfer.effectAllowed = "move";
        let _parent = event.target.parentElement;
        // if (event.target.parentElement.tagName.indexOf("SGN-") > -1) { // inside component fix / more than 1? 
        //     _parent = event.target.parentElement.parentElement;
        // }

        //_parent = event.target.parentElement.parentElement;
        const fieldRect = _parent.getBoundingClientRect();
        _parent.id = 'drag-me-baby-' + (new Date()).getTime();

        const jsonData = JSON.stringify({ type: "move", element: _parent.id, gapx: event.clientX - fieldRect.x, gapy: event.clientY - fieldRect.y });
       // console.log(jsonData);

        event.dataTransfer.setData("text", jsonData);
        // this.store.dispatch(new fieldActions.SelectField({ SelectedField: { FieldName: this.data.name, TemplateId: this.data.templateId } }));
       // console.log("drag start")
        this.showNavs = false;
    }

    public onTextChanged(elem: HTMLInputElement) {


        if (!this.validationCheck()) {
            this.renderer.addClass(this.elRef.nativeElement.parentElement, "is-mandatory");
            // TODO - send update to state - field not valid
        } else {
            this.renderer.removeClass(this.elRef.nativeElement.parentElement, "is-mandatory");
        }
        //console.log("Change");

        /* TODO only for specific flow */
        this.store.dispatch(new syncActions.TextChangedAction({
            Description: this.data.description,
            Text: elem.value,
        }));

    }



    //@HostListener("onfocusin", ["$event"])
    public onFocusIn(e) {
        this.store.dispatch(new fieldActions.SelectField({ SelectedField: { FieldName: this.data.name, TemplateId: this.data.templateId } }))

    }

    // RTL LTR keyboard check
    @HostListener("keypress", ["$event"])
    public onTextEntered(e) {
        // if (e.keyCode != 32) {
        //     var rtlRegex = /[\u0591-\u07FF\uFB1D-\uFDFD\uFE70-\uFEFC]/;
        //     var isRtl = rtlRegex.test(String.fromCharCode(e.which));
        //     var direction = isRtl ? 'rtl' : 'ltr';
        //     this.renderer.setAttribute(this.elRef.nativeElement, "dir", direction);
        // }
    };

    private validationCheck() {
        if (this.data.mandatory && !this.data.value)
            return false;

        if (this.htmlPattern) {
            const regMeBaby = new RegExp(this.htmlPattern, "gi");
            return regMeBaby.test(this.data.value);
        }

        return true;
    }

    public editField() {
        this.assignService.toggleEditModal(true);
    }

    public duplicateField() {
        this.store.dispatch(new fieldActions.FieldButtonClickedAction({
            FieldType: this.fieldTypeForDuplication,
            Y: this.elRef.nativeElement.getBoundingClientRect().top,
            X: this.elRef.nativeElement.getBoundingClientRect().left,
            Width: this.elRef.nativeElement.getBoundingClientRect().width,
            Height: this.elRef.nativeElement.getBoundingClientRect().height,
            GroupName: "",
            Mandatory : this.data.mandatory
        }));
    }

    public removeField() {
        this.stateProcess.removeField(this.data.name, this.data.templateId);
        this.assignService.removeField(this.data);
    }

    public onKeyPress(e) {
        if (this.fieldTypeForDuplication != "Number")
            return
        var characters = String.fromCharCode(e.which);
        if (e.target.value != "" || characters != "-")
            if (!(/[0-9]/.test(characters))) {
                e.preventDefault();
            }
    }


    public onPaste(e) {
        if (this.fieldTypeForDuplication != "Number")
            return
        e.stopPropagation();
        e.preventDefault();

        let clipboardData = e.clipboardData
        let pastedData = clipboardData.getData('Text');
        if ((/^-?[0-9]*$/.test(e.target.value + pastedData)))
            e.target.value += pastedData;


    }
}
