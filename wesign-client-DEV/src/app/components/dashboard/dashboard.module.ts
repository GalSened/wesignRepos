import { NgModule } from "@angular/core";

import { CommonModule } from "@angular/common";
import { HttpClient } from "@angular/common/http";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { EffectsModule } from "@ngrx/effects";
import { TranslateLoader, TranslateModule } from "@ngx-translate/core";
import { TranslateHttpLoader } from "@ngx-translate/http-loader";
import { ContactApiService } from "@services/contact-api.service";
import { DocumentApiService } from "@services/document-api.service";
import { PagerService } from "@services/pager.service";
import { SharedService } from "@services/shared.service";
import { StateProcessService } from "@services/state-process.service";
import { TemplateApiService } from "@services/template-api.service";
import { UserApiService } from "@services/user-api.service";
import { AngularDraggableModule } from "angular2-draggable";

import { SignalRConfiguration } from "ng2-signalr";
import { SignalRModule } from "ng2-signalr";
import { DebounceModule } from "ngx-debounce";
import { DndModule } from "ngx-drag-drop";
import { NgxFilesizeModule } from "ngx-filesize";
import { COMPONENTS } from "./dashboard-components.list";
import { DashboardRoutingModule } from "./dashboard-routing.module";
import { OrderByPipe } from "../../pipes/orderBy.pipe";
import { SelfSignApiService } from '@services/self-sign-api.service';
import { HelperModule } from '../../modules/helper.module';
import { PdfJsViewerModule } from 'ng2-pdfjs-viewer';
import { NgxCleaveDirectiveModule } from 'ngx-cleave-directive';
import { GroupAssignService } from '@services/group-assign.service';
import { AddUserComponent } from './main/managment/add-user/add-user.component';
import { DocumentMethodSelectorComponent } from './main/signers/document-method-selector/document-method-selector.component';
import { NgxSignaturePadModule } from '@o.krucheniuk/ngx-signature-pad';

import { TemplateSelectComponent } from './main/signers/template-select/template-select.component';
import { ServerSignComponent } from './selfsign/server-sign/server-sign.component';
import { SelectContactComponent } from './main/contacts/select-contact/select-contact.component';
import { SignersReviewComponent } from './main/signers/signers-review/signers-review.component';
import { NoteComponent } from './main/signers/signers-review/note/note.component';
import { AppendicesComponent } from './main/signers/signers-review/appendices/appendices.component';
import { AttachmentsComponent } from './main/signers/signers-review/attachments/attachments.component';
import { OtpComponent } from './main/signers/signers-review/otp/otp.component';
import { SelectTabletComponent } from './main/contacts/select-tablet/select-tablet.component';
import { ShareDocComponent } from './main/documents/share-doc/share-doc.component';
import { UploadTemplateComponent } from './main/templates/upload-template/upload-template.component';
import {Ng2TelInputModule} from 'ng2-tel-input';
import { DocumentSentSuccessfullyComponent } from './document-sent-successfully/document-sent-successfully.component';
import { DocumentsLinksComponent } from './main/documents/documents-links/documents-links.component';
import { CertificatesignComponent } from './selfsign/certificate-sign/certificate-sign.component';
import { CertificateSignUploadComponent } from './selfsign/certificate-sign/certificate-sign-upload/certificate-sign-upload.component';
import { ColorPickerModule } from 'ngx-color-picker';
import { ReplaceSignerComponent } from './main/documents/replace-signer/replace-signer.component';
import { UploadFileButtonComponent } from './main/home-buttons/upload-file-button/upload-file-button.component';
import { UploadFileAreaComponent } from './main/templates/upload-template/upload-file-area/upload-file-area.component';
import { DistributionApiService } from '@services/distribution-api.service';
import { SelectMultiContactsComponent } from './main/contacts/select-multi-contacts/select-multi-contacts.component';
import { DistributionDocumentsComponent } from './main/documents/distribution-documents/distribution-documents.component';
import { DocumentsComponent } from './main/documents/documents.component';
import { CountriesPhoneData } from '@services/countries-phone-data.service';
import { VisualIdentificationComponent } from './main/signers/signers-review/visual-Identification/visual-identification.component';
import { NgSelectModule } from '@ng-select/ng-select';
import { MergeFilesComponent } from './main/merge-files/merge-files.component';

