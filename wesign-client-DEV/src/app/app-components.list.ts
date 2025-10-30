import { DashboardComponent } from "@components/dashboard/dashboard.component";
import { AccountNavComponent } from "@components/header/account-nav/account-nav.component";
import { ActionHeaderComponent } from "@components/header/action-header/action-header.component";
import { HeaderComponent } from "@components/header/header.component";
import { MultisignHeaderComponent } from "@components/header/multisign-header/multisign-header.component";
import { OnlineHeaderComponent } from "@components/header/online-header/online-header.component";
import { SelfSignHeaderComponent } from "@components/header/selfsign-header/selfsign-header.component";
import { TemplateHeaderComponent } from "@components/header/template-header/template-header.component";
import { ActivateComponent } from "@components/login/activate.component";
import { ForgetComponent } from "@components/login/forget.component";
import { LoginComponent } from "@components/login/login.component";
import { RegisterConfirmComponent } from "@components/login/register-confirm.component";
import { RegisterComponent } from "@components/login/register.component";
import { ResetComponent } from "@components/login/reset.component";
import { LangSelectorComponent } from "@components/shared/lang-selector.component";
import { TermsOfUseComponent } from '@components/login/terms-of-use/terms-of-use.component';
import { PrivacyPolicyComponent } from '@components/login/privacy-policy/privacy-policy.component';
import { ErrorPageComponent } from '@components/shared/error-page/error-page.component';
import { ModalComponent } from "@components/shared/controls/modal.component";
import { PlanComponent } from '@components/dashboard/plans-payment/plan/plan.component';
import { ThankyouComponent } from '@components/login/thankyou.component';
import { PopUpConfirmComponent } from '@components/shared/controls/pop-up-confirm/pop-up-confirm.component';
import { EidasSigningFlowAfterFirstAuthComponent } from '@components/dashboard/selfsign/eidas-signing-flow-after-first-auth/eidas-signing-flow-after-first-auth.component';

const COMPONENTS: any[] = [
    LoginComponent,
    ForgetComponent,
    ResetComponent,
    RegisterComponent,
    //LangSelectorComponent,
    RegisterConfirmComponent,
    ActivateComponent,
    DashboardComponent,
    HeaderComponent,
    ActionHeaderComponent,
    SelfSignHeaderComponent,
    AccountNavComponent,
    TemplateHeaderComponent,
    OnlineHeaderComponent,
    MultisignHeaderComponent,
    PrivacyPolicyComponent,
    TermsOfUseComponent,
    ErrorPageComponent,
    ModalComponent,
    //PlanComponent
    EidasSigningFlowAfterFirstAuthComponent,
    ThankyouComponent
];

export { COMPONENTS };
