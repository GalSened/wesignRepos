import { Component, ElementRef, HostListener, Input, Renderer2, ChangeDetectorRef, OnInit } from "@angular/core";
import { FLOW_STEP } from "@models/enums/flow-step.enum";
import { ChoiceField } from "@models/template-api/page-data-result.model";
import { Store } from "@ngrx/store";
import { GroupAssignService } from '@services/group-assign.service';
import { StateProcessService } from '@services/state-process.service';
import { IAppState } from "@state/app-state.interface";
import { TextFieldComponent } from "./text-field.component";
import * as fieldActions from "@state/actions/fields.actions";
import { Router } from '@angular/router';

import { Actions } from '@ngrx/effects';

@Component({
    selector: "[choiceField]",
    template: `
         <div class="marker" *ngIf="data.mandatory" ></div>
            <div class="ct-c-field ct-animate-fadein" [ngClass]="data.signerId" style="width: 100%;height: 100%;"
            (focusin)="onFocusIn($event)" draggable="true" (dragstart)="ondrag($event)" (dragend)="ondragend($event)"
            (endOffset)="onStopMoving($event)">
       
                <div class="field__icon">
                    <i-feather [name]="icon"></i-feather>
                </div>
             
                <select class="ct-select--primary" (click)="onChoiceSelected()" [(ngModel)]="data.selectedOption"  #currentInput> 
                    <option *ngFor="let option of data?.options" [selected]="option==data.selectedOption" [ngValue]="option">{{option}}</option>
                </select>

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

export class ChoiceComponent extends TextFieldComponent {

    public declare flowStep: FLOW_STEP;

    @Input()
    public declare data: ChoiceField;

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
        this.icon = "list";
        this.fieldTypeForDuplication = "Choice";
    }

    @HostListener("click")
    public onChoiceSelected() {
        this.onSelected();
    }

    public editField() {
        this.assignService.toggleEditModal(true);
    }

    public duplicateField() {
        this.store.dispatch(new fieldActions.FieldButtonClickedAction({
            FieldType: this.fieldTypeForDuplication,
            Y: this.elRef.nativeElement.getBoundingClientRect().top,
            X : this.elRef.nativeElement.getBoundingClientRect().left,
            Width : this.elRef.nativeElement.getBoundingClientRect().width,
            Height : this.elRef.nativeElement.getBoundingClientRect().height,
            GroupName: "",
            Mandatory: this.data.mandatory
        }));
    }

    public removeField() {
        this.stateProcess.removeField(this.data.name, this.data.templateId);
        this.assignService.removeField(this.data);
    }
}
