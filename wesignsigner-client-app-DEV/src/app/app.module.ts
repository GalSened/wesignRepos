import { BrowserModule } from '@angular/platform-browser';
import { APP_INITIALIZER, CUSTOM_ELEMENTS_SCHEMA , NgModule } from '@angular/core';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { FirstHeaderComponent as FirstHeaderComponent } from './components/first-header/first-header.component';
import { MainSignerComponent } from './components/main-signer/main-signer.component';
import { SecondHeaderComponent } from './components/second-header/second-header.component';
import { MenuComponent } from './components/menu/menu.component';
import { DocumentsGroupComponent } from './components/documents-group/documents-group.component';
import { DocumentPageComponent } from './components/document-page/document-page.component';
import { ViewSettingsComponent } from './components/view-settings/view-settings.component';
import { FeatherModule } from 'angular-feather';
import { allIcons } from 'angular-feather/icons';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { AppConfigService } from './services/app-config.service';
import { SignatureFieldComponent } from './components/fields/signature-field/signature-field.component';
import { SignPadComponent } from './components/sign-pad/sign-pad.component';
import { NgxSignaturePadModule } from '@o.krucheniuk/ngx-signature-pad';
import { SuccessPageComponent } from './components/success-page/success-page.component';
import { AlertComponent } from './components/alert/alert.component';
import { LoaderComponent } from './components/loader/loader.component';
import { TextFieldComponent } from './components/fields/text-field/text-field.component';
import { FormsModule } from '@angular/forms';
import { ErrorComponent } from './components/error/error.component';
import { DownloadDocumentComponent } from './components/download-document/download-document.component';
import { CheckBoxFieldComponent } from './components/fields/check-box-field/check-box-field.component';
import { ChoiceFieldComponent } from './components/fields/choice-field/choice-field.component';
import { RadioFieldComponent } from './components/fields/radio-field/radio-field.component';
import { RadioGroupFieldComponent } from './components/fields/radio-group-field/radio-group-field.component';
import { ServerSignComponent } from './components/server-sign/server-sign.component';
import { SenderViewComponent } from './components/sender-view/sender-view.component';
import { DeclineComponent } from './components/decline/decline.component';
import { OtpDetailsComponent } from './components/otp-details/otp-details.component';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { TranslateModule, TranslateLoader, } from '@ngx-translate/core';
import { ClickOutsideModule } from 'ng-click-outside';
import { LangComponent } from './components/lang/lang.component';
import { AttachmentsComponent } from './components/attachments/attachments.component';
import { AttachmentComponent } from './components/attachment/attachment.component';
import { NotesComponent } from './components/notes/notes.component';
import { AppendicesComponent } from './components/appendices/appendices.component';
import { SingleLinkComponent } from './components/single-link/single-link.component';
import { PrivacyPolicyComponent } from './components/privacy-policy/privacy-policy.component';
import { TermsOfUseComponent } from './components/terms-of-use/terms-of-use.component';
import { AgentViewComponent } from './components/agent-view/agent-view.component';
import { DeclinePageComponent } from './components/decline-page/decline-page.component';
import { PopUpMessageComponent } from './components/pop-up-message/pop-up-message.component';
import { DatePipe } from '@angular/common';
import { SmartCardAlertComponent } from './components/alert/smart-card-alert/smart-card-alert.component';
import { Ng2TelInputModule } from 'ng2-tel-input';
import { NgxSpinnerModule } from 'ngx-spinner';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { OauthComsignIdpComponent } from './components/oauth-comsign-idp/oauth-comsign-idp.component';
import { IdentitySucsessComponent } from './components/oauth-comsign-idp/identity-sucsess/identity-sucsess.component';
import { NgSelectModule } from '@ng-select/ng-select';
import { ConfirmMessagePopUpComponent } from './confirm-message-pop-up/confirm-message-pop-up.component';
import { HelperModule } from './modules/helper.module';
import { MeaningOfSignatureComponent } from './components/sign-pad/meaning-of-signature/meaning-of-signature.component';
import { IedasSigningFlowComponent } from './components/iedas-signing-flow/iedas-signing-flow.component';
import { IedasSigningFlowAfterFirstAuthComponent } from './components/iedas-signing-flow-after-first-auth/iedas-signing-flow-after-first-auth.component';
import { OtpExceededPageComponent } from './components/otp-exceeded-page/otp-exceeded-page.component';

export function HttpLoaderFactory(http: HttpClient, appservice: AppConfigService) 
{
  
  let isLocalhost = 
    window.location.hostname === 'localhost' ||
    // [::1] is the IPv6 localhost address.
    window.location.hostname === '[::1]' ||
    // 127.0.0.1/8 is considered localhost for IPv4.
    window.location.hostname.match(
        /^127(?:\.(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)){3}$/);
    
 let translatePath = appservice.baseUrl + "./style/assets/i18n/";
if(! isLocalhost)
{
  translatePath = appservice.baseUrl + "/style/assets/i18n/";
}
  
  return new TranslateHttpLoader(http, translatePath, ".json");
}

@NgModule({
  declarations: [
    AppComponent,
    FirstHeaderComponent,
    MainSignerComponent,
    SecondHeaderComponent,
    MenuComponent,

    DocumentsGroupComponent,
    DocumentPageComponent,
    ViewSettingsComponent,
    SignatureFieldComponent,
    SignPadComponent,
    SuccessPageComponent,
    AlertComponent,
    LoaderComponent,
    TextFieldComponent,
    ErrorComponent,
    DownloadDocumentComponent,
    CheckBoxFieldComponent,
    ChoiceFieldComponent,
    RadioFieldComponent,
    RadioGroupFieldComponent,
    ServerSignComponent,
    SenderViewComponent,
    DeclineComponent,
    OtpDetailsComponent,
    LangComponent,
    AttachmentsComponent,
    AttachmentComponent,
    NotesComponent,
    AppendicesComponent,
    SingleLinkComponent,
    PrivacyPolicyComponent,
    TermsOfUseComponent,
    AgentViewComponent,
    DeclinePageComponent,
    PopUpMessageComponent,
    SmartCardAlertComponent,
    OauthComsignIdpComponent,
    IdentitySucsessComponent,
    ConfirmMessagePopUpComponent,
    MeaningOfSignatureComponent,
    IedasSigningFlowComponent,
    IedasSigningFlowAfterFirstAuthComponent,
    OtpExceededPageComponent,
    
  ],
  imports: [
    TranslateModule.forRoot({
      loader: {
        provide: TranslateLoader,
        deps: [HttpClient, AppConfigService],
        useFactory: HttpLoaderFactory,
      }
      //,
      // useDefaultLang: true,
      // isolate: false
    }),

    BrowserModule,
    AppRoutingModule,
    FeatherModule.pick(allIcons),
    HttpClientModule,
    NgxSignaturePadModule,
    FormsModule,
    NgSelectModule,
    Ng2TelInputModule,
    ClickOutsideModule,
    NgxSpinnerModule,
    BrowserAnimationsModule,
    HelperModule
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  providers: [
    [
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
      DatePipe
      // ,
      //   {
      //     // processes all errors
      //     provide: ErrorHandler,
      //     useClass: GlobalErrorHandler,
      //     useFactory: (appConfigService: AppConfigService) => {
      //       return () => {
      //         return appConfigService.loadAppConfig();
      //       };
      //     }
      //   }
    ]
  ],

  bootstrap: [AppComponent]
})
export class AppModule { }