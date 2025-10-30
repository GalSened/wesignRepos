import { Component, OnInit } from '@angular/core';
import { User } from '@models/account/user.model';
import { userType } from '@models/enums/user-type.enum';
import { Modal } from '@models/modal/modal.model';
import { TranslateService } from '@ngx-translate/core';
import { UserApiService } from '@services/user-api.service';
import { Subscription } from 'rxjs';
import { environment } from "../../../../environments/environment"

@Component({
    selector: 'sgn-profile-wrapper',
    template: `
        <sgn-pop-up-confirm [data]="popUpModal" (submitEvent)="logout()" 
            (cancelEvent)="popUpModal.showModal = false">
        </sgn-pop-up-confirm>
        <div class="ct-p-profile">
            <sgn-alert-component></sgn-alert-component>
            

            <sgn-top-header-title [title]="'GLOBAL.PROFILE.MY_PROFILE' | translate"></sgn-top-header-title>

            <main>
                <sgn-side-page-menu [links]="links" [menuAction]="'GLOBAL.TOPMENU.SIGNOUT'" (menuActionClicked)="showLogOutPopup()"></sgn-side-page-menu>
                <div style="flex:1">
                    <router-outlet></router-outlet>
                </div>
            </main> 
        </div>
    `
    // <sgn-loading></sgn-loading>
})

export class ProfileWrapperComponent implements OnInit {

    public links: { [key: string]: string[] } = {
        'GLOBAL.PROFILE.USER_INFO': ['./info'],
        'GLOBAL.PROFILE.CHANGE_PASS': ['./password'],
        'GLOBAL.PROFILE.MY_PLAN': ['./plans']
    }
    

    public tiny = environment.tiny;
    user$: any;
    userType: userType;
    public popUpModal: Modal = new Modal();
    
    constructor(
        private userApiService: UserApiService,
        private translate: TranslateService
    ) { }

    ngOnInit() {
        this.user$ = this.userApiService.getCurrentUser(); 
        this.user$.subscribe((user:User) => { 
            this.userType = user.type;
            if(!this.tiny) 
            {
                if(this.userType == userType.Editor)
                {
                    this.links  = {
                        'GLOBAL.PROFILE.USER_INFO': ['./info'],
                        'GLOBAL.PROFILE.CHANGE_PASS': ['./password'],
                        'GLOBAL.PROFILE.MY_PLAN': ['./plans'],                        
                        'GLOBAL.PROFILE.MANAGE_CONTACTS': ['./contacts']   
                                     
                        };
                }
                if (this.userType == userType.CompanyAdmin|| this.userType == userType.SystemAdmin){
                this.links  = {
                    'GLOBAL.PROFILE.USER_INFO': ['./info'],
                    'GLOBAL.PROFILE.CHANGE_PASS': ['./password'],
                    'GLOBAL.PROFILE.MY_PLAN': ['./plans'],
                    'GLOBAL.PROFILE.MANAGE_USERS': ['./users']    ,
                    'GLOBAL.PROFILE.MANAGE_CONTACTS': ['./contacts']               
                    };
                }
            }
        });
        this.translate.get(['LOGOUT.MODALTITLE', 'LOGOUT.MODALTEXT', 'REGISTER.CANCEL', 'BUTTONS.SUBMIT'])
            .subscribe((res: object) => {
                let keys = Object.keys(res);
                this.popUpModal.title = res[keys[0]];
                this.popUpModal.content = res[keys[1]];
                this.popUpModal.rejectBtnText = res[keys[2]];
                this.popUpModal.confirmBtnText = res[keys[3]];
            });
    }


    public logout() {
        this.userApiService.logout();
        
    }

    public showLogOutPopup() {
        this.popUpModal.showModal = true;        
    }
}