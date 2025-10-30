import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { DownloadDocumentComponent } from './components/download-document/download-document.component';
import { ErrorComponent } from './components/error/error.component';
import { MainSignerComponent } from './components/main-signer/main-signer.component';
import { SenderViewComponent } from './components/sender-view/sender-view.component';
import { SuccessPageComponent } from './components/success-page/success-page.component';
import { SingleLinkComponent } from './components/single-link/single-link.component';
import { PrivacyPolicyComponent } from './components/privacy-policy/privacy-policy.component';
import { TermsOfUseComponent } from './components/terms-of-use/terms-of-use.component';
import { AgentViewComponent } from './components/agent-view/agent-view.component';
import { DeclinePageComponent } from './components/decline-page/decline-page.component'
import { IdentitySucsessComponent } from './components/oauth-comsign-idp/identity-sucsess/identity-sucsess.component';
import { IedasSigningFlowAfterFirstAuthComponent } from './components/iedas-signing-flow-after-first-auth/iedas-signing-flow-after-first-auth.component';
import { OtpExceededPageComponent } from './components/otp-exceeded-page/otp-exceeded-page.component';

const routes: Routes = [
  { path: 'signature/:id', component: MainSignerComponent },
  { path: 'download/:id', component: DownloadDocumentComponent },
  { path: 'singlelink/:id', component: SingleLinkComponent },
  { path: 'sender/:id', component: SenderViewComponent },
  { path: 'agent/:id/company/:companyId', component: AgentViewComponent },
  { path: 'success', component: SuccessPageComponent },
  { path: 'privacy', component: PrivacyPolicyComponent },
  { path: 'terms', component: TermsOfUseComponent },
  { path: 'decline', component: DeclinePageComponent },
  { path: 'otp-exceeded', component: OtpExceededPageComponent},
  { path: 'identityflowdone/:signerToken', component: IdentitySucsessComponent },
  { path: 'oauth', component: IedasSigningFlowAfterFirstAuthComponent },
  { path: '**', component: ErrorComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes, {})],
  exports: [RouterModule]
})
export class AppRoutingModule { }