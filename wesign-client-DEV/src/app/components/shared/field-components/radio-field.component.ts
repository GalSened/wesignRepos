import { Component, ElementRef, HostListener, Input, OnInit, Renderer2, ChangeDetectorRef } from "@angular/core";
import { Router } from '@angular/router';
import { FLOW_STEP } from "@models/enums/flow-step.enum";
import { RadioField } from "@models/template-api/page-data-result.model";
import { Store } from "@ngrx/store";
import { GroupAssignService } from '@services/group-assign.service';
import { StateProcessService } from '@services/state-process.service';
import * as fieldActions from "@state/actions/fields.actions";
import { AppState, IAppState } from "@state/app-state.interface";
import { Observable, Subscription } from "rxjs";
import { tap } from "rxjs/operators";
import { TextFieldComponent } from "./text-field.component";
import { AngularResizableDirective } from 'angular2-draggable';
import { Actions } from '@ngrx/effects';
@Component({
    selector: "[radioField]",
    template: `
    <div class="marker" *ngIf="data.mandatory" ></div>
    
    <div class="ct-c-field is-radio ct-animate-fadein" [ngClass]="data.signerId"  
    (focusin)="onFocusIn($event)" draggable="true" (dragstart)="ondrag($event)" (dragend)="ondragend($event)">
    
           <input [attr.name]="data.groupName" type="radio" [attr.value]="data.value"  [readonly]="!isEditable"         
            [checked]="data.isDefault"  (change)="RadioChanged()" #currentInput >

        <nav style="z-index: 1;" *ngIf="showNavs">
            <button class="ct-button--icon button--field" (click)="editField()">
            <i-feather name="edit-2"></i-feather>
            </button>

            <button class="ct-button--icon button--field" (click)="duplicateField()">
            <i-feather name="plus"></i-feather>
            </button>

            <button class="ct-button--icon button--field" (click)="removeField()">
            <i-feather name="trash-2"></i-feather>
            </button>
        </nav>

  </div>        
`,

})

export class RadioButtonComponent extends TextFieldComponent {

    public declare flowStep: FLOW_STEP;
    public isSingleClick: Boolean = true;



    @Input()
    public declare data: RadioField;
    private storeSelectSub: Subscription;

    constructor(
        elRef: ElementRef,
        renderer: Renderer2,
        store: Store<IAppState>,
        protected router: Router,
        protected changeDetectorRef: ChangeDetectorRef,
        protected stateProcess: StateProcessService,
        protected assignService: GroupAssignService,
        protected actions: Actions
    ) {
        super(elRef, renderer, router, store, changeDetectorRef, stateProcess, assignService, actions);
        this.fieldTypeForDuplication = "Radio";

    }


    public ngOnInit() {
        super.ngOnInit();
        this.storeSelectSub = this.store.select<any>('appstate').subscribe(

            (x: AppState) => {
                if (x.lastAction == fieldActions.RADIO_SELECTED &&
                    x.RadioSelectd.TemplateId == this.data.templateId &&
                    x.RadioSelectd.Group == this.data.groupName) {
                    this.data.isDefault = x.RadioSelectd.FieldName == this.data.name;
                }

            });
    }
    public ngOnDestroy() {


        if (this.storeSelectSub) {
            this.storeSelectSub.unsubscribe();
        }

    }


    public ngAfterViewInit() {
        super.ngAfterViewInit();
        this.fixWidth();

    }


    public fixWidth() {

        if (this.elRef.nativeElement.getBoundingClientRect().width > 88) {
            this.renderer.setStyle(this.elRef.nativeElement, 'width', "45px");
            this.updateElementPosition(this.data);
        }


    }
    @HostListener("click")
    public onRadioSelected() {
        this.onSelected();
    }

    @HostListener("dblclick")
    public onDoubleClick() {
        this.isSingleClick = false;
        this.onRadioUnselected();
    }


    public onSelected() {
        this.store.dispatch(new fieldActions.SelectField({ SelectedField: { FieldName: this.data.name, TemplateId: this.data.templateId } }));
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
            GroupName: this.data.groupName,
            Mandatory: this.data.mandatory
        }));
    }

    public removeField() {
        this.stateProcess.removeField(this.data.name, this.data.templateId);
        this.assignService.removeField(this.data);
    }

    public RadioChanged() {
        this.isSingleClick = true;
        setTimeout(() => {
            if (this.isSingleClick)
                this.store.dispatch(new fieldActions.RadioSelectdAction({ RadioSelectd: { FieldName: this.data.name, TemplateId: this.data.templateId, Group: this.data.groupName } }));

        }, 30);
    }

    public onRadioUnselected() {
        this.store.dispatch(new fieldActions.UnselectRadio({ RadioUnselected: { FieldName: this.data.name, TemplateId: this.data.templateId, Group: this.data.groupName } }));
    }

}
