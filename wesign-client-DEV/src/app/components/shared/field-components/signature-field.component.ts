import { AfterViewInit, Component, ElementRef, HostListener, Input, OnDestroy, OnInit, Renderer2, RendererStyleFlags2, ViewChild } from "@angular/core";
import { Router } from '@angular/router';
import { FLOW_STEP } from "@models/enums/flow-step.enum";
import { SignatureField } from "@models/template-api/page-data-result.model";
import { Store } from "@ngrx/store";
import { GroupAssignService } from '@services/group-assign.service';
import { StateProcessService } from '@services/state-process.service';
import * as fieldActions from "@state/actions/fields.actions";
import { IAppState } from "@state/app-state.interface";
import { Subscription } from "rxjs";
import { environment } from '../../../../environments/environment';

import { BaseFieldComponent } from "./base-field.component";
import { SignatureFieldKind } from '@models/enums/signature-field-kind.enum';

@Component({
    selector: "[signatureField]",
    template: `
         <div class="marker" *ngIf="data.mandatory" ></div>
        <div id="{{data.name}}" class="ct-c-field ct-animate-fadein" [ngClass]="data.signerId" draggable="true" (dragstart)="ondrag($event)"
        (dragend)="ondragend($event)" style="width:100%;height:100%;" (focusin)="onFocusIn($event)"
        (endOffset)="onStopMoving($event)">
        
            <div class="field__icon">
                <i-feather name="feather"></i-feather>
            </div>

            <button  *ngIf="!data.image" (click)="signClick($event)"  (contextmenu)="onRightClick($event)" #currentInput>
                <span *ngIf="data.signatureKind == SignatureFieldKind.Simple">{{'TEMPLATE.SIGN_HERE' | translate}}</span>
                <span *ngIf="data.signatureKind == SignatureFieldKind.Initials">{{'TEMPLATE.SIGN_HERE_INITIALS' | translate}}</span>
            </button>
        
            <nav [ngClass]="{ 'multiple-nav': data.image }" style="z-index: 1;" *ngIf="showNavs">            
                <button   *ngIf="!isTiny" class="ct-button--icon button--field" (click)="editField()">
                <i-feather name="{{editIcon}}"></i-feather>
                </button>
                
                <button class="ct-button--icon button--field"  (click)="duplicateField()">
                <i-feather name="plus"></i-feather>
                </button>

                <button class="ct-button--icon button--field" (click)="removeField()">
                <i-feather name="trash-2"></i-feather>
                </button>
                <button *ngIf="data.image" class="ct-button--icon button--field clear-signature-btn" (click)="clearField($event)">
                </button>
            </nav> 
        </div>
    `,
})

export class SignatureFieldComponent extends BaseFieldComponent implements OnInit, AfterViewInit, OnDestroy {



    onRightClick($event: any) {
        if (!this.isDocView) {
            if (this.flowStep === FLOW_STEP.SELF_SIGN_PLACE_FIELDS) {
                this.store.dispatch(new fieldActions.StartSignFieldAction({ SelectedSignField: { FieldName: this.data.name, TemplateId: this.data.templateId } }));
            } else {
                this.assignService.toggleEditModal(true);
            }
            return false;
        }
    }

    get data(): SignatureField {
        return this._data;
    }

    get editIcon(): string {
        let icon = "edit-2";
        if (this.flowStep === FLOW_STEP.SELF_SIGN_PLACE_FIELDS) {
            icon = "feather"
        }
        return icon;
    }

    @Input()
    set data(value: SignatureField) {
        this._data = value;
    }

    public isTiny: boolean = false;

    public flowStep: FLOW_STEP;

    public FLOW_STEP = FLOW_STEP;
    public SignatureFieldKind = SignatureFieldKind;
    public selectedField: any;

    private _data: SignatureField;

    private storeSelectSub: Subscription;
    private isDocView: boolean = true;
    public get isResizable() {
        return this.flowStep === FLOW_STEP.TEMPLATE_EDIT || this.flowStep === FLOW_STEP.SELF_SIGN_PLACE_FIELDS;
    }
    @ViewChild("currentInput") public signatureUIElement: ElementRef;
    constructor(
        elRef: ElementRef,
        renderer: Renderer2,
        protected router: Router,
        protected store: Store<IAppState>,
        private stateProcess: StateProcessService,
        protected assignService: GroupAssignService
    ) {
        super(elRef, renderer);

    }

    @HostListener("click")
    public onSelected() {
        this.store.dispatch(new fieldActions.SelectField({ SelectedField: { FieldName: this.data.name, TemplateId: this.data.templateId } }));
        if (this.flowStep === FLOW_STEP.SELF_SIGN_SIGN) {
            this.store.dispatch(new fieldActions.StartSignFieldAction({ SelectedSignField: { FieldName: this.data.name, TemplateId: this.data.templateId } }));
        }

    }


    public onStopMoving(event) {
        this.updateElementPosition(this.data);
    }

    @HostListener("rzStop", ["$event"])
    public onStopResizing(event) {
        this.updateElementPosition(this.data);
    }

    public signClick($event) {
        if (this.flowStep === FLOW_STEP.SELF_SIGN_PLACE_FIELDS) {
            if (this.showNavs) {
                console.log("need to open pad");
            }
        }
    }

