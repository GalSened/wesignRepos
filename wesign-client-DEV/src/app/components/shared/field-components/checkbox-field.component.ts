import { Component, ElementRef, HostListener, Input, Renderer2, ChangeDetectorRef, ViewChild } from "@angular/core";
import { CheckBoxField } from "@models/template-api/page-data-result.model";
import { Store } from "@ngrx/store";
import { GroupAssignService } from '@services/group-assign.service';
import { StateProcessService } from '@services/state-process.service';
import { IAppState } from "@state/app-state.interface";
import { TextFieldComponent } from "./text-field.component";
import * as fieldActions from "@state/actions/fields.actions";
import { Router } from '@angular/router';

import { Actions } from '@ngrx/effects';

@Component({
    selector: "[checkBoxField]",
    template: `
     <div class="marker" *ngIf="data.mandatory" ></div>
        <div class="ct-c-field is-checkbox ct-animate-fadein"  [ngClass]="data.signerId" (focusin)="onFocusIn($event)"
         draggable="true" (dragstart)="ondrag($event)" (dragend)="ondragend($event)" >
         
             <input type="checkbox" (click)="onSelected()" [(ngModel)]="data.isChecked" [readonly]="!isEditable"  #currentInput/>       

            <nav style="z-index: 1;" *ngIf="showNavs" [ngClass]="flowStep == 2 ? 'nav_with_two_elements' : ''" >
                <button class="ct-button--icon button--field" *ngIf="flowStep != 2" (click)="editField()">
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

export class CheckboxComponent extends TextFieldComponent  {

    @Input()
    public declare data: CheckBoxField;
    
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
        this.fieldTypeForDuplication = "Checkbox";
        
    }

    @HostListener("click")
    public onCheckBoxSelected() {
        this.onSelected();
    }

    public editField() {
        this.assignService.toggleEditModal(true);
    }

    public ngAfterViewInit() {
        super.ngAfterViewInit();       
        this.fixWidth()
    }
    

    public duplicateField() {
        this.store.dispatch(new fieldActions.FieldButtonClickedAction({
            FieldType: this.fieldTypeForDuplication,
            Y: this.elRef.nativeElement.getBoundingClientRect().top,
            X : this.elRef.nativeElement.getBoundingClientRect().left,
            Width : this.elRef.nativeElement.getBoundingClientRect().width,
            Height : this.elRef.nativeElement.getBoundingClientRect().height,
            GroupName: "",
            Mandatory:this.data.mandatory
        }));
    }

    public removeField() {
        this.stateProcess.removeField(this.data.name, this.data.templateId);
        this.assignService.removeField(this.data);
    }
    
    public fixWidth()
    {        
        if(this.elRef.nativeElement.getBoundingClientRect().width > 75) 
        {
            this.renderer.setStyle(this.elRef.nativeElement, 'width', "45px");
            this.renderer.setStyle(this.elRef.nativeElement, 'height', "45px");
            this.updateElementPosition(this.data);
        }
    }
}
