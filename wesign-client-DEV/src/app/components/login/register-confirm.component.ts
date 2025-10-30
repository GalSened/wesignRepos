import { Component, OnInit, ViewEncapsulation } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { UserApiService } from "@services/user-api.service";
import { Errors } from '@models/error/errors.model';
import { environment } from "../../../environments/environment"

@Component({
    selector: "sgn-register-confirm",
    templateUrl: "register-confirm.component.html",
})

export class RegisterConfirmComponent {

    public email: string;
    public isSent: boolean = false;
    public tiny = environment.tiny;
    public errorMessage: string;
    public isBusy: boolean = false;

    constructor(private userApiService: UserApiService, private activatedRoute: ActivatedRoute) {
        this.activatedRoute.params.subscribe((data) => {
            this.email = data.email;
        });
    }

    public send() {
        this.isSent = false;
        this.isBusy = true;
        this.userApiService.resendActivationLink(this.email).subscribe((data) => {
            this.isSent = true;
            this.isBusy = false;
        }, (error) => {
            this.isSent = false;
            this.isBusy = false;
            let result = new Errors(error.error);
            this.errorMessage = `SERVER_ERROR.${result.errorCode}`;
        });
    }
}
