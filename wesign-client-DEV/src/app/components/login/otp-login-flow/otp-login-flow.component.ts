import { AfterViewInit, Component, Input, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { LoginResult } from '@models/Users/login-result.model';
import { Signer1Credential } from '@models/self-sign-api/signer-authentication.model';
import { Store } from '@ngrx/store';
import { UserApiService } from '@services/user-api.service';
import { IAppState } from '@state/app-state.interface';
import * as actions from "@state/actions/app.actions";
import { TranslateService } from '@ngx-translate/core';
import { Errors } from '@models/error/errors.model';

@Component({
  selector: 'sgn-otp-login-flow',
  templateUrl: './otp-login-flow.component.html',
  styles: [
  ]
})
export class OtpLoginFlowComponent implements OnInit, AfterViewInit, OnDestroy {

  public hasOtpTimeLeft: boolean = false
  public otpCode: string = "";
  public sentSignerMeans: string = "";
  public otpSuccesSentToSignerMeans: string = "";
  public otpLeftTime: string;
  public timerFunctionId;
  public otpExpiredTime: Date;
  public isSubmitBusy: boolean = false;
  public showErrorCodeMessage: boolean = false;
  public errorCodeMessage: string = "";
  @Input() public token: string;
  @Input() public otpFlow: boolean;

  public showErrorMessage: boolean = false;
  public errorMessage: string = "";

  constructor(private userApiService: UserApiService,
    private store: Store<IAppState>,
    private router: Router,
    private translate: TranslateService) { }

  ngOnDestroy(): void {
    if (this.timerFunctionId) {
      clearInterval(this.timerFunctionId);
      this.hasOtpTimeLeft = false
    }
  }

  ngAfterViewInit(): void {
    this.otpExpiredTime = new Date();
    this.otpExpiredTime.setMinutes(this.otpExpiredTime.getMinutes() + 5);
    this.hasOtpTimeLeft = true;
  }

  ngOnInit(): void {
    this.timerFunctionId = setInterval(() => {
      this.timerFunction(this.hasOtpTimeLeft)
    }, 1000)
  }

  public timerFunction(hasOtpTimeLeft: boolean): void {
    if (hasOtpTimeLeft) {
      var timeDistance = this.otpExpiredTime.getTime() - new Date().getTime();

      var minutes = Math.floor((timeDistance % (1000 * 60 * 60)) / (1000 * 60));
      var seconds = Math.floor((timeDistance % (1000 * 60)) / 1000);

      if (timeDistance < 0) {
        this.hasOtpTimeLeft = false;
      }

      if (seconds < 10) {
        this.otpLeftTime = `0${minutes}:0${seconds}`
      } else {
        this.otpLeftTime = `0${minutes}:${seconds}`
      }
    }
  }

  private setOtpExpiredTime() {
    this.otpExpiredTime = new Date();
    this.otpExpiredTime.setMinutes(this.otpExpiredTime.getMinutes() + 5);
    this.hasOtpTimeLeft = true
  }

  public isValidCode() {
    if (this.otpCode == undefined || this.otpCode.trim() == "" || this.otpCode.trim().length != 6 || !this.otpCode) {
      this.showErrorCodeMessage = true;
      this.errorCodeMessage = this.translate.instant('OTP.INVALID_CODE');
      return;
    }

    this.otpCode = this.otpCode.trim();
    this.showErrorCodeMessage = false;
    this.showErrorMessage = false;

    if (this.hasOtpTimeLeft) {
      this.isSubmitBusy = true;
      this.userApiService.validateOtpflow(this.token, this.otpCode).subscribe(
        (data: LoginResult) => {
          this.isSubmitBusy = false;
          this.userApiService.accessToken = data.token;
          this.userApiService.refreshAccessToken = data.refreshToken;
          this.userApiService.authToken = data.authToken;
          this.store.dispatch(new actions.LoginAction({ Token: data.token, RefreshToken: data.refreshToken, AuthToken: data.authToken }));
          if (this.userApiService.authToken != "" && this.userApiService.authToken != null) {
            let auth = new Signer1Credential();
            auth.signerToken = this.userApiService.authToken;
            this.store.dispatch(new actions.SetSignerAuthAction({ signerAuth: auth }));
          }
          this.router.navigate(["dashboard"]);
        },
        (err) => {

          this.isSubmitBusy = false;

          if (err.status == 0) {
            this.errorMessage = this.translate.instant('SERVER_ERROR.429');
          } else {
            let result = new Errors(err.error);
            this.errorMessage = this.translate.instant('SERVER_ERROR.' + result.errorCode);
          }
          this.showErrorMessage = true;
          this.showErrorCodeMessage = false;
        });
    }
  }

  public generateOtpCode() {
    this.showErrorMessage = false;
    this.showErrorCodeMessage = false;
    this.isSubmitBusy = true;
    this.userApiService.resendOtp(this.token).subscribe(
      (data: LoginResult) => {
        this.isSubmitBusy = false;
        this.token = data.refreshToken;
        this.setOtpExpiredTime();
      },
      (err) => {

        this.isSubmitBusy = false;
        if (err.status == 0) {
          this.errorMessage = this.translate.instant('SERVER_ERROR.429');
        } else {
          let result = new Errors(err.error);
          this.errorMessage = this.translate.instant('SERVER_ERROR.' + result.errorCode);
        }
        this.showErrorMessage = true;

        this.showErrorCodeMessage = false;

      });
  }
}