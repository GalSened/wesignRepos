import { Component, OnInit, NgZone } from "@angular/core";
import { User } from "@models/account/user.model";
import { ProgramInfo } from "@models/program/program-info.model";
import { ProgramStatus } from "@models/program/program-status.model";
import { UserApiService } from "@services/user-api.service";
import { Observable, from } from "rxjs";
import { Router, NavigationStart, NavigationEnd, NavigationError } from '@angular/router';
import { userType } from '@models/enums/user-type.enum';
import {environment} from "../../../../environments/environment"

@Component({
    selector: "sgn-account-nav",
    templateUrl: "account-nav.component.html",
})

export class AccountNavComponent implements OnInit {

    public programStatus$: Observable<ProgramStatus>;
    public programInfo$: Observable<ProgramInfo>;
    public programExploitedPercent: number;
    public showHeaderMenu: boolean = false;
    public userType = userType;
    public tiny =  environment.tiny;

    public user$: Observable<User>;

    constructor(private userApiService: UserApiService, private router: Router) {

        this.user$ = this.userApiService.getCurrentUser(true);
        

        router.events.subscribe((event: any) => { // TODO - Unsubscribe ? 
            if (event instanceof NavigationStart) {
                // Show loading indicator
                this.showHeaderMenu = false;
            }

            if (event instanceof NavigationEnd) {
                // Hide loading indicator
                this.showHeaderMenu = false;
            }

            if (event instanceof NavigationError) {
                // Hide loading indicator

                // Present error to user
             //   console.log(event.error);
            }
        });
    }

    public ngOnInit() {
        /* TODO */
    }

    public logout() {
        this.userApiService.logout();
       
    }

}
