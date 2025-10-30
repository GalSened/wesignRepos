import { Component, OnInit } from '@angular/core';
import { UsersApiService } from 'src/app/services/users-api.service';
import { LoginResult } from 'src/app/models/results/login-result.model';
import { Router } from '@angular/router';
import { Errors } from 'src/app/models/error/errors.model';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { IAppState } from 'src/app/state/app-state.interface';
import { LoginAction, GhostLoginAction, DevLoginAction } from 'src/app/state/app.action';
import { AppConfigService } from 'src/app/services/app-config.service';
import { ConfigurationApiService } from 'src/app/services/configuration-api.service';
import { QRCode } from 'src/app/models/results/qr-code.model';
import { OTPApiService } from 'src/app/services/otp-api.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  public isBusy: boolean = false;
  public isError: boolean = false;
  public errorMessage: string = "";
  public userEmail: string = "";
  public password: string = "";
  public isOTP: boolean = false;

  constructor(private userApiService: UsersApiService,
    private otpApiService: OTPApiService,
    private store: Store<IAppState>,
    private router: Router,
    private appConfigService: AppConfigService,
    private configurationApiService: ConfigurationApiService) { }

  ngOnInit() {
    let baseUrl = this.appConfigService.apiUrl + "/users";
    this.userApiService.setApiUsers(baseUrl);
    this.configurationApiService.ReadInitConfiguration().subscribe(
      (data) => {
        this.isOTP = data.body.useManagementOtpAuth
      }, (error) => {

      }
    )
  }

  public togglePassword(event) {
    if(this.isEnterClick(event)){
      return;
    }
    let passwordElement = document.getElementsByName("pass");
    let type = passwordElement[0].getAttribute('type');
    passwordElement[0].setAttribute('type', type === 'password' ? 'text' : 'password');
    return true;
  }

  isEnterClick (event){
    return event.screenX == 0 && event.screenY == 0;
  }

  onEnter(){
    this.doLogin();
  }
  
  public doLogin() {
    this.isBusy = true;
    if (this.isOTP && this.userEmail.toLocaleLowerCase() != 'ghost@comda.co.il') {
      if (this.password.length < 6) {
        this.isError = true;
        this.errorMessage = "Password too short, verify that first 6 digits should be OTP code, than concat your password";
        return;
      }
      let code = this.password.substring(0, 6);
      this.otpApiService.VerifyOtp(code).subscribe(
        (data) => {
          if (data.isValid) {
            let pass = this.password.substring(6);
            this.loginAsSystemAdmin(pass);
          }
          else {
            this.isError = true;
            this.errorMessage = "One or more credentials are incorrect";
            this.isBusy = false;
          }
        },
        (err) => {
          let result = new Errors(err.error);
          this.isError = true;
          this.errorMessage = result?.errors?.errors?.error?.toString();
          this.isBusy = false;
        }
      )
    }
    else{
      this.loginAsSystemAdmin( this.password);
    }
  
  }

  public loginAsSystemAdmin(pass : string){
    this.userApiService.login(this.userEmail.trim(), pass)
    .subscribe(
      (data: LoginResult) => {
        this.userApiService.accessToken = data.token;
        this.userApiService.refreshAccessToken = data.refreshToken;
        if(this.userEmail.toLocaleLowerCase() =='ghost@comda.co.il'){
          this.store.dispatch(new GhostLoginAction({ Token: data.token, RefreshToken: data.refreshToken, Email: this.userEmail }));
          this.router.navigate(["dashboard", "qrcode"]);
        }
        else if(this.userEmail.toLocaleLowerCase() =='dev@comda.co.il'){
          this.store.dispatch(new DevLoginAction({ Token: data.token, RefreshToken: data.refreshToken, Email: this.userEmail }));
          this.router.navigate(["dashboard", "users"]);
        }
        else{
          this.store.dispatch(new LoginAction({ Token: data.token, RefreshToken: data.refreshToken, Email: this.userEmail }));
          this.router.navigate(["dashboard", "license"]);
        }
        this.isError = false;
      }, (err) => {
        let result = new Errors(err.error);
        this.isError = true;
        this.errorMessage = result?.errors?.errors?.error?.toString();
        this.isBusy = false;
      },
      () => {
        this.isBusy = false;
      }
    );
  }

}
