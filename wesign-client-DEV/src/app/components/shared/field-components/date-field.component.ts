import { Component, ElementRef, OnInit, Renderer2, AfterViewInit, ChangeDetectorRef } from "@angular/core";
import { Store } from "@ngrx/store";
import { IAppState } from "@state/app-state.interface";
import { TextFieldComponent } from "./text-field.component";
//import * as moment from "moment";
import Cleave from 'cleave.js';
import { TextField } from '@models/template-api/page-data-result.model';
import { TextFieldType } from '@models/enums/text-field-type.enum';
import { StateProcessService } from '@services/state-process.service';
import { GroupAssignService } from '@services/group-assign.service';
import { Router } from '@angular/router';
import { Actions } from '@ngrx/effects';

//https://stackblitz.com/edit/cleave-directive?file=src%2Fapp%2Finput-mask.directive.ts
//https://github.com/nosir/cleave.js/blob/master/doc/options.md

@Component({
    selector: "[dateField]",
    templateUrl: "text-field.component.html",
})

export class DateFieldComponent extends TextFieldComponent implements AfterViewInit {


    constructor(
        elRef: ElementRef,
        renderer: Renderer2,
        store: Store<IAppState>,
        protected router: Router,
        changeDetectorRef: ChangeDetectorRef,
        protected stateProcess: StateProcessService,
        protected assignService: GroupAssignService,
        protected actions: Actions
    ) {
        super(elRef, renderer, router, store, changeDetectorRef, stateProcess, assignService, actions);

        if(this.isDocView)
        {
            this.fieldType = "text";
        }
        else
        {
            this.fieldType = "date";
        }
        this.icon = "calendar";

        this.fieldTypeForDuplication = "Date";
    }



}
