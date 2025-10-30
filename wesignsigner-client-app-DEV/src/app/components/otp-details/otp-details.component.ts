import { Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AppState } from 'src/app/models/state/app-state.model';
import { StateService } from 'src/app/services/state.service';
import { OtpService } from 'src/app/services/otp.service';
import { GenerateCodeRequest } from 'src/app/models/requests/generate-code-request.model';
import { Errors } from 'src/app/models/error/errors.model';
import { TranslateService } from '@ngx-translate/core';
import { OtpMode } from 'src/app/models/responses/document-collection-count.model';
import { AppConfigService } from 'src/app/services/app-config.service';

@Component({
  selector: 'app-otp-details',
  templateUrl: './otp-details.component.html',
  styleUrls: ['./otp-details.component.scss']
})
export class OtpDetailsComponent implements OnInit, OnDestroy {

  @Output() public hideOtpDetailsEvent = new EventEmitter<string>();
  @Output() public closeSpinner = new EventEmitter<any>();
  @Output() public showSpinner = new EventEmitter<any>();
  @Input() public mode: OtpMode;
  @Input() public signerMeans: string;

  state: AppState;
  otpIdentification: string;
  otpCode: string;
  token: string;
  showCode = false;
  showErrorMessage = false;
  isSendRequestBusy = false;
  isSubmitBusy = false;
  errorMessage = "";
  errorCodeMessage = "";
  showErrorCodeMessage: boolean;
  alertMessage: string;
  showAlertMessage: boolean;
  isErrorAlert: boolean;
  isConfirmAlert: boolean;
  sentSignerMeans = "";
  otpSuccesSentToSignerMeans= "";
  timerFunctionId;
  otpExpiredTime: Date;
  tokenTime: number;
  otpLeftTime: string;
  isOtpCodeSent = false;
  hasOtpTimeLeft = false;
  year = 2025;

  constructor(private stateService: StateService, private otpService: OtpService, private route: ActivatedRoute,
    private appConfigService: AppConfigService, private translate: TranslateService, private router: Router) { }

  ngOnDestroy(): void {
    if (this.timerFunctionId) {
      clearInterval(this.timerFunctionId);
      this.hasOtpTimeLeft = false
    }
  }

  ngOnInit(): void {
    this.year = new Date().getFullYear();
    this.stateService.state$.subscribe((data) => {
      this.state = data

      if (this.state.signerMeans != undefined) {
        let endTime = new Date(localStorage.getItem(this.state.signerMeans));
        let now = new Date();

        if (this.state.otpMode != OtpMode.None && localStorage.getItem('shouldSaveOtpLocalStorage') == 'true' && now < endTime) {
          this.isSubmitBusy = false;
          this.stateService.setOtpMode(OtpMode.None);
        }

        if (!this.isOtpCodeSent && this.state.otpMode == OtpMode.CodeRequired) {
          this.generateOtpCode(false);
          this.isOtpCodeSent = true;
        }
      }

      if (this.state.otpMode != undefined && this.state.otpMode != OtpMode.None) {
        this.closeSpinner.emit();
      }
    });

    this.route.paramMap.subscribe(params => {
      this.token = params.get("id");
    });

    this.otpService.initOtpDetailsToLocalStorage();

    this.tokenTime = this.appConfigService.otpTime;

    this.timerFunctionId = setInterval(() => {
      this.timerFunction(this.hasOtpTimeLeft)
    }, 1000);
  }

  timerFunction(hasOtpTimeLeft: boolean): void {
    if (hasOtpTimeLeft) {
      var timeDistance = this.otpExpiredTime.getTime() - new Date().getTime();

      var minutes = Math.floor((timeDistance % (1000 * 60 * 60)) / (1000 * 60));
      var seconds = Math.floor((timeDistance % (1000 * 60)) / 1000);

      if (timeDistance < 0) {
        this.hasOtpTimeLeft = false;
      }

      if (seconds < 10) {
        this.otpLeftTime = `0${minutes}:0${seconds}`
      }

      else {
        this.otpLeftTime = `0${minutes}:${seconds}`
      }
    }
  }

  setOtpExpiredTime() {
    this.otpExpiredTime = new Date();
    this.otpExpiredTime.setMinutes(this.otpExpiredTime.getMinutes() + this.tokenTime);
    this.hasOtpTimeLeft = true
  }

