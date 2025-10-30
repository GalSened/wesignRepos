import { Router } from '@angular/router';
import { UsersApiService } from 'src/app/services/users-api.service';
import { Component, OnInit, EventEmitter, Output, ViewChild } from '@angular/core';
import { BannerAlertComponent } from 'src/app/components/shared/banner-alert/banner-alert.component';
import { Errors } from 'src/app/models/error/errors.model';
import { SharedService } from 'src/app/services/shared.service';
import { AppState, IAppState } from 'src/app/state/app-state.interface';
import { Store } from '@ngrx/store';

@Component({
  selector: 'app-change-password',
  templateUrl: './change-password.component.html',
  styleUrls: ['./change-password.component.css']
})
export class ChangePasswordComponent implements OnInit {

  private SUCCESS = 1;
  private FAILED = 2;
  public newPassword : string = "";
  public confirmPassword : string = "";

  public showError : boolean = false; 
  public showUpdateAlert : boolean = false;
  public showAlert : boolean = false; 
  public passMinLengthMsg: string;

  
  @ViewChild('bannerAlert', { static: true }) bannerAlert: BannerAlertComponent;
  @Output() public removeChangePasswordForm = new EventEmitter<number>();
  
  constructor(private usersApi :UsersApiService,
              private sharedService: SharedService) {
     
               }

  ngOnInit(): void {
  }

  public cancel(){
    this.hideTheForm();
    //this.showAlert = false;
  }

  public hideTheForm(){ 
    this.removeChangePasswordForm.emit()
  }

  public update(){
    if(this.newPassword == "" || this.newPassword != this.confirmPassword){
      this.showError = true;
      this.hideAlert();
      return;
    }

    this.usersApi.resetPassword(this.newPassword).subscribe(
      (data)=>{
        this.bannerAlert.showBannerAlert("Successfully Update Password", this.SUCCESS);        
        this.showError = false;
        this.hideAlert();
      },
      (err)=>{
        let errors = new Errors(err.error);
        let passMinLength = this.sharedService.getMinimumPasswordLengthFromError("NewPassword", errors);
        if (passMinLength == null) {
          let errorMessage = this.sharedService.getErrorMessage(new Errors(err.error));
          this.bannerAlert.showBannerAlert(errorMessage, this.FAILED);        
          this.showError = false;
        }
        else {
          this.passMinLengthMsg = `Password should contain at least one digit, one special character and at least ${passMinLength} characters long`;
        }
        this.hideAlert();
      }
    );
  }

  public hideAlert(){
    this.showAlert = false;
  }

  public showPopUpAlert(){
    this.showAlert =true;
    
  }

  public togglePassword(name:string){    
    let passwordElement  = document.getElementsByName(name);    
    let type = passwordElement[0].getAttribute('type');
    passwordElement[0].setAttribute('type', type === 'password' ? 'text' : 'password');
    return true;
  }

 
}
