import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from "@angular/core";
import { Contact, SendingMethod } from "@models/contacts/contact.model";
import { NgForm } from '@angular/forms';
import { SharedService } from '@services/shared.service';
import { TranslateService } from '@ngx-translate/core';
import { Observable } from 'rxjs';
import { AppState } from '@state/app-state.interface';
import { UserProgram } from '@models/program/user-program.model';
import { StateProcessService } from '@services/state-process.service';
import { CountriesPhoneData } from '@services/countries-phone-data.service';
import { ContactApiService } from '@services/contact-api.service';
import { Errors } from '@models/error/errors.model';

@Component({
    selector: "sgn-edit-contact",
    templateUrl: "edit-contact.component.html",
})

export class EditContactComponent implements OnInit { // TODO - change name to: NewContactComponent

    @ViewChild('newContactForm') newContactForm: NgForm;
    @ViewChild('signingMethod') signingMethod: any;
    @Input() public isInEditMode: boolean;
    @Input() public contact: Contact;
    @Output() public accept = new EventEmitter<void>();
    @Output() public cancel = new EventEmitter<void>();
    selectedOption: string;
    header: string;
    hasError: boolean;
    phoneExt = "+972";
    userProgram = new UserProgram();
    errorMsg: string = "";
    isEmailOption: boolean;
    isReadyToSubmit: boolean;
    telOptions = { initialCountry: 'il' };
    methods = { 1: "SMS", 2: "EMAIL" }
    submitted = false;
    public state$: Observable<any>;

    options = [
        { name: "option1", value: 1 },
        { name: "option2", value: 2 }
    ]

    public get SendingMethodOptions() {
        return SharedService.EnumToArrayHelper<SendingMethod>(SendingMethod)
    };

    constructor(private translate: TranslateService, private contactsApi: ContactApiService,
        private sharedService: SharedService, private countriesPhoneData: CountriesPhoneData, private stateService: StateProcessService) { }

    ngOnInit(): void {
        if (this.contact.defaultSendingMethod == SendingMethod.EMAIL) {
            this.selectedOption = "EMAIL";
        }
        else {
            this.selectedOption = "SMS";
        }
        this.isEmailOption = this.selectedOption == this.methods[2] ? true : false;
        let iso2 = this.countriesPhoneData.getIso2CodeByDialCode(this.contact.phoneExtension.replace("+", ""), this.contact.phone);
        this.telOptions = iso2 != undefined ? { initialCountry: <string>iso2 } : { initialCountry: 'il' };
        this.phoneExt = this.contact.phoneExtension == "" ? "+972" : this.contact.phoneExtension;
        this.stateService.getState().subscribe((state: AppState) => {
            this.userProgram = state.program;
        })
        this.isFormValid();
    }

    dropDownUpdate(value: any) {
        if (value) {
            this.contact.defaultSendingMethod = value;
        }
    }
    onSendingMethodChange(sendingMethod: string) {
        this.isEmailOption = this.selectedOption == this.methods[2] ? true : false;
        this.contact.defaultSendingMethod = this.isEmailOption ? SendingMethod.EMAIL : SendingMethod.SMS;
        this.isFormValid();
    }

    isEmailValid(): boolean {
        var email = this.contact.email;
        var validRegex = /^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$/;
        if (email.match(validRegex)) {
            return true;
        } else {
            return false;
        }
    }

    isEmailEmpty(): boolean {
        return this.contact.email.length === 0;
    }

    isPhoneNumberEmpty(): boolean {
        return this.contact.phone.length === 0;
    }

    isFormValid(): boolean {
        let nameElement = document.getElementById("fullNameFieldInput");
        let emailElement = document.getElementById("emailFieldInput");
        let phoneElement = document.getElementById("phoneFieldInput");
        let button = document.getElementById("addContactButton");

        if (nameElement == null || emailElement == null || phoneElement == null) {
            button.setAttribute("disabled", "true");
            return false;
        }

        let isNameValid = nameElement.classList.contains("is-confirm");
        let isEmailOrPhoneFilledAndValid = emailElement.classList.contains("is-confirm") ||
            phoneElement.classList.contains("is-confirm")
        let isBothEmailAndPhoneNotInvalid = emailElement.classList.contains("is-error") == false &&
            phoneElement.classList.contains("is-error") == false;

        let isPhoneOptionSelectedAndValid = phoneElement.classList.contains("is-confirm") && !this.isEmailOption;
        let isEmailOptionSelectedAndValid = emailElement.classList.contains("is-confirm") && this.isEmailOption;
        let isSelectedOptionValid = isPhoneOptionSelectedAndValid || isEmailOptionSelectedAndValid;

        if (isNameValid && isEmailOrPhoneFilledAndValid && isBothEmailAndPhoneNotInvalid && isSelectedOptionValid) {
            button.removeAttribute("disabled");
            return true;
        }
        button.setAttribute("disabled", "true");
        return false;
    }

    public isFormValidOld(): boolean {
        if (this.newContactForm.controls.contactName.invalid) {
            return false
        }
        let selectedMethod = this.selectedOption;
        if (selectedMethod) {
            selectedMethod = (selectedMethod as string).trim();
            if (!this.newContactForm.controls.contactName || !this.newContactForm.controls.contactName.value) {
                return false
            }
            if (selectedMethod == SendingMethod[SendingMethod.SMS]) {
                this.contact.defaultSendingMethod = SendingMethod.SMS;
                if (!this.newContactForm.controls.contactPhone.value || this.newContactForm.controls.contactPhone.invalid) {
                    return false
                }
                if (!this.userProgram.isSmsProviderSupportGloballySend && this.phoneExt != "+972") {
                    return false
                }
            } else if (selectedMethod == SendingMethod[SendingMethod.EMAIL]) {
                this.contact.defaultSendingMethod = SendingMethod.EMAIL;
                if (!this.newContactForm.controls.contactEmail.value || this.newContactForm.controls.contactEmail.invalid) {
                    return false
                }
            }
        }
        return true
    }

    addContact() {
        this.contactsApi.addContact(this.contact).subscribe((res) => {

            this.sharedService.setSuccessAlert("CONTACTS.SAVED_SUCCESSFULY");
            this.accept.emit();
            this.submitted = false;
        }, (error) => {
            let result = new Errors(error.error);
            this.translate.get(`SERVER_ERROR.${result.errorCode}`).subscribe(
                (msg) => {
                    this.errorMsg = msg;
                    this.submitted = false;
                });

        });
    }

    saveContact() {

        this.contactsApi.saveContact(this.contact).subscribe((res) => {
            this.sharedService.setSuccessAlert("CONTACTS.SAVED_SUCCESSFULY");
            this.accept.emit();
            this.submitted = false;
        }, (error) => {
            let result = new Errors(error.error);
            this.translate.get(`SERVER_ERROR.${result.errorCode}`).subscribe(
                (msg) => {
                    this.errorMsg = msg;
                    this.submitted = false;
                });
        });
    }

    save() {
        if (this.submitted) {
            return;
        }
        
        this.errorMsg = "";

        if (this.contact.phone) {
            this.contact.phoneExtension = this.phoneExt.includes("+") ? this.phoneExt : `+${this.phoneExt}`;
            this.contact.phone = this.contact.phone.replace(/-/g, "");
        }
        if (this.isFormValid()) {
            this.submitted = true;
            if (this.isInEditMode) {
                this.saveContact();
            }
            else {
                this.addContact();
            }
        }
    }

    onCountryChange(obj) {
        this.phoneExt = obj.dialCode;
    }
}