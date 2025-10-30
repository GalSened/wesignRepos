import { FormsModule } from '@angular/forms';
import { HTTP_INTERCEPTORS, HttpClientModule } from "@angular/common/http";
import { NgModule, APP_INITIALIZER, ErrorHandler } from "@angular/core";
import { ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from "@angular/platform-browser";
import { StoreModule } from "@ngrx/store";
import { UnauthorizedInterceptor } from "./interceptors/authorization.interceptor";
import { AppRoutingModule } from "./app-routing.module";
import { AppComponent } from "./app.component";
import { COMPONENTS } from "./app-components.list";
import { SERVICES } from "./app.services";
import { EffectsModule } from "@ngrx/effects";
import { appStateReducer } from "@state/reducers/app.reducer";
import { PIPES } from './app-pipes.list';
import { HeaderInterceptor } from './interceptors/headers.interceptor';
import { HelperModule } from './modules/helper.module';
import { CommonModule, DatePipe } from '@angular/common';
import { AppConfigService } from '@services/app-config.service';
import { DeviceDetectorService } from 'ngx-device-detector';
import { NgxSignaturePadModule } from '@o.krucheniuk/ngx-signature-pad';
import { RecaptchaModule, RecaptchaFormsModule } from 'ng-recaptcha';
import { ExternalloginComponent } from './components/login/externallogin/externallogin.component';
import { NgSelectModule } from '@ng-select/ng-select';
import { OtpLoginFlowComponent } from './components/login/otp-login-flow/otp-login-flow.component';
import { SwitchGroupModalComponent } from './components/shared/controls/switch-group-modal/switch-group-modal.component';
import { ExpiredPasswordComponent } from './components/login/expired-password/expired-password.component';



declare module "@angular/core" {
    interface ModuleWithProviders<T = any> {
      ngModule: Type<T>;
     
    }
  }


@NgModule({
    bootstrap: [AppComponent],
    declarations: [
        AppComponent,
        ...COMPONENTS,
        ...PIPES,
        ExternalloginComponent,
        OtpLoginFlowComponent,
        SwitchGroupModalComponent,
        ExpiredPasswordComponent,
        
        
        // TODO - move to helper module
    ],
    imports: [
        BrowserModule,
        CommonModule,
        AppRoutingModule,
        ReactiveFormsModule,
        FormsModule,
        NgSelectModule,
        HttpClientModule,
        NgxSignaturePadModule,
        StoreModule.forRoot({
            appstate: appStateReducer,
        },{
        runtimeChecks: {
            strictStateImmutability: false,
            strictActionImmutability: false,
          }}),
        EffectsModule.forRoot([]),
        HelperModule,
        RecaptchaModule,
        RecaptchaFormsModule, 
    ],
    providers: [
        AppConfigService,
        DeviceDetectorService,
        {
            provide: APP_INITIALIZER,
            multi: true,
            deps: [AppConfigService],
            useFactory: (appConfigService: AppConfigService) => {
                return () => {
                    return appConfigService.loadAppConfig();
                };
            }
        },
        DatePipe,
        ...SERVICES,
        [
            // { 
            //     // processes all errors
            //     provide: ErrorHandler, 
            //     useClass: GlobalErrorHandler 
            // },
            {
                multi: true,
                provide: HTTP_INTERCEPTORS,
                useClass: UnauthorizedInterceptor,
            },
            {
                provide: HTTP_INTERCEPTORS,
                useClass: HeaderInterceptor,
                multi: true // Add this line when using multiple interceptors.
            },
        ],
    ],

})
export class AppModule { }
