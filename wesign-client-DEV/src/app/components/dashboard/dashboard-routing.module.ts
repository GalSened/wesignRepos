import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { CanActivateGuard } from "../../guards/can-activate.guard";
import { DocViewComponent } from "./document/doc-view.component";
import { MainComponent } from "./main/main.component";

import { PasswordChangeComponent } from "./profile/password-change.component";
import { ProfileComponent } from "./profile/profile.component";
import { SelfSignPlaceFieldsComponent } from "./selfsign/self-sign-place-fields.component";
import { SelfSignUploadComponent } from "./selfsign/self-sign-upload.component";
import { TemplateEditComponent } from "./template/template-edit.component";
import { GroupSignAssignFieldsComponent } from './groupsign/group-sign-assign-fields';
import { TinySendDocumentComponent } from './tiny-send-document/tiny-send-document.component';
import { TinySignPlaceFieldsComponent } from './tiny-send-document/tiny-sign-place-fields/tiny-sign-place-fields.component';
import { PlansPaymentComponent } from './plans-payment/plans-payment.component';
import { ManagmentComponent } from './main/managment/managment.component';
import { HomeComponent } from './main/home.component';
import { ProfileWrapperComponent } from './profile/profile-wrapper.component';
import { DocumentsComponent } from './main/documents/documents.component';
import { SignersComponent } from './main/signers/signers.component';
import { ContactsComponent } from './main/contacts/contacts.component';
import { TemplatesComponent } from './main/templates/templates.component';
import { SignersReviewComponent } from './main/signers/signers-review/signers-review.component';
import { DocumentSentSuccessfullyComponent } from './document-sent-successfully/document-sent-successfully.component';
import { DocumentsLinksComponent } from './main/documents/documents-links/documents-links.component';
import { CertificatesignComponent } from './selfsign/certificate-sign/certificate-sign.component';
import { DistributionDocumentsComponent } from './main/documents/distribution-documents/distribution-documents.component';
import { MergeFilesComponent } from './main/merge-files/merge-files.component';
import { ManageContactsComponent } from './profile/manage-contacts/manage-contacts.component';
import { ReportsComponent } from './reports/reports.component';
import { AnalyticsDashboardComponent } from './analytics-dashboard/analytics-dashboard.component';
// import { GovSignComponent } from './selfsign/gov-sign/gov-sign.component';


const routes: Routes = [
    { path: "",
    pathMatch: "full",
    redirectTo: "main" },
    {
        canActivate: [CanActivateGuard],
        component: ContactsComponent,
        path: "contacts",

    },
    {
        canActivate: [CanActivateGuard],
        component: TemplatesComponent,
        path: "templates",

    },
    // {
    //     canActivate: [CanActivateGuard],        
    //     path: "",
    //     pathMatch: "full",
    //     redirectTo: "main/documents",
    // },
    {
        canActivate: [CanActivateGuard],
        component: HomeComponent,
        path: "main",
    },
     {
         canActivate: [CanActivateGuard],
         component: DocumentSentSuccessfullyComponent,
        path: "success",
     },
     {
        canActivate: [CanActivateGuard],
        component: DocumentSentSuccessfullyComponent,
       path: "success/selfsign",
    },
    {
        canActivate: [CanActivateGuard],
        component: MainComponent,
        path: "main/templates/:flow",
    },
    {
        canActivate: [CanActivateGuard],
        component: ManagmentComponent,
        path: "main/managment",
    },
    {
        canActivate: [CanActivateGuard],
        component: MergeFilesComponent,
        path: "mergefiles",

    },
    {
        canActivate: [CanActivateGuard],
        component: ReportsComponent,
        path: "reports"
    },
    {
        canActivate: [CanActivateGuard],
        component: AnalyticsDashboardComponent,
        path: "analytics"
    },
    {
        canActivate: [CanActivateGuard],
        component: ProfileWrapperComponent,
        path: "profile",
        children: [
            { path: '', redirectTo: 'info', pathMatch:"prefix" },
            { path: 'info', component: ProfileComponent },
            { path: 'password', component: PasswordChangeComponent },
            { path: 'plans', component: PlansPaymentComponent },
            //{ path: 'users', component: ManageUsersComponent },
            { path: 'users', component: ManagmentComponent },
            { path: 'contacts', component: ManageContactsComponent },
            
        ]
    },
    {
        canActivate: [CanActivateGuard],
        component: MainComponent,
        path: "documents",
        children: [
            { path: '', redirectTo: 'all', pathMatch:"prefix" },
            { path: 'all', component: DocumentsComponent },
            { path: 'pending', component: DocumentsComponent },
            { path: 'signed', component: DocumentsComponent },
            { path: 'declined', component: DocumentsComponent },
            { path: 'signing', component: DocumentsLinksComponent },
            { path: 'canceled', component: DocumentsComponent },
            { path: 'distribution', component: DistributionDocumentsComponent },
        ]
    },
    {
        canActivate: [CanActivateGuard],
        component: SignersComponent,
        path: "selectsigners",        
    },
    {
        canActivate: [CanActivateGuard],
        component: SignersReviewComponent,
        path: "selectsigners/review",
    },
   
    {
        canActivate: [CanActivateGuard],
        component: SelfSignUploadComponent,
        path: "selfsign",
    },
    {
        canActivate: [CanActivateGuard],
        component: SelfSignPlaceFieldsComponent,
        path: "selfsignfields/:colid/:docid",
    },
    {
        canActivate: [CanActivateGuard],
        component: GroupSignAssignFieldsComponent,
        path: "groupsign",
    },
  
    {
        canActivate: [CanActivateGuard],
        component: DocViewComponent,
        path: "docview/:colid/:docid",
    },
    
    {
        canActivate: [CanActivateGuard],
        component: TemplateEditComponent,
        path: "template/edit/:id",
    },
    {
        canActivate: [CanActivateGuard],
        component: TinySignPlaceFieldsComponent,
        path: "tinysignfields/:templateId",
    },
    {
        canActivate: [CanActivateGuard],
        //canLoad: [ CanActivateGuard ],
        component: TinySendDocumentComponent,
        path: "tiny-send-document",
    },
     {
        canActivate: [CanActivateGuard],
        
        component: CertificatesignComponent,
        path: "certsign",
    },
    // {
    //     canActivate: [CanActivateGuard],
        
    //     component: GovSignComponent,
    //     path: "govsign/:colid/:docid",
    // },

    { path: "**",
    pathMatch: "full",
    redirectTo: "/login" }
];

@NgModule({
    declarations: [],
    exports: [RouterModule],
    imports: [RouterModule.forChild(routes)],
    providers: [CanActivateGuard],
})
export class DashboardRoutingModule { }

