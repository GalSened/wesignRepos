import { AfterViewInit, Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Errors } from '@models/error/errors.model';
import { TranslateService } from '@ngx-translate/core';
import { SharedService } from '@services/shared.service';
import { UserApiService } from '@services/user-api.service';

@Component({
  selector: 'sgn-expired-password',
  templateUrl: './expired-password.component.html',
  styles: [
  ]
})
export class ExpiredPasswordComponent implements OnInit, AfterViewInit, OnDestroy {
  public oldPassword: string;
  public newPassword: string;
  public confirmPassword: string;
  public errorMessage: string;
  public passMinLength: string;
  public isBusy: boolean;
  public expiredPasswordForm: FormGroup;
  public oldPasswordEyeIcon: string = "eye";
  public newPasswordEyeIcon: string = "eye";
  public confirmPasswordEyeIcon: string = "eye";
  public tokenExpirationTime: Date;
  public timerFunctionId;
  @Input() token: string;
  @Input() expiredPassFlow: boolean;
  @Output() passwordChanged = new EventEmitter();
  @Output() expiredTimeToChange = new EventEmitter();

  constructor(private userApiService: UserApiService,
    private translate: TranslateService,
    private sharedService: SharedService,
    private router: Router) { }

  public ngOnDestroy(): void {
    if (this.timerFunctionId) {
      clearInterval(this.timerFunctionId);
    }
  }

  public ngAfterViewInit(): void {
    this.tokenExpirationTime = new Date();
    this.tokenExpirationTime.setMinutes(this.tokenExpirationTime.getMinutes() + 5);
  }

  public ngOnInit(): void {
    this.expiredPasswordForm = new FormGroup({
      oldPassword: new FormControl('', [Validators.required, Validators.pattern("^(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).*$")]),
      newPassword: new FormControl('', [Validators.required, Validators.pattern("^(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).*$")]),
      confirmPassword: new FormControl('', [Validators.required, Validators.pattern("^(?=.*?[0-9])(?=.*?[#?!@$%^&*-]).*$")]),
    }, {
      validators: [this.matchPassword, this.isNewPassword]
    });
    this.timerFunctionId = setInterval(() => {
      this.checkTokenActivation();
    }, 1000);
  }

  public checkTokenActivation() {
    var timeDistance = this.tokenExpirationTime.getTime() - new Date().getTime();
    if (timeDistance < 0) {
      this.expiredTimeToChange.emit();
    }
  }

  private matchPassword(AC: AbstractControl) {
    let password = AC.get('newPassword').value;
    let confirmPassword = AC.get('confirmPassword').value;
    if (password !== confirmPassword) {
      AC.get('confirmPassword').setErrors({ matchPassword: true });
    } else {
      AC.get('confirmPassword').setErrors(null);
      return null;
    }
  }

  private isNewPassword(AC: AbstractControl) {
    let oldPassword = AC.get('oldPassword').value;
    let newPassword = AC.get('newPassword').value;
    if (oldPassword === newPassword) {
      AC.get('oldPassword').setErrors({ isNewPassword: true });
    } else {
      AC.get('oldPassword').setErrors(null);
      return null;
    }
  }

  public submit() {
    if (!this.expiredPasswordForm.get('oldPassword').valid) {
      this.errorMessage = 'ERROR.INPUT.E_OLD_PASS';
      return;
    }
    else if (!this.expiredPasswordForm.get('confirmPassword').valid) {
      this.errorMessage = 'ERROR.INPUT.E_PASS';
      return;
    }

    this.isBusy = true;
    let oldPassword = this.expiredPasswordForm.get('oldPassword').value;
    let newPassword = this.expiredPasswordForm.get('newPassword').value;
    this.userApiService.validateExpiredPasswordFlow(this.token, oldPassword, newPassword).subscribe(_ => {
      this.isBusy = false;
      this.passwordChanged.emit();
    }, err => {
      let result = new Errors(err.error);
      this.passMinLength = this.sharedService.getMinimumPasswordLengthFromError("NewPassword", result);
      if (this.passMinLength == null) {
        this.errorMessage = this.translate.instant(`SERVER_ERROR.${result.errorCode}`);
      }
      this.isBusy = false;
    },
  () => {
    this.isBusy = false;
  });
  }
}
