import { Component, ElementRef, OnInit, Renderer2, ChangeDetectorRef } from "@angular/core";
import { Store } from "@ngrx/store";
import { IAppState } from "@state/app-state.interface";
import { TextFieldComponent } from "./text-field.component";
import { TextField } from '@models/template-api/page-data-result.model';
import { TextFieldType } from '@models/enums/text-field-type.enum';
import { StateProcessService } from '@services/state-process.service';
import { GroupAssignService } from '@services/group-assign.service';
import { Router } from '@angular/router';
import { Actions } from '@ngrx/effects';

@Component({
    selector: "[timeField]",
    templateUrl: "text-field.component.html",
})

export class TimeFieldComponent extends TextFieldComponent {



    constructor(elRef: ElementRef,
        renderer: Renderer2,
        store: Store<IAppState>,
        protected router: Router,
        protected changeDetectorRef: ChangeDetectorRef,
        protected stateProcess: StateProcessService,
        protected assignService: GroupAssignService,
        protected actions: Actions
    ) {
        super(elRef, renderer, router, store, changeDetectorRef, stateProcess, assignService, actions);
        this.fieldType = "time";
        this.fieldTypeForDuplication = "Time";
        this.icon = "clock";
    }


}
