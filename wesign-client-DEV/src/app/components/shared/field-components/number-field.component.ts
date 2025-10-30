import { Component, ElementRef, Renderer2, ChangeDetectorRef } from "@angular/core";
import { Router } from '@angular/router';
import { Actions } from '@ngrx/effects';
import { Store } from "@ngrx/store";
import { GroupAssignService } from '@services/group-assign.service';
import { StateProcessService } from '@services/state-process.service';
import { IAppState } from "@state/app-state.interface";

import { TextFieldComponent } from "./text-field.component";

@Component({
    selector: "[numberField]",
    templateUrl: "text-field.component.html",
})

export class NumberFieldComponent extends TextFieldComponent {
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

        this.icon = "hash";
        this.fieldTypeForDuplication = "Number";
        

    }


}
