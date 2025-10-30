import { NgModule } from '@angular/core';
import { ClickOutsideDirective } from '../directives/click-outside.directive';
import { TogglePasswordDirective } from '../directives/toggle-password.directive';
import { TranslateModule, TranslateLoader, TranslateStore, TranslateService } from '@ngx-translate/core';
import { HttpClient } from '@angular/common/http';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { Actions, ofType } from "@ngrx/effects";
import * as appActions from "../../app/state/actions/app.actions";
import { IconsModule } from './icons.modules';
import { PlanComponent } from '@components/dashboard/plans-payment/plan/plan.component';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { FooterComponent } from '@components/shared/footer.component';
import { RouterModule } from '@angular/router';
import { LangSelectorComponent } from '@components/shared/lang-selector.component';
import { TopHeaderTitleComponent } from '@components/shared/top-header-title.component';
import { SidePageMenuComponent } from '@components/shared/side-page-menu.component';
import { LoadingComponent } from '@components/shared/loading.component';
import { PopUpConfirmComponent } from '@components/shared/controls/pop-up-confirm/pop-up-confirm.component';
import { NgSelectModule } from '@ng-select/ng-select';
import { PopUpOkComponent } from '@components/shared/controls/pop-up-ok/pop-up-ok.component';
import { AlertComponent } from '@components/shared/alert.component';

export function HttpLoaderFactory(http: HttpClient) {
    return new TranslateHttpLoader(http, "./assets/i18n/", ".json");
}

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        NgSelectModule,
        RouterModule,
        TranslateModule.forRoot({
            loader: {
                deps: [HttpClient],
                provide: TranslateLoader,
                useFactory: HttpLoaderFactory,
            },
            useDefaultLang: true,
            isolate: false
        }),
        IconsModule
    ],
    exports: [
        ClickOutsideDirective,
        PopUpConfirmComponent,
        PopUpOkComponent,
        TogglePasswordDirective,
        TranslateModule,
        PlanComponent,
        FooterComponent,
        IconsModule,
        LangSelectorComponent,
        TopHeaderTitleComponent,
        SidePageMenuComponent,
        LoadingComponent,
        AlertComponent
    ],
    declarations: [
        ClickOutsideDirective,
        PopUpConfirmComponent,
        PopUpOkComponent,
        TogglePasswordDirective,
        PlanComponent,
        FooterComponent,
        LangSelectorComponent,
        TopHeaderTitleComponent,
        SidePageMenuComponent,
        LoadingComponent,
        AlertComponent
    ],
    providers: [
        TranslateStore
    ],
})
export class HelperModule {
    constructor(private translate: TranslateService, private actions$: Actions) {
        translate.setDefaultLang("en");
        translate.use("en");
        this.actions$.pipe(ofType(appActions.LANGUAGE_STATE)).subscribe((action: appActions.SetLangAction) => {
            this.translate.use(action.payload.Language)
        });
    }
}
