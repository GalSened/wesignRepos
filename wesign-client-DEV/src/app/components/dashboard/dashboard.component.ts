import { Component, OnInit, ViewChild } from "@angular/core";
import { ProgramInfo } from "@models/program/program-info.model";
import { ProgramStatus } from "@models/program/program-status.model";
import { UserApiService } from "@services/user-api.service";
import { Observable, Subscription } from 'rxjs';
import { AppState, IAppState } from '@state/app-state.interface';
import { map } from 'rxjs/operators';
import { Store } from '@ngrx/store';
import { User } from '@models/account/user.model';
import { environment } from "../../../environments/environment"
import * as appActions from "@state/actions/app.actions";
import { LangSelectorComponent } from '@components/shared/lang-selector.component';
import { ILanguage, LangList } from '@components/shared/languages.list';
import { TranslateService } from '@ngx-translate/core';
import { SharedService } from '@services/shared.service';
import { Signer1Credential } from '@models/self-sign-api/signer-authentication.model';
import { UiViewLicense } from '@models/program/user-program.model';
import { Modal } from '@models/modal/modal.model';
import { PageField } from '@models/template-api/page-data-result.model';
import { GroupAssignService } from '@services/group-assign.service';
import { group } from '@models/managment/groups/management-groups.model';
import { LoginResult } from '@models/Users/login-result.model';
import * as actions from "@state/actions/app.actions";
import { Router } from '@angular/router';

@Component({
    selector: "sgn-dashboard",
    templateUrl: "dashboard.component.html",
})

export class DashboardComponent {
    public popUpModal: Modal;
    public programStatus: ProgramStatus;
    public programInfo: ProgramInfo;
    public programExploitedPercent: number;
    headerClasses$: Observable<string>;
    public user$: Observable<User>;
    private userSubscription: Subscription;
    companyLogo: string;
    public tiny = environment.tiny;
    public manuShow : boolean = false;
    public lang: LangSelectorComponent ;
    public uiUser : User;
    public groupsToSwitch:group[];
    public selectedOption: string;
    public showSwitchGroup : boolean = false;
    //@ViewChild('lang', { static: true }) lang: LangSelectorComponent;
    public languages: ILanguage[] = LangList;
   
    uiViewLicense: UiViewLicense;
    constructor(private store: Store<IAppState>,
        private userApiService: UserApiService,
        private translate: TranslateService,
        private groupAssignService: GroupAssignService,
        private sharedService: SharedService,
        private router: Router,) {
        this.headerClasses$ = store.select<any>('appstate').pipe(
            map((state) => {
                return state.HeaderStyles;
            })
        )
        this.popUpModal = new Modal();
        this.lang = new LangSelectorComponent(store, translate, sharedService);
    }


    onTemplatesClick(){
        sessionStorage.removeItem("CONTACTS");
        sessionStorage.removeItem("SIGNERS");
        sessionStorage.removeItem("FIELDS_ARRAY");
        
        this.groupAssignService.updateFieldsMap(new Map<string, PageField[]>());
        this.groupAssignService.useMeaningOfSignature = false;
        
        
    }

    public onLanguageChange() {
        let code = this.selectedOption == this.translate.instant(`LANGUAGE.ENGLISH`) ? "en" : "he";
        let isRtl = this.selectedOption == this.translate.instant(`LANGUAGE.ENGLISH`) ?  false : true;
        sessionStorage.setItem("language", code);
        this.translate.use(code);
        document.getElementsByTagName("html")[0].setAttribute("dir", isRtl ? "rtl" : "ltr");
        document.getElementsByTagName("html")[0].setAttribute("lang", code);
        this.store.dispatch(new actions.SetLangAction({ Language: code, IsRtl: isRtl }));
        let language = this.languages.find((l) => l.Code === code);
        this.lang.languageSelected(language);
        this.uiUser.userConfiguration.language = language.Enum;
        this.userApiService.updateUser(this.uiUser).subscribe();
    }

    public switchToGroup($event)
    {
        
        this.userApiService.SwitchGroup($event).subscribe( (data: LoginResult) => {
            this.userApiService.accessToken = data.token;
            this.userApiService.refreshAccessToken = data.refreshToken;
            this.userApiService.authToken = data.authToken;
            this.store.dispatch(new actions.LoginAction({ Token: data.token, RefreshToken: data.refreshToken, AuthToken:data.authToken }));
            if(this.userApiService.authToken != "" && this.userApiService.authToken != null)
            {
            let auth = new Signer1Credential();
            auth.signerToken = this.userApiService.authToken;
            this.store.dispatch(new actions.SetSignerAuthAction({ signerAuth: auth }));
            }
            window.location.reload();
            
        },
        (err) =>{

        },
        ()=>{
            this.showSwitchGroup = false;
        }

    )
    }