import { MergeSelectTemplatsComponent } from './main/merge-files/merge-select-templats/merge-select-templats.component';
import { MergeFileSelectorComponent } from './main/merge-files/merge-file-selector/merge-file-selector.component';
import { ManageContactsComponent } from './profile/manage-contacts/manage-contacts.component';
import { AddEditContactsGroupComponent } from './profile/manage-contacts/add-edit-contacts-group/add-edit-contacts-group.component';
import { SelectContactsGroupComponent } from './main/contacts/select-contacts-group/select-contacts-group.component';
import { ReportsComponent } from './reports/reports.component';
import { ReportFilterComponent } from './reports/report-filter/report-filter.component';
import { ReportOutputTableComponent } from './reports/report-output-table/report-output-table.component';
import { AddReportFrequencyComponent } from './reports/report-filter/add-report-frequency/add-report-frequency.component';
import { DuplicateFieldToPagesComponent } from './groupsign/duplicate-field-to-pages/duplicate-field-to-pages.component';
import { SmartCardSigningAlertComponent } from './selfsign/smart-card-signing-alert/smart-card-signing-alert.component';
import { EidasSigningFlowAlertComponent } from './selfsign/eidas-signing-flow-alert/eidas-signing-flow-alert.component';
import { VideoConfrenceComponent } from './main/signers/signers-review/video-confrence/video-confrence.component';
import { EditUserPhoneComponent } from './profile/edit-user-phone/edit-user-phone.component';
import { SingleLinkComponent } from './main/templates/single-link/single-link.component';
// import { GovSignUploadComponent } from './selfsign/gov-sign/gov-sign-upload/gov-sign-upload.component';

// Analytics Dashboard Components
import { AnalyticsDashboardComponent } from './analytics-dashboard/analytics-dashboard.component';
import { KpiCardsComponent } from './analytics-dashboard/kpi-cards/kpi-cards.component';
import { UsageChartsComponent } from './analytics-dashboard/usage-charts/usage-charts.component';
import { SegmentationChartsComponent } from './analytics-dashboard/segmentation-charts/segmentation-charts.component';
import { ProcessFlowComponent } from './analytics-dashboard/process-flow/process-flow.component';
import { AnalyticsApiService } from '@services/analytics-api.service';
import { AnalyticsLoadingService } from '@services/analytics-loading.service';
import { AnalyticsErrorHandlerService } from '@services/analytics-error-handler.service';



export function createConfig(): SignalRConfiguration {
    const c = new SignalRConfiguration();
    c.logging = true;
    c.executeEventsInZone = true; // optional, default is true
    c.executeErrorsInZone = false; // optional, default is false
    c.executeStatusChangeInZone = true; // optional, default is true
    return c;
}

export function HttpLoaderFactory(http: HttpClient) {
    return new TranslateHttpLoader(http, "./assets/i18n/", ".json");
}

@NgModule({
    declarations: [
        ...COMPONENTS,
        OrderByPipe,
        DocumentMethodSelectorComponent,
        TemplateSelectComponent,
        ServerSignComponent,
        SelectContactComponent,
        SignersReviewComponent,
        NoteComponent,
        AppendicesComponent,
        AttachmentsComponent,
        OtpComponent,
        SelectTabletComponent,
        ShareDocComponent,
        UploadTemplateComponent,
        DocumentSentSuccessfullyComponent,
        DocumentsLinksComponent,
        CertificatesignComponent,
        // GovSignUploadComponent,
        CertificateSignUploadComponent,
        ReplaceSignerComponent,
        UploadFileButtonComponent,
        UploadFileAreaComponent,
        SelectMultiContactsComponent,
        DistributionDocumentsComponent,
        VisualIdentificationComponent,
        MergeFilesComponent,        
        MergeSelectTemplatsComponent, 
        MergeFileSelectorComponent, 
        ManageContactsComponent, 
        SmartCardSigningAlertComponent, EidasSigningFlowAlertComponent,
        AddEditContactsGroupComponent, SelectContactsGroupComponent, ReportsComponent, ReportFilterComponent, ReportOutputTableComponent, AddReportFrequencyComponent, DuplicateFieldToPagesComponent, VideoConfrenceComponent, EditUserPhoneComponent, SingleLinkComponent, /*GovSignUploadComponent*/
        // Analytics Dashboard Components
        AnalyticsDashboardComponent,
        KpiCardsComponent,
        UsageChartsComponent,
        SegmentationChartsComponent,
        ProcessFlowComponent,
        // TODO - move to helper module
    ],
    exports: [],
    imports: [
        DashboardRoutingModule,
        FormsModule,
        NgSelectModule,
        ReactiveFormsModule,
        CommonModule,
        DebounceModule,
        NgxFilesizeModule,
        AngularDraggableModule,
      
        SignalRModule.forRoot(createConfig),
        EffectsModule.forFeature([]),
        //TranslateModule.forChild({}),
        DndModule,
        HelperModule,
        PdfJsViewerModule,
        NgxCleaveDirectiveModule,
        NgxSignaturePadModule,
        Ng2TelInputModule ,
        ColorPickerModule

    ],
    providers: [
        TemplateApiService,
        PagerService,
        UserApiService,
        SharedService,
        CountriesPhoneData,
        DocumentApiService,
        DistributionApiService,
        ContactApiService,
        StateProcessService,
        SelfSignApiService,
        GroupAssignService,
        AnalyticsApiService,
        AnalyticsLoadingService,
        AnalyticsErrorHandlerService
    ],
})
export class DashboardModule {

}

      