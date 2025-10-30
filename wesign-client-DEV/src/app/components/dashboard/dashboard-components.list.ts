
import { CalendarComponent } from "@components/shared/calendar.component";
import { PaginatorComponent } from "@components/shared/controls/paginator.component";
import { ZoomComponent } from "@components/shared/controls/zoom.component";
import { BaseFieldComponent } from "@components/shared/field-components/base-field.component";
import { CheckboxComponent } from "@components/shared/field-components/checkbox-field.component";
import { ChoiceComponent } from "@components/shared/field-components/choice-field.component";
import { EmailFieldComponent } from "@components/shared/field-components/email-field.component";
import { NumberFieldComponent } from "@components/shared/field-components/number-field.component";
import { PhoneFieldComponent } from "@components/shared/field-components/phone-field.component";
import { RadioButtonComponent } from "@components/shared/field-components/radio-field.component";
import { SignatureFieldComponent } from "@components/shared/field-components/signature-field.component";
import { TextFieldComponent } from "@components/shared/field-components/text-field.component";
import { MultilineTextFieldComponent } from "@components/shared/field-components/multiline-text-field.component";
import { EditPageComponent } from "@components/shared/pages/edit-page.component";
import { ViewPageComponent } from "@components/shared/pages/view-page.component";
import { SignPadComponent } from "@components/shared/sign-pad.component";
import { SidebarFieldButtonComponent } from "../shared/field-components/sidebar-field-button.component";
import { DocViewComponent } from "./document/doc-view.component";
import { ContactsComponent } from "./main/contacts/contacts.component";
import { EditContactComponent } from "./main/contacts/edit-contact.component";
import { DocumentsComponent } from "./main/documents/documents.component";
import { MainComponent } from "./main/main.component";
import { OnboardComponent } from "./main/onboard/onboard.component";
import { TemplatesComponent } from "./main/templates/templates.component";

import { PasswordChangeComponent } from "./profile/password-change.component";
import { ProfileComponent } from "./profile/profile.component";
import { SelfSignPlaceFieldsComponent } from "./selfsign/self-sign-place-fields.component";
import { SelfSignUploadComponent } from "./selfsign/self-sign-upload.component";
import { TemplateEditComponent } from "./template/template-edit.component";
import { CustomDropDownComponent } from "../shared/controls/custom-dropdown.component";
import { CustomDropDownForDatesComponent } from "../shared/custom-drop-down/custom-drop-down-for-dates.component";

import { DateFieldComponent } from '@components/shared/field-components/date-field.component';
import { TimeFieldComponent } from '@components/shared/field-components/time-field.component';
import { CustomFieldComponent } from '@components/shared/field-components/custom-field.component';
import { GroupSignAssignFieldsComponent } from '@components/dashboard/groupsign/group-sign-assign-fields'

import { TinySendDocumentComponent } from './tiny-send-document/tiny-send-document.component';
import { TinySignPlaceFieldsComponent } from './tiny-send-document/tiny-sign-place-fields/tiny-sign-place-fields.component';
import { PlansPaymentComponent } from './plans-payment/plans-payment.component';
import { ManagmentComponent } from './main/managment/managment.component';
import { AddUserComponent } from './main/managment/add-user/add-user.component';
import { ManageGourpComponent } from './main/managment/manage-gourp/manage-gourp.component';
import { HomeComponent } from './main/home.component';
import { ProfileWrapperComponent } from './profile/profile-wrapper.component';
import { SignersComponent } from './main/signers/signers.component';
import { AddFiledsSidebarComponent } from '@components/shared/controls/add-fileds-sidebar/add-fileds-sidebar.component';
import { AssignModalComponent } from './groupsign/assign-modal.component';
import { CertificatesignComponent } from './selfsign/certificate-sign/certificate-sign.component';
import { PopUpConfirmComponent } from '@components/shared/controls/pop-up-confirm/pop-up-confirm.component';
// import { GovSignComponent } from './selfsign/gov-sign/gov-sign.component';


const COMPONENTS: any[] = [
    
    MainComponent,
    HomeComponent,
    SignersComponent,
    OnboardComponent,
    DocumentsComponent,
    CalendarComponent,
    ProfileComponent,
    ProfileWrapperComponent,
    PasswordChangeComponent,
    SelfSignUploadComponent,
    SelfSignPlaceFieldsComponent,
    TinySignPlaceFieldsComponent,
    SidebarFieldButtonComponent,
    TextFieldComponent,
    MultilineTextFieldComponent,
    EditPageComponent,    
    SignatureFieldComponent,
    BaseFieldComponent,    
    SignPadComponent,
    // AlertComponent,
    // LoadingComponent,
    TemplatesComponent,
    EmailFieldComponent,
    DateFieldComponent,
    CustomFieldComponent,
    TimeFieldComponent,
    DocViewComponent,
    ViewPageComponent,
    TemplateEditComponent,
    PhoneFieldComponent,
    NumberFieldComponent,
    ZoomComponent,
    ContactsComponent,
    CheckboxComponent,
    PaginatorComponent,
    
    RadioButtonComponent,    
    EditContactComponent,
 
    ChoiceComponent,    
    
    CustomDropDownComponent,
    CustomDropDownForDatesComponent,
    GroupSignAssignFieldsComponent,
        
    AssignModalComponent,
    
    TinySendDocumentComponent,
    PlansPaymentComponent,
    AddFiledsSidebarComponent,
    ManagmentComponent,
    AddUserComponent,
    ManageGourpComponent,
    CertificatesignComponent,
    // GovSignComponent
];

export { COMPONENTS };
