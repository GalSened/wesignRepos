import { AfterViewInit, Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { User } from '@models/account/user.model';
import { Errors } from '@models/error/errors.model';
import { TranslateService } from '@ngx-translate/core';
import { UserApiService } from '@services/user-api.service';


@Component({
  selector: 'sgn-edit-user-phone',
  templateUrl: './edit-user-phone.component.html',
  styles: [
  ]
})
export class EditUserPhoneComponent implements OnInit,  OnDestroy, AfterViewInit{

    @Input() user: User;
    @Output() public accept = new EventEmitter<void>();
    @Output() public cancel = new EventEmitter<void>();
    public submited: boolean = false;
    public errorMsg : string = '';
    public code = ""; // OTP code
    public OTPProccess = false; 
    public otpLeftTime: string;
    public timerFunctionId;
    public otpExpiredTime: Date;
    public hasOtpTimeLeft: boolean = false;
    public showOtpResend: boolean = false;
    public timertimeOutId;
    public userPhone: string = "";
    public numberOfResend = 3;
    

    constructor( private userApiService: UserApiService,
      private translate: TranslateService,
    ) { }


  ngAfterViewInit(): void {
   this.userPhone = this.user.phone;
   this.numberOfResend = 3;
  }
  ngOnDestroy(): void {
    if (this.timerFunctionId) {
      clearInterval(this.timerFunctionId);
      this.hasOtpTimeLeft = false;
    }
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

    updatePhone(){

    if(!this.isValidPhoneNumber()){
       return;
  
    } 

      if(this.OTPProccess){
        this.validateOtp();
      }
      else{
      this.submited = true;
      this.showOtpResend = false;
      try{
        this.errorMsg = "";
        this.code = "";
        this.userApiService.updatePhoneStartProcess(this.userPhone).subscribe(
          x => {
            this.OTPProccess = true;
            this.submited = false;
            if(this.timertimeOutId){
              clearTimeout(this.timertimeOutId);
            }
            this.timertimeOutId = setTimeout(() => {
              if(this.numberOfResend > 0){
              this.showOtpResend = true;
              this.numberOfResend--;
          }
        },15000);
            this.setOtpExpiredTime();
          },
          err =>{
            this.submited = false;
            
            
                
            if (err.status == 0) {
              this.errorMsg = this.translate.instant('SERVER_ERROR.429');
            } else {
              let result = new Errors(err.error);
              this.errorMsg = this.translate.instant('SERVER_ERROR.' + result.errorCode);
            }
          
          }
        );
        


      }
      finally{
      }
     
    }
      // validate phone
      // update user phone

    }
    validateOtp(){

      if (this.code == undefined || this.code.trim() == "" || this.code.trim().length != 6 ||
      !this.code) {
          
          this.errorMsg = this.translate.instant('OTP.INVALID_CODE');
          return;
        }

        this.submited = true;
      this.userApiService.updatePhoneValidateOtp(this.code).subscribe(
        x => {
          this.user.phone = this.userPhone;
          this.accept.emit();
        } ,
              (err) => {
                
                this.submited = false;
                
                if (err.status == 0) {
                  this.errorMsg = this.translate.instant('SERVER_ERROR.429');
                } else {
                  let result = new Errors(err.error);
                  this.errorMsg = this.translate.instant('SERVER_ERROR.' + result.errorCode);
                }
              
              }
      );

    }
resendOtp(){
  this.code = "";
  this.OTPProccess = false;
  this.updatePhone();
}
public isValidToShowSubmit(phone)
{
  if(this.OTPProccess)
  {
    return  this.code == undefined || this.code.trim() == "" || this.code.trim().length != 6;
  }
  else{
    return !(phone.valid && !this.isPhoneNumberEmpty())
  }
}
private setOtpExpiredTime() {
  this.otpExpiredTime = new Date();
  this.otpExpiredTime.setMinutes(this.otpExpiredTime.getMinutes() + 5);
  this.hasOtpTimeLeft = true
}

    public isPhoneNumberEmpty()
    {
      return this.userPhone == null || this.userPhone == '';
    }
    public isValidPhoneNumber(){
      if(this.isPhoneNumberEmpty() || !this.validateIsraeliNumber(this.userPhone)){
        
        return false;
      }
      
      return true;
    }
     validateIsraeliNumber(phoneNumber: string): boolean {
      const pattern = /^05[0-9]?\d{7}$/;
      return pattern.test(phoneNumber);
  }
}
