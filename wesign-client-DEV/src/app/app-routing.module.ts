import { ErrorPageComponent } from './components/shared/error-page/error-page.component';
import { PrivacyPolicyComponent } from './components/login/privacy-policy/privacy-policy.component';
import { TermsOfUseComponent } from './components/login/terms-of-use/terms-of-use.component';
import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { DashboardComponent } from "@components/dashboard/dashboard.component";
import { ActivateComponent } from "@components/login/activate.component";
import { ForgetComponent } from "@components/login/forget.component";
import { LoginComponent } from "@components/login/login.component";
import { RegisterConfirmComponent } from "@components/login/register-confirm.component";
import { RegisterComponent } from "@components/login/register.component";
import { ResetComponent } from "@components/login/reset.component";
import { CanActivateGuard } from "./guards/can-activate.guard";
import { ThankyouComponent } from '@components/login/thankyou.component';
import { ExternalloginComponent } from '@components/login/externallogin/externallogin.component';
import { ExpiredPasswordComponent } from '@components/login/expired-password/expired-password.component';
import { EidasSigningFlowAfterFirstAuthComponent } from '@components/dashboard/selfsign/eidas-signing-flow-after-first-auth/eidas-signing-flow-after-first-auth.component';

const routes: Routes = [
    {
        path: "",
        pathMatch: "full",
        redirectTo: "/login",
    },
    {
        canActivate: [CanActivateGuard],
        component: LoginComponent,
        path: "login",
    },
    {
        canActivate: [CanActivateGuard],
        component: ForgetComponent,
        path: "login/forget",
    },
    {
        canActivate: [CanActivateGuard],
        component: RegisterComponent,
        path: "login/register",
    },
    {
        component: TermsOfUseComponent,
        path: "terms",
    },
    {
        component: PrivacyPolicyComponent,
        path: "privacy",
    },
    
    {
        canActivate: [CanActivateGuard],
        component: EidasSigningFlowAfterFirstAuthComponent,
        path: "oauth",
        
    },
    {
        component: ThankyouComponent,
        path: "thankyou",
    },
    {
        component: ErrorPageComponent,
        path: "notfound",
    },
    
    {
        canActivate: [CanActivateGuard],
        component: ActivateComponent,
        path: "login/activate/:guid",
    },
    {
        canActivate: [CanActivateGuard],
        component: RegisterConfirmComponent,
        path: "login/registerconfirm/:email",
    },
    {
        canActivate: [CanActivateGuard],
        component: ResetComponent,
        path: "login/resetpass/:guid",
    },
    
    {
        canActivate: [CanActivateGuard],
        component: ExternalloginComponent,
        path: "externallogin/:guid",
    },
    {
        component: DashboardComponent,
        loadChildren: () => import('@components/dashboard/dashboard.module').then(m => m.DashboardModule),
        path: "dashboard",
    },
    { path: "**", redirectTo: "/login", pathMatch: "full" },
];

@NgModule({
    exports: [RouterModule],
    imports: [RouterModule.forRoot(routes, { useHash: false, enableTracing: false, onSameUrlNavigation: "reload" })],
    providers: [CanActivateGuard],
})
export class AppRoutingModule {

}
