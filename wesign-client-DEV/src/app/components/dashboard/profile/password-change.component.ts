import { Component, OnInit } from "@angular/core";
import { FormGroup, FormBuilder, FormControl, Validators, AbstractControl } from '@angular/forms';
import { Errors } from '@models/error/errors.model';
import { UserApiService } from '@services/user-api.service';
import { ChangePasswordRequest } from '@models/Users/change-password-request.model';
import { SharedService } from '@services/shared.service';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';


@Component({
    selector: "sgn-password-change",
    templateUrl: "password-change.component.html",
})

export class PasswordChangeComponent implements OnInit {

    public changePassForm: FormGroup;
    public isSubmited: boolean = false;
    public isBusy: boolean = false;
    public errorsArr: string[] = [];
    public passMinLength: string;
    public eyeIcon: string = "eye";
    public eyeIcon2: string = "eye";
    public eyeIcon3: string = "eye";


    constructor(private userApiService: UserApiService,
        private sharedService: SharedService,
        private translate: TranslateService,
        private router: Router) {
        this.changePassForm = new FormGroup({
            oldPassword: new FormControl('', [Validators.required, Validators.pattern("^(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).*$")]),
            newPassword: new FormControl('', [Validators.required, Validators.pattern("^(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).*$")]),
            confirmPassword: new FormControl('', [Validators.required, Validators.pattern("^(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).*$")]),
        }, {
            validators: this.MatchPassword
        });
    }

    public ngOnInit() {
        /* TODO */
    }

    public MatchPassword(AC: AbstractControl) {
        let password = AC.get('newPassword').value; // to get value in input tag
        let confirmPassword = AC.get('confirmPassword').value; // to get value in input tag
        if (password != confirmPassword) {
            AC.get('confirmPassword').setErrors({ MatchPassword: true })
        } else {
            return null
        }
    }

    public changePassword() {
        this.errorsArr = [];
        if (this.changePassForm.valid) {
            let cpr = new ChangePasswordRequest();
            cpr.OldPassword = this.changePassForm.get('oldPassword').value;
            cpr.NewPassword = this.changePassForm.get('newPassword').value;

            this.isSubmited = true;
            this.isBusy = true;

            this.userApiService.changePassword(cpr).subscribe(() => {
                this.sharedService.setSuccessAlert(this.translate.instant(`ERROR.OPERATION.4`));
                setTimeout(() => {
                    this.router.navigate(['/dashboard', 'profile']);
                }, 3000);
            }, (err) => {
                this.isBusy = false;
                let errors = new Errors(err.error);
                this.passMinLength = this.sharedService.getMinimumPasswordLengthFromError("NewPassword", errors);
                if (this.passMinLength == null) {
                    this.sharedService.setErrorAlert(errors);
                }
            });
        } else {
            if (this.changePassForm.get('oldPassword').invalid
                || this.changePassForm.get('confirmPassword').invalid
                || this.changePassForm.get('newPassword').invalid
                || this.changePassForm.get('confirmPassword').value != this.changePassForm.get('newPassword').value) {
                this.errorsArr.push("ERROR.INPUT.E_PASS");
            } else {
                this.errorsArr.push("ERROR.INPUT.0");
            }
            this.isBusy = false;
            //this.isSubmited = false;
        }
    }
}
