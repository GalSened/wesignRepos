import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Modal } from '@models/modal/modal.model';
import { TranslateService } from '@ngx-translate/core';
import { AppConfigService } from '@services/app-config.service';
import { SharedService } from '@services/shared.service';
import { UserApiService } from '@services/user-api.service';
import { environment } from "../../../environments/environment";
import { Store } from "@ngrx/store";
import { IAppState, AppState } from "@state/app-state.interface";
import { Subscription } from 'rxjs';

@Component({
    selector: 'sgn-footer-component',
    templateUrl: "footer.component.html",
})


export class FooterComponent implements OnInit {

    @Input() public isFullClass: boolean = true;
    showLogOut: boolean;
    showAAA: boolean;
    public tiny = environment.tiny;
    public emailSupport: string;
    public year: number = 2022;
    public popUpModal: Modal;
    public isEng: boolean;
    public contactUsLink: string;
    public comsignGuidesLink: string;
    public lastAction: Subscription;

    constructor(private userApiService: UserApiService,
        private router: Router,
        private translate: TranslateService,
        private appConfigService: AppConfigService,
        private sharedService: SharedService,
        private store: Store<IAppState>) {
        this.emailSupport = appConfigService.EmailSupport;
        this.popUpModal = new Modal();

        this.lastAction = this.store.select<any>('appstate').subscribe((state: AppState) => {
            if (state.lastAction == "[Language] Set Language State") {
                this.isEng = state.Language == "en" ? true : false;
                this.contactUsLink = this.isEng ? this.appConfigService.ContactUsEnglishLink : this.appConfigService.ContactUsHebrewLink;
            }
        });
    }
    
    ngOnInit() {
        let x = this.router.url.split('/').pop();
        this.showLogOut = ((x != "login") && (x != "forget") && (x != "register"));
        if ((<string>this.router.url).includes("resetpass")) {
            this.showLogOut = false;
        }
        this.year = new Date().getFullYear();
        this.showAAA = this.appConfigService.EnAAAUrl != "" && this.appConfigService.HeAAAUrl != "";
        this.translate.get(['LOGOUT.MODALTITLE', 'LOGOUT.MODALTEXT', 'REGISTER.CANCEL', 'BUTTONS.SUBMIT'])
        .subscribe((res: object) => {
            let keys = Object.keys(res);
            this.popUpModal.title = res[keys[0]];
            this.popUpModal.content = res[keys[1]];
            this.popUpModal.rejectBtnText = res[keys[2]];
            this.popUpModal.confirmBtnText = res[keys[3]];
        });
        
        this.isEng = this.sharedService.getCurrentLanguage().Code == "en";
        this.contactUsLink = this.isEng ? this.appConfigService.ContactUsEnglishLink : this.appConfigService.ContactUsHebrewLink;
        this.comsignGuidesLink = this.appConfigService.ComsignGuidesLink;
    }

    public ngOnDestroy() {
        if (this.lastAction) {
            this.lastAction.unsubscribe();
        }
    }

    public showLogOutPopup() {
        this.popUpModal.showModal = true;
    }

    public logout() {
        this.userApiService.logout();
        
    }

    public openAAAUrl() {
        this.isEng = this.sharedService.getCurrentLanguage().Code == "en";
        let url = this.isEng ? this.appConfigService.EnAAAUrl : this.appConfigService.HeAAAUrl;
        window.open(url, "_blank").focus();
    }

    public appVersion(): string {
        return this.appConfigService.appVersion;
    }
}