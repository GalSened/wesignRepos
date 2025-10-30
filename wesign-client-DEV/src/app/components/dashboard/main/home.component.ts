import * as selectActions from "@state/actions/selection.actions";
import { Component, OnInit, ElementRef, ViewChild, OnDestroy } from "@angular/core";
import { Router } from "@angular/router";
import { FLOW_STEP } from "@models/enums/flow-step.enum";
import { Store } from "@ngrx/store";
import { SharedService } from "@services/shared.service";
import * as appActions from "@state/actions/app.actions";
import { IAppState, AppState, AlertLevel } from "@state/app-state.interface";
import { Observable, Subscription } from "rxjs";
import * as documentActions from "@state/actions/document.actions";
import { environment } from "../../../../environments/environment"
import { UserApiService } from '@services/user-api.service';
import { PageField } from '@models/template-api/page-data-result.model';
import { GroupAssignService } from '@services/group-assign.service';
import * as fieldsActions from "@state/actions/fields.actions";
import { User } from '@models/account/user.model';
import { UploadRequest } from '@models/template-api/upload-request.model';
import { ConfigurationApiService } from '@services/configuration-api.service';
import { UiViewLicense } from '@models/program/user-program.model';

@Component({
    selector: "sgn-home-component",
    templateUrl: "home.component.html",
})

export class HomeComponent implements OnInit, OnDestroy {

    appState: IAppState;
    state$: Observable<AppState>;
    tiny = environment.tiny;
    // isGovSign = false;

    @ViewChild("fileInput") fileInput: ElementRef

    file: any = null;
    certFile: any = null;
    busy: boolean = false;
    createSub: Subscription;
    storeSub: Subscription;
    showSigner1ExtraSigningTypes = false;
    showSigner1FileTypes = false;
    showUploadTemplate = false;
    user: User;
    uiViewLicense: UiViewLicense;

    constructor(private store: Store<IAppState>, private sharedService: SharedService, private userApiService: UserApiService,
        private groupAssignService: GroupAssignService, private configurationApiService: ConfigurationApiService, public router: Router) {
    }

    ngOnDestroy(): void {
        if (this.createSub)
            this.createSub.unsubscribe();
        if (this.storeSub)
            this.storeSub.unsubscribe();
    }

    ngOnInit() {
        this.groupAssignService.updateFieldsMap(new Map<string, PageField[]>());
        this.groupAssignService.useMeaningOfSignature = false;
        this.store.dispatch(new documentActions.ClearFileUploadRequestAction({}));
        this.store.dispatch(new fieldsActions.ClearAllFieldsAction({}))
        this.store.dispatch(new selectActions.ClearTemplateSelectionAction({}))
        

        if (this.tiny)
            this.sharedService.setFlowState("tinysign", FLOW_STEP.TINY_SIGN_UPLOAD);
        else
            this.sharedService.setFlowState("none", FLOW_STEP.NONE);

        this.storeSub = this.store.select<any>('appstate').subscribe((state: AppState) => {
            if (state.program) {
                this.uiViewLicense = state.program.uiViewLicense;
            }
        });

        this.configurationApiService.readInitConfiguration().subscribe((data) => {
            this.showSigner1ExtraSigningTypes = data.enableSigner1ExtraSigningTypes;
        });

        this.createSub = this.userApiService.getCurrentUser().subscribe(
            (user) => {
                this.user = user;
                this.store.dispatch(new appActions.SetCurrentUserDetailesAction({ userName: user.name, userType: user.type }));
                this.store.dispatch(new appActions.TabletsSupportAction({ EnableTabletSupport: user.enableTabletsSupport }));
                this.store.dispatch(new appActions.SetSignerAuthDefault({
                    enableVisualIdentityFlow: user.enableVisualIdentityFlow,
                    shouldSendWithOTPByDefault: user.shouldSendWithOTPByDefault,
                    defaultSigningType: user.defaultSigningType,
                    enableVideoConferenceFlow: user.enableVideoConferenceFlow,
                }));
            });
    }

    onTemplatesClick() {
        sessionStorage.removeItem("CONTACTS");
        sessionStorage.removeItem("SIGNERS");
    }

    public async fileDropped() {
        if (!this.busy && this.fileInput.nativeElement.files.length > 0) {
            this.file = this.fileInput.nativeElement.files[0];
            const userData = await this.userApiService.getUserProgramStatus();

            if (userData.remeiningDocs == 0 || (userData.isFreeTrial && userData.isExpired)) {
                this.sharedService.setTranslateAlert("TINY.PAYMENT.DOCUMENT_COUNT_EXPIERED", AlertLevel.ERROR);

                if (this.tiny)
                    setTimeout(() => { this.router.navigate(["dashboard", "profile", "plans"]); }, 3000);

                return;
            }

            if (this.file) {
                this.busy = true;
                this.sharedService.setBusy(true, "DOCUMENT.UPLOADING");
                const reader = new FileReader();
                reader.readAsDataURL(this.file);
                reader.onload = () => {
                    const uploadRequest = new UploadRequest();
                    let array = this.file.name.split(".");
                    let name = "";
                    for (let index = 0; index < array.length - 1; index++) {
                        name += array[index];
                        if (index != array.length - 2) {
                            name += '.'
                        }
                    }

                    uploadRequest.Name = name;
                    uploadRequest.Base64File = reader.result.toString();

                    this.store.dispatch(new documentActions.SetFileUploadRequestAction({ fileUploadRequest: uploadRequest }));
                    this.store.dispatch(new selectActions.ClearTemplateSelectionAction({}));
                    this.sharedService.setBusy(false);
                    this.busy = false;
                    this.router.navigate(["dashboard", "selectsigners"]);
                    this.sharedService.setBusy(false);
                };
            } 
            
            else {
                this.busy = false;
                this.router.navigate(["dashboard", "selectsigners"]);
                this.sharedService.setTranslateAlert("GLOBAL.PLEASE_SELECT_FILE", AlertLevel.ERROR);
            }
        }
    }
}