  generateOtpCode(isFromClickEvent: boolean) {
    if ((this.otpIdentification == undefined || this.otpIdentification == "") && isFromClickEvent &&
      (this.mode == OtpMode.IdentificationRequired || this.mode == OtpMode.CodeAndIdentificationRequired)) {
      this.showErrorMessage = true;
      this.errorMessage = this.translate.instant('ERROR.INPUT.0');
      return;
    }

    this.setOtpExpiredTime();

    this.sentSignerMeans = "";
    this.isSendRequestBusy = true;
    this.showErrorMessage = false;
    let input = new GenerateCodeRequest();
    input.token = this.token;
    input.identification = this.otpIdentification;
    this.showSpinner.emit();
    this.otpService.validatePassword(input).subscribe(
      (data) => {
        this.showCode = true;
        this.isSendRequestBusy = false;
        this.showAlertMessage = true;
        this.isErrorAlert = false;
        this.isConfirmAlert = true;
        this.alertMessage = this.translate.instant('ERROR.OPERATION.5');
        if (document.getElementsByTagName("html")[0].lang == 'he' && this.otpSuccesSentToSignerMeans.includes("+")) {
          this.otpSuccesSentToSignerMeans = `${this.translate.instant('OTP.OTP_SENT_SUCCESSFULLY')} ${data.sentSignerMeans.split('+')[1].split('*')[4]}****${data.sentSignerMeans.split('*')[0].replace("+", "")}+`;
        } else {
          this.otpSuccesSentToSignerMeans = `${this.translate.instant('OTP.OTP_SENT_SUCCESSFULLY')} ${data.sentSignerMeans}`;
        }


        this.sentSignerMeans = data.sentSignerMeans;
        if (this.mode == OtpMode.IdentificationRequired) {
          this.stateService.setOtpMode(OtpMode.None);
          if (data.authToken && data.authToken != "00000000-0000-0000-0000-000000000000") {
            this.stateService.SetDocuementToken(data.authToken);
          }
          this.updateSignerTimeToLocalStorage();
        }
        this.closeSpinner.emit();
      },
      (err) => {
        this.closeSpinner.emit();

        let error = new Errors(err.error);

        if (error.errorCode == 150) {
          this.errorCodeMessage = this.translate.instant('OTP.OTP_ATTEMPTS_EXCEEDED');
          this.router.navigate(["/otp-exceeded"]);
        }
        else if (err.status == 0) {
          this.errorMessage = this.translate.instant('SERVER_ERROR.429');
        } else {

          this.errorMessage = this.translate.instant('SERVER_ERROR.' + error.errorCode);
        }
        this.showAlertMessage = true;
        this.isConfirmAlert = false;
        this.isErrorAlert = true;
        this.alertMessage = this.errorMessage;
        this.showErrorMessage = true;
        this.isSendRequestBusy = false;
      });
  }

  isValidCode() {
    if (this.otpCode == undefined || this.otpCode == "") {
      this.showErrorCodeMessage = true;
      this.errorCodeMessage = this.translate.instant('ERROR.INPUT.0');
      return;
    }
    this.showErrorCodeMessage = false;
    this.isSubmitBusy = true;
    this.showSpinner.emit();
    this.otpService.isValidCode(this.token, this.otpCode).subscribe(
      (data) => {
        this.setOtpExpiredTime();
        this.showAlertMessage = true;
        this.isConfirmAlert = true;
        this.isErrorAlert = false;
        this.alertMessage = this.translate.instant('ERROR.OPERATION.5');
        this.isSubmitBusy = false;
        if (this.state.otpMode != OtpMode.None) {
          this.stateService.setOtpMode(OtpMode.None);
          if (data.authToken && data.authToken != "00000000-0000-0000-0000-000000000000") {
            this.stateService.SetDocuementToken(data.authToken);
          }
        }
        this.updateSignerTimeToLocalStorage();
        this.closeSpinner.emit();
      },
      (err) => {
        let error = new Errors(err.error);

        if (error.errorCode == 149) {
          this.errorCodeMessage = this.translate.instant('OTP.OTP_ATTEMPTS_EXCEEDED');
          this.router.navigate(["/otp-exceeded"]);
        }

        else {
          this.errorCodeMessage = this.translate.instant('OTP.INVALID_CODE');
        }

        this.showErrorCodeMessage = true;
        this.isSubmitBusy = false;
        this.closeSpinner.emit();
      });
  }

  updateSignerTimeToLocalStorage() {
    let now = new Date();
    now.setHours(now.getHours() + Number(localStorage.getItem('otpExpirationInHours')));
    localStorage.setItem(this.signerMeans, now.toString());
    //   console.log("update signerMeans = " + localStorage.getItem(this.signerMeans) + " to local storage , end otp time = " + now.toString());
  }

  closeAlert() {
    this.showAlertMessage = false;
  }
}