    //@HostListener("ondragstart", ["$event"])
    public ondrag(event) {
        event.dataTransfer.effectAllowed = "move";
        const _parent = event.target.parentElement;
        const fieldRect = _parent.getBoundingClientRect();
        _parent.id = 'drag-me-baby-' + (new Date()).getTime();

        const jsonData = JSON.stringify({ type: "move", element: _parent.id, gapx: event.clientX - fieldRect.x, gapy: event.clientY - fieldRect.y });
        //  console.log(jsonData);

        event.dataTransfer.setData("text", jsonData);
        this.store.dispatch(new fieldActions.SelectField({ SelectedField: { FieldName: this.data.name, TemplateId: this.data.templateId } }));
        // console.log("drag start")
    }

    //@HostListener("dragend", ["$event"])
    public ondragend(event) {
        //    console.log("drag end")        
        this.positionElement(this.data);
    }

    public ngAfterViewInit() {
        this.renderer.setStyle(this.elRef.nativeElement, "background-repeat", `no-repeat`);
        this.positionElement(this.data);
    }

    public ngOnInit() {
        this.storeSelectSub = this.store.select<any>('appstate').subscribe((state) => {
            this.flowStep = state.FlowStep;
            this.selectedField = state.SelectedField;

            if (this.selectedField.FieldName === this.data.name && this.selectedField.TemplateId == this.data.templateId) {

                if (this.flowStep === FLOW_STEP.SELF_SIGN_SIGN || this.flowStep === FLOW_STEP.ONLINE_SEND_GUIDE) {
                    // this.elRef.nativeElement.scrollIntoView({ behavior: "smooth", block: "center", inline: "nearest" });
                    this.renderer.addClass(this.elRef.nativeElement, "is-active");
                }
            } else {
                this.renderer.removeClass(this.elRef.nativeElement, "is-active");
            }
            const signatureElement = this.elRef.nativeElement.querySelector('.ct-c-field');
            if (this.data.image && signatureElement) {

                this.renderer.setStyle(signatureElement, "background-image", `url('${this.data.image}')`);

                const flags = RendererStyleFlags2.DashCase | RendererStyleFlags2.Important;
                this.renderer.setStyle(signatureElement, "background-position", `center`, flags);

                this.renderer.setStyle(signatureElement, "background-size", `contain`);
                this.renderer.setStyle(signatureElement, "background-repeat", `no-repeat`);

                //this.renderer.setStyle(this.elRef.nativeElement.children[1],"display",`none`);
            }



            this.showNavs = false;
            if (state.SelectedField.FieldName == this.data.name && state.SelectedField.TemplateId == this.data.templateId) {
                this.showNavs = true;
            }
            let mainUrl = this.router.url;
            this.isDocView = mainUrl.includes("docview");
            if (this.isDocView) {
                this.showNavs = false;
            }


            if (state.lastAction == fieldActions.FIELD_INTERSECT) {

                if (this.filedIntercect(this.data, state.IntersetctFiled)) {
                    this.signatureUIElement.nativeElement.focus();
                    this.renderer.setStyle(this.elRef.nativeElement, "z-index", 2);
                    this.renderer.addClass(this.elRef.nativeElement, "ct-animate-blink");
                    this.renderer.addClass(this.elRef.nativeElement, "ct-animate-blink");
                    setTimeout(() => this.renderer.removeClass(this.elRef.nativeElement, "ct-animate-blink"), 3000);


                }
            }
        });

        this.isTiny = environment.tiny;
    }

    public onFocusIn(e) {
        this.store.dispatch(new fieldActions.SelectField({ SelectedField: { FieldName: this.data.name, TemplateId: this.data.templateId } }))
    }


    public removeField() {
        // TODO - move to base class
        this.stateProcess.removeField(this.data.name, this.data.templateId);
        this.assignService.removeField(this.data);
    }

    public clearField(event: any) {
        event.stopPropagation();
        const signatureElement = this.elRef.nativeElement.querySelector('.ct-c-field');
        if (signatureElement) {
            this.renderer.removeStyle(signatureElement, "background-image");
            this.renderer.removeStyle(signatureElement, "background-position");
            this.renderer.removeStyle(signatureElement, "background-size");
            this.renderer.removeStyle(signatureElement, "background-repeat");
            this.data.image = null;
        }
    }

    public duplicateField() {

        let type = 'Signature';
        if (this._data.signatureKind == SignatureFieldKind.Initials) {
            type = 'SignatureInitials';
        }
        this.store.dispatch(new fieldActions.FieldButtonClickedAction({
            FieldType: type,
            Y: this.elRef.nativeElement.getBoundingClientRect().top,
            X: this.elRef.nativeElement.getBoundingClientRect().left,
            Width: this.elRef.nativeElement.getBoundingClientRect().width,
            Height: this.elRef.nativeElement.getBoundingClientRect().height,
            Mandatory: this._data.mandatory,
            GroupName: ""
        }));
    }

    public editField() {
        if (this.flowStep === FLOW_STEP.SELF_SIGN_PLACE_FIELDS) {
            this.store.dispatch(new fieldActions.StartSignFieldAction({ SelectedSignField: { FieldName: this.data.name, TemplateId: this.data.templateId } }));
        } else {
            this.assignService.toggleEditModal(true);
        }
    }

    public ngOnDestroy() {
        this.storeSelectSub.unsubscribe();
    }


}



