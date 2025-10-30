import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Router } from '@angular/router';
import { SharedService } from '@services/shared.service';
import { Location } from '@angular/common';
import { TranslateService } from '@ngx-translate/core';
import { PageField } from '@models/template-api/page-data-result.model';
import { GroupAssignService } from '@services/group-assign.service';
import * as fieldsActions from "@state/actions/fields.actions";
import * as documentActions from "@state/actions/document.actions";
import { Store } from '@ngrx/store';
import { IAppState } from '@state/app-state.interface';
@Component({
    selector: 'sgn-top-header-title',
    template: `
    <div class="ct-c-titlebar">
        <main>
            <div></div>
            <h1>{{title}}</h1>
        </main>

        <nav class="header__nav_doc_edit">
            <button class="ct-button--titlebar-outline" (click)="backClick()">
                {{'FORGET.BACK' | translate}}
            </button>
            <button *ngIf="nextButtonText" class="ct-button--titlebar-primary" (click)="next()">
                {{text}}
            </button>
        </nav>
    </div>
  `
})

export class TopHeaderTitleComponent implements OnInit {

    @Input() public title: string = "";
    public backUrl: string;
    @Input() public nextButtonText: string;
    @Output() public nextButton = new EventEmitter<any>();
    @Output() public backButton = new EventEmitter<any>();
    text: any;

    constructor(
        private store: Store<IAppState>,
        private translate: TranslateService,
        private router: Router,
        private sharedService: SharedService,
        private location: Location,
        private groupAssignService: GroupAssignService

    ) { }

    ngOnInit() {
        if (this.nextButtonText)
            this.text = this.translate.instant(this.nextButtonText);
    }

    public backClick(): void {
        this.groupAssignService.updateFieldsMap(new Map<string, PageField[]>());
       if(this.router.url.includes("selectsigners") )
       {
          this.groupAssignService.useMeaningOfSignature = false;
       }
        this.store.dispatch(new fieldsActions.ClearAllFieldsAction({}))
        if (this.router.url.includes("profile") || this.router.url.includes("documents") || this.router.url.includes("certsign")) {
            this.router.navigate(["/dashboard"])
        }
        if ( this.router.url.includes("template/edit")) {
            this.router.navigate(["/dashboard", "templates"])
        } 
        else {
            this.location.back();
        }
        this.backButton.emit();
    }

    public next() {
        if (this.nextButton && this.nextButtonText)
            this.nextButton.emit();
    }
}