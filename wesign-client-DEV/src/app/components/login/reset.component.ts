import { Component, OnInit, ViewEncapsulation } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { Store } from "@ngrx/store";
import { UserApiService } from "@services/user-api.service";
import * as actions from "@state/actions/app.actions";
import { IAppState } from "@state/app-state.interface";
import { FormControl, Validators, FormGroup, AbstractControl } from '@angular/forms';
import { Errors } from '@models/error/errors.model';
import { environment } from "../../../environments/environment"
import { SharedService } from '@services/shared.service';

@Component({
    selector: "sgn-reset",
    templateUrl: "reset.component.html",
})

export class ResetComponent implements OnInit {

    public resetToken: string = "";
    public newPassword: string = "";
    public confirmPassword: string = "";
    public errorMessage: string;
    public passMinLength: string;
    public isBusy: boolean = false;
    public resetForm: FormGroup;
    public eyeIcon: string = "eye";
    public eyeIcon2: string = "eye";
    public tiny = environment.tiny;

    constructor(private activatedRoute: ActivatedRoute, private router: Router,
        private userApiService: UserApiService, private store: Store<IAppState>, private readonly sharedService: SharedService) {
        this.activatedRoute.params.subscribe((params) => {
            this.resetToken = params.guid;
        });
    }

    public ngOnInit() {
        this.resetForm = new FormGroup({
            newPassword: new FormControl('', [Validators.required, Validators.pattern("^(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).*$")]),
            confirmPassword: new FormControl('', [Validators.required, Validators.pattern("^(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).*$")])
        }, {
            validators: this.MatchPassword
        });
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

    public reset() {
        if (!this.resetForm.valid || this.resetForm.get('newPassword').value != this.resetForm.get('confirmPassword').value) {
            this.errorMessage = 'ERROR.INPUT.E_PASS';
            return;
        }
        this.isBusy = true;

        this.userApiService.updatePassword(this.resetForm.get('newPassword').value, this.resetToken).subscribe((data) => {
            this.userApiService.accessToken = data.token;
            this.userApiService.refreshAccessToken = data.refreshToken;
            this.userApiService.authToken = data.authToken;
            this.store.dispatch(new actions.LoginAction({ Token: data.token, RefreshToken: data.refreshToken, AuthToken: data.authToken }));
            this.router.navigate(["dashboard"]);

            this.isBusy = false;
        }, (err) => {
            let result = new Errors(err.error);
            this.passMinLength = this.sharedService.getMinimumPasswordLengthFromError("NewPassword", result);
            if (this.passMinLength == null) {
                this.errorMessage = `SERVER_ERROR.${result.errorCode}`;
            }
            this.isBusy = false;
        });
    }

}