    private fetchIfoOnCurrentUser()
    {

        this.user$ = this.userApiService.getCurrentUser();
        let language ;
        this.userSubscription = this.user$.subscribe(
            user => {
                this.uiUser = user;
                if (user.companyLogo != null && user.companyLogo != '') {
                    let el = (<HTMLDivElement>document.getElementById("logo_image"));
                    if (el) {
                        
                        el.style.backgroundImage = `url(${user.companyLogo})`;
                        el.style.backgroundRepeat = 'no-repeat';
                        el.style.backgroundPosition ='center';
                        el.style.backgroundSize = 'contain';
                        
                    }
                }
                this.store.dispatch(new appActions.SetCurrentUserProgram({ program: user.program}));
                this.store.dispatch(new appActions.SetCurrentUserConfiguration({userConfiguration: user.userConfiguration}));
                this.store.dispatch(new appActions.SetDisplaySignerNameInSignature({enableDisplaySignerNameInSignature: user.enableDisplaySignerNameInSignature}));
                this.store.dispatch(new appActions.SetDisplayMeaningOfSignature({enableMeaningOfSignature: user.enableMeaningOfSignature}));
                this.store.dispatch(new appActions.SetEnableSignEidasSignatureFlow({enableSignEidasSignatureFlow: user.shouldSignEidasSignatureFlow}));
                this.store.dispatch(new appActions.SetCompanySigner1Details({ companySigner1Details: user.companySigner1Details}));
                this.store.dispatch(new appActions.SetSignerAuthDefault({enableVisualIdentityFlow: user.enableVisualIdentityFlow, 
                    shouldSendWithOTPByDefault : user.shouldSendWithOTPByDefault,
                    defaultSigningType : user.defaultSigningType,
                    enableVideoConferenceFlow : user.enableVideoConferenceFlow,
                }));


                this.store.dispatch(new actions.TabletsSupportAction({ EnableTabletSupport: user.enableTabletsSupport }));

                if(this.userApiService.authToken != "" && this.userApiService.authToken != null)
                {
                  let auth = new Signer1Credential();
                  auth.signerToken = this.userApiService.authToken;
                  this.store.dispatch(new appActions.SetSignerAuthAction({ signerAuth: auth }));
                }
                
                language = this.languages.find((l) => l.Enum === user.userConfiguration.language);
                this.lang.languageSelected(language);
                this.selectedOption = this.sharedService.getCurrentLanguage().Code == "en" ? this.translate.instant(`LANGUAGE.ENGLISH`) : this.translate.instant(`LANGUAGE.HEBREW`);
            }
        );                
    }
    public switchGroup()
    {
        this.userApiService.GetUserGroups().subscribe(
        groups => {
           this.groupsToSwitch = groups.groups.filter(x => x.groupId != this.uiUser.groupId);
           if(this.groupsToSwitch && this.groupsToSwitch.length > 0)
            {
                
                this.showSwitchGroup = true;
            }
                
        },
        err => {

        },
        () => {

        }

        );
    }
    public ngOnInit() {

       this.store.select<any>('appstate').subscribe((state: AppState) => {
            if(state.program)
            {
                this.uiViewLicense =  state.program.uiViewLicense;                     

            }
            
            if(state.ShouldFetchInfoOnCurrentUser)
                {
                    this.store.dispatch(new appActions.FetchCurrentUserInfo({ needToFetch: false}));
                    this.fetchIfoOnCurrentUser();

                }
        });

        this.fetchIfoOnCurrentUser();
       
        

    }

  
    public showLogOutPopup() {
        this.translate.get(['LOGOUT.MODALTITLE', 'LOGOUT.MODALTEXT', 'REGISTER.CANCEL', 'BUTTONS.SUBMIT'])
        .subscribe((res: object) => {
            let keys = Object.keys(res);
            this.popUpModal.title = res[keys[0]];
            this.popUpModal.content = res[keys[1]];
            this.popUpModal.rejectBtnText = res[keys[2]];
            this.popUpModal.confirmBtnText = res[keys[3]];
            this.popUpModal.showModal = true;
        });
        
    }
    public doClickOutside()
    {
        if(this.manuShow )
        {
            this.manuShow = false;
        }
    }

    public logout() {
        this.userApiService.logout();
    }
    public clearSession(){
        sessionStorage.removeItem("FIELDS_ARRAY");
        sessionStorage.removeItem("CONTACTS");
        sessionStorage.removeItem("SIGNERS");
    }
}
