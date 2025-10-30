import { Component, OnInit, ViewEncapsulation } from "@angular/core";
import { UserApiService } from "@services/user-api.service";
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Errors } from '@models/error/errors.model';
import { environment } from "../../../environments/environment"

@Component({
    selector: "sgn-login-forget",
    templateUrl: "forget.component.html",
})

export class ForgetComponent implements OnInit {

    public email: string;
    public isError: boolean;
    public errorMessage: string;
    public forgetForm: FormGroup;
    public isSent: boolean = false;
    public isBusy: boolean = false;
    public tiny = environment.tiny;

    constructor(private userApiService: UserApiService, private router: Router) { }

    public ngOnInit() {
        this.forgetForm = new FormGroup({
            email: new FormControl('', [Validators.required, Validators.email])
        });
    }

    public send() {
        this.isBusy = true;

        this.userApiService.resetPassword(this.forgetForm.get('email').value).subscribe((res) => {
            this.isSent = true;
            this.isBusy = false;
            setTimeout(() => {
                this.router.navigate(["/login"]);
            }, 2000);
        }, (err) => {
            this.isBusy = false;
            let result = new Errors(err.error);
            this.errorMessage = `SERVER_ERROR.${result.errorCode}`;
        });
    }

}
