import { Component, ElementRef, OnInit, ViewChild, OnDestroy, Output } from "@angular/core";
import { Router } from "@angular/router";
import { LangList, Language } from "@components/shared/languages.list";
import { UserImage } from "@models/account/user-image.model";
import { User } from "@models/account/user.model";
import { SharedService } from "@services/shared.service";
import { UserApiService } from "@services/user-api.service";
import { NgModel } from '@angular/forms';
import { Store } from '@ngrx/store';
import { IAppState } from '@state/app-state.interface';
import * as styleActions from "@state/actions/style.actions";
import { userType } from '@models/enums/user-type.enum';
import { Observable, Subscription } from 'rxjs';
import { Errors } from '@models/error/errors.model';
import { TranslateService } from '@ngx-translate/core';

@Component({
    selector: "sgn-profile",
    templateUrl: "profile.component.html",
})

export class ProfileComponent implements OnInit, OnDestroy {
    public user$: Observable<User>;
    public file: any = null;
    public imgUrl: string;
    public userType: string;
    public userTypeJson: string = "GLOBAL.USER_TYPES.";
    public userTypeJsonWithValue: string;
    public enableSignReminderSettings: boolean;
    public enableDisplaySignerNameInSignature: boolean;
    public errorsArr: string[] = [];
    public submited: boolean = false;
    public selectedOption: string;

    public showUpdatePhone: boolean = false;
    public methods ;
    public color: string;
    public languageOptions = LangList.reduce((prev, curr) => {
        prev[curr.Enum] = curr.Description;
        return prev;
    }, {});

    @ViewChild("file") public el: ElementRef;
    @ViewChild("name") public el_name: NgModel;
    @ViewChild("reminderFrequency") public el_reminderFrequency: NgModel;
    @ViewChild("email") public el_email: NgModel;
    @ViewChild("username") public el_usename: NgModel;

    

    private userSubscription: Subscription;
    public isBlue: boolean;

    constructor(
        private userApiService: UserApiService,
        private sharedService: SharedService,
        private translate: TranslateService,
        private router: Router,
        private store: Store<IAppState>,
    ) {
        this.methods = { 1: this.translate.instant(`LANGUAGE.ENGLISH`), 2: this.translate.instant(`LANGUAGE.HEBREW`) }
        this.user$ = this.userApiService.getCurrentUser(); // TODO merge
        this.userSubscription = this.user$.subscribe(user => { // TODO merge
            this.userType = userType[user.type];
            this.userTypeJsonWithValue = this.userTypeJson + this.userType;
            this.selectedOption = user.userConfiguration.language == Language.ENGLISH ? this.translate.instant(`LANGUAGE.ENGLISH`) : this.translate.instant(`LANGUAGE.HEBREW`);
            this.color = user.userConfiguration.signatureColor;
            this.isBlue = this.color == "#0000FF";
            this.enableSignReminderSettings = user.enableSignReminderSettings;
            this.enableDisplaySignerNameInSignature = user.enableDisplaySignerNameInSignature;
        });

    }

    public ngOnInit() {
        this.store.dispatch(new styleActions.StyleHeaderClassesAction({ Classes: "ws_is-not-fixed" }));
    }

    ngOnDestroy(): void {
        this.userSubscription.unsubscribe();
    }

    public fileDropped() {
        if (this.el.nativeElement.files.length > 0) {
            this.file = this.el.nativeElement.files[0];
            const reader = new FileReader();

            reader.onload = (event: ProgressEvent) => {
                const img = new UserImage();
                img.Name = this.file.name;
                img.Content = (event.target as FileReader).result as string;
                //this.user.Images = [img];
            };

            reader.readAsDataURL(this.file);

        } else {
            this.file = null;
        }
    }
    public containsHebrewLetters(str): boolean {
    
        for (var i = 0; i < str.length; i++) {
          if (/^[[\u0590-\u05FF]*$/.test(str.charAt(i))) {
            return true;  
        }
      }
       return false;
        
      }
private isValidUserName(){

    if(this.el_usename.value && this.el_usename.value.trim() != "")
    {
      if((this.el_usename.value.length < 6 || this.el_usename.value.length > 15))
      {
      
      this.translate.get(`MANAGEMENT.USER_NAME_TOO_LONG_OR_SHORT`).subscribe(
        (msg) => {
            this.errorsArr.push( msg);
        });
      
      return false;
      }

      if(this.containsHebrewLetters(this.el_usename.value))
      {
        
        this.translate.get(`MANAGEMENT.USER_NAME_CANNOT_CONTAINNS_HEB_LETTERS`).subscribe(
        (msg) => {
            this.errorsArr.push( msg);
        });
        return false;
       }
    }
    return true;

}

public phoneUpdated(){
    this.sharedService.setSuccessAlert("ERROR.OPERATION.1");
    this.showUpdatePhone = false;
}
    public update(user: User) {
        this.errorsArr = [];
        this.submited = true;
        user.userConfiguration.language = this.selectedOption == this.translate.instant(`LANGUAGE.ENGLISH`) ? Language.ENGLISH : Language.HEBREW;
        user.userConfiguration.signatureColor = this.color;
        const userColor = user.userConfiguration.signatureColor;
            if(!this.isValidUserName())
            {
                this.submited = false;
                return
            }
if (!this.el_name.invalid && !this.el_email.invalid && ((user.userConfiguration.shouldNotifySignReminder && !this.el_reminderFrequency?.invalid) || !user.userConfiguration.shouldNotifySignReminder)) {
            this.userApiService.updateUser(user).subscribe((_) => {
                window.scroll(0, 0);
                this.sharedService.setSuccessAlert("ERROR.OPERATION.1");
                this.methods = { 1: this.translate.instant(`LANGUAGE.ENGLISH`), 2: this.translate.instant(`LANGUAGE.HEBREW`) }
                this.submited = false;
            }, (error) => {
                window.scroll(0, 0);
                this.sharedService.setErrorAlert(new Errors(error.error));
                this.submited = false;
            });
        } else {
            this.submited = false;
            if (this.el_name.invalid)
                this.errorsArr.push('ERROR.INPUT.I_NAME');
            if (this.el_email.invalid)
                this.errorsArr.push('ERROR.INPUT.I_EMAIL');
            if (this.el_usename.invalid)
                this.errorsArr.push('ERROR.INPUT.I_USERNAME');
            if (this.el_reminderFrequency.invalid)
                this.errorsArr.push('ERROR.INPUT.I_REMINDER_FREQUENCY');
        }
    }

    public back() {
        const prevUrl = sessionStorage.getItem("prev.url");
        this.router.navigateByUrl(prevUrl || "/dashboard");
    }

    public showHideupdatePhoneModal(){
        this.showUpdatePhone = !this.showUpdatePhone ;
    }
    public onBlueSelected() {
        this.isBlue = true;
        this.color = "#0000FF";
    }

    public onBlackSelected() {
        this.isBlue = false;
        this.color = "#000000";
    }

    public changeDayState(user : User) {
        if (!user.userConfiguration.shouldNotifySignReminder)
        {
            user.userConfiguration.signReminderFrequencyInDays = 0;
        }
        else    
        {
            user.userConfiguration.signReminderFrequencyInDays = undefined;
        }

    }

    // public logout() {
    //     this.userApiService.logout();
    //     this.userApiService.redirectForLogin();
    // }
}
