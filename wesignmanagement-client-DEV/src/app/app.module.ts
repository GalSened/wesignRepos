import { SharedService } from 'src/app/services/shared.service';
import { BrowserModule } from '@angular/platform-browser';
import { NgModule, APP_INITIALIZER } from '@angular/core';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { LoginComponent } from './components/login/login.component';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';
import { UsersApiService } from 'src/app/services/users-api.service';
import { AppEffects } from './app.effects';
import { FormsModule,  ReactiveFormsModule } from '@angular/forms';
import { HeaderComponent } from './components/header/header.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { SidebarComponent } from './components/dashboard/sidebar/sidebar.component';
import { CompaniesComponent } from './components/dashboard/companies/companies.component';
import { ManagementGuard } from './ManagementGuard';
import { ProgramsComponent } from './components/dashboard/programs/programs.component';
import { UsersComponent } from './components/dashboard/users/users.component';
import { ConfigurationComponent } from './components/dashboard/configuration/configuration.component';
import { LogsComponent } from './components/dashboard/logs/logs.component';
import { CompanyFormComponent } from './components/dashboard/companies/company-form/company-form.component';
import { ProgramFormComponent } from './components/dashboard/programs/program-form/program-form.component';
import { CustomDropDownComponent } from './components/shared/custom-drop-down/custom-drop-down.component';
import { PagerService } from './services/pager.service';
import { PaginatorComponent } from './components/shared/paginator.component';
import { CalendarComponent } from './components/shared/calendar/calendar.component';
import { AlertComponent } from './components/alert/alert.component';
import { ChangePasswordComponent } from './components/dashboard/configuration/change-password/change-password.component';
import { UnauthorizedInterceptor } from './interceptors/authorization.interceptor';
import { BannerAlertComponent } from './components/shared/banner-alert/banner-alert.component';
import { appStateReducer } from './state/app.reducer';
import { AppConfigService } from './services/app-config.service';
import { LicenseComponent } from './components/dashboard/license/license.component';
import { GenerateRequestComponent } from './components/dashboard/license/generate-request/generate-request.component';
import { LoadLicenseComponent } from './components/dashboard/license/load-license/load-license.component';
import { ShowPropertiesComponent } from './components/dashboard/license/show-properties/show-properties.component';
import { EulaComponent } from './components/dashboard/license/eula/eula.component';
import { QrCodeComponent } from './components/dashboard/qr-code/qr-code.component';
import { OTPApiService } from './services/otp-api.service';
import { ToolTipComponent } from './components/shared/tool-tip/tool-tip.component';
import { HighlightPipe } from './pipes/highlight.pipe';
import { NewlinePipe } from './pipes/newline.pipe';
import { ActiveDirectoryMapperComponent } from './components/dashboard/companies/active-directory-mapper/active-directory-mapper.component';
import { NgSelectModule } from '@ng-select/ng-select';
import { UserFormComponent } from './components/dashboard/users/user-form/user-form.component';
import { HtmlTemplateFormComponent } from './components/dashboard/users/html-template-form/html-template-form.component';
import {Ng2TelInputModule} from 'ng2-tel-input';
import { NgxSpinnerModule } from "ngx-spinner";  
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import { ReportsComponent } from './components/dashboard/reports/reports.component';
import { ReportFilterComponent } from './components/dashboard/reports/report-filter/report-filter.component';
import { ReportOutputTableComponent } from './components/dashboard/reports/report-output-table/report-output-table.component';
import { ClickOutsideDirective } from './directives/click-outside.directive';
import { UtilsService } from './services/utils.service';
@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    HeaderComponent,
    DashboardComponent,
    SidebarComponent,
    CompaniesComponent,
    ProgramsComponent,
    UsersComponent,
    ConfigurationComponent,
    LogsComponent,
    CompanyFormComponent,
    ProgramFormComponent,
    CustomDropDownComponent,
    PaginatorComponent,
    CalendarComponent,
    AlertComponent,
    ChangePasswordComponent,
    BannerAlertComponent,
    LicenseComponent,
    GenerateRequestComponent,
    LoadLicenseComponent,
    ShowPropertiesComponent,
    EulaComponent,
    QrCodeComponent,
    ToolTipComponent,    
    HighlightPipe, NewlinePipe, ActiveDirectoryMapperComponent, UserFormComponent, HtmlTemplateFormComponent, ReportsComponent,
     ReportFilterComponent, ReportOutputTableComponent,
    ClickOutsideDirective
  ],
  imports: [
    BrowserAnimationsModule,
    BrowserModule,
    AppRoutingModule,
    FormsModule,    
    NgSelectModule,
    ReactiveFormsModule,
    Ng2TelInputModule,
    NgxSpinnerModule,    
    HttpClientModule,
    StoreModule.forRoot({
      appstate: appStateReducer,
    }),
    EffectsModule.forRoot([AppEffects])
  ],
  providers: [
    
    AppConfigService,
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
    UsersApiService,
    PagerService,
    ManagementGuard,
    OTPApiService,
    SharedService,
    UtilsService,
    [
    {
      multi: true,
      provide: HTTP_INTERCEPTORS,
      useClass: UnauthorizedInterceptor,
    }
    ]
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
