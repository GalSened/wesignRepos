import { HttpResponse } from "@angular/common/http";
import { Component, OnInit, AfterViewInit, OnDestroy } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { DocFilter } from "@models/document-api/doc-filter.model";
import { DocumentsStatus } from "@models/document-api/documents-status.model";
import { FLOW_STEP } from "@models/enums/flow-step.enum";
import { Store } from "@ngrx/store";
import { DocumentApiService } from "@services/document-api.service";
import { SharedService } from "@services/shared.service";
import * as appActions from "@state/actions/app.actions";
import { IAppState, AppState } from "@state/app-state.interface";
import { Observable, Subscription } from "rxjs";
import { switchMap } from "rxjs/operators";
import * as styleActions from "@state/actions/style.actions";
import { environment } from "../../../../environments/environment"
import { UserApiService } from '@services/user-api.service';
import { StateProcessService } from '@services/state-process.service';
import { User } from '@models/account/user.model';

@Component({
    selector: "sgn-main-component",
    templateUrl: "main.component.html",
})

export class MainComponent implements OnInit  /*, AfterViewInit */ {

    public appState: IAppState;
    public state$: Observable<AppState>;
    public tiny = environment.tiny;
    public filter: any;
    public user$;
    public currentUser : User;

    public inProcess = false;
    public links: { [key: string]: string[] } = {
        'DOCUMENT.ALL_DOCUMENTS': ['./all'],
        'DATA.FILTER.PENDING': ['./pending'],
        'DATA.FILTER.SIGNED': ['./signed'],
        'DATA.FILTER.DECLINED': ['./declined'],
        'DATA.FILTER.CANCELED': ['./canceled'],
        'DATA.FILTER.DISTRIBUTION': ['./distribution'],
        'DATA.FILTER.FOR_SIGNING': ['./signing'],
    }

    constructor(
        private store: Store<IAppState>,
        private documentApiService: DocumentApiService,
        private route: ActivatedRoute,
        private sharedService: SharedService,
        private stateService: StateProcessService,
        private router: Router,
        private userApi: UserApiService) {
    }

    public ngOnInit() {
        this.user$ = this.userApi.getCurrentUser();
        this.user$.subscribe((user:User) => {
            this.currentUser = user;
         });
        this.state$ = this.stateService.getState();
        this.state$.subscribe((x: AppState) => {
            if (!x.program.uiViewLicense.shouldShowDistribution) {
                this.links = {
                    'DOCUMENT.ALL_DOCUMENTS': ['./all'],
                    'DATA.FILTER.PENDING': ['./pending'],
                    'DATA.FILTER.SIGNED': ['./signed'],
                    'DATA.FILTER.DECLINED': ['./declined'],
                    'DATA.FILTER.CANCELED': ['./canceled'],
                    'DATA.FILTER.FOR_SIGNING': ['./signing'],
                }
            }
        })
    }

    public export(event) {
        if(this.inProcess)
        {
            return;
        }
        this.inProcess = true;
        setTimeout(() => {  this.inProcess = false;
        }, 3000);
        let sent = event == "all" || event == "pending";
        let viewed = event == "all" || event == "pending";
        let signed = event == "all" || event == "signed";
        let declined = event == "all" || event == "declined";
        let canceled = event == "all" || event == "canceled";
        let sendingFailed = event == "all";
        let distribution = event == "distribution";
        this.sharedService.setBusy(true, "DATA.EXPORT_EXCEL");
        if(distribution)
        {
            this.documentApiService.exportDistribution( this.currentUser.userConfiguration.language);
        }
        else
        {
            this.documentApiService.export(sent, viewed, signed, declined, sendingFailed, canceled, this.currentUser.userConfiguration.language); 
        }      
    }
}
