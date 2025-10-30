import { Component, OnInit, OnDestroy } from '@angular/core';
import { UserInfo } from 'src/app/models/user-info.model';
import { LicenseService } from 'src/app/services/license-api.service';
import { LicenseStatus } from 'src/app/models/license-status.enum';
import { Errors } from 'src/app/models/error/errors.model';
import { SharedService } from 'src/app/services/shared.service';
import { Router } from '@angular/router';
import { ActivateLicenseAction } from 'src/app/state/app.action';
import { IAppState, AppState } from 'src/app/state/app-state.interface';
import { Store } from '@ngrx/store';
import { UsersApiService } from 'src/app/services/users-api.service';
import { Subscription } from 'rxjs';
import { NgxSpinnerService } from 'ngx-spinner';

@Component({
  selector: 'app-license',
  templateUrl: './license.component.html',
  styleUrls: ['./license.component.css']
})
export class LicenseComponent implements OnInit, OnDestroy {
  public errorMessage: string = "Error - Please correct the marked fields";
  public userInfo = new UserInfo();
  public message: string = "Retrieving license...";
  public generateRequest = false;
  public loadLicense = false;
  public showProp = false;
  public showPropButton = false;
  public appState: AppState;
  private apiSub: Subscription;

  constructor(private licenseApiService: LicenseService,
    private sharedService: SharedService,
    private userApiService: UsersApiService,
    private store: Store<IAppState>,
    private spinner: NgxSpinnerService) { 

    }

  ngOnDestroy(): void {
    if (this.apiSub){
      this.apiSub.unsubscribe();
    }
  }

  ngOnInit(): void {
    this.store.select<any>('appstate').subscribe((state: any) => {
      this.appState = state;
    });
    this.fetchData();
  }

  fetchData() {
    this.spinner.show();
    this.apiSub = this.licenseApiService.read().subscribe(
      () => {
        this.message = "License Activated";
        this.showPropButton = true;
        this.store.dispatch(new ActivateLicenseAction({ Token: this.userApiService.accessToken, RefreshToken: this.userApiService.refreshAccessToken }));
        this.spinner.hide();
      },
      (err) => {
        this.message = this.sharedService.getErrorMessage(new Errors(err.error));
        this.showPropButton = false;
        this.spinner.hide();
      }

    );
  }

  private showProperties() {
    this.generateRequest = false;
    this.loadLicense = false;
    this.showProp = true;
  }
  public activeLicenseSucceedEvent(event) {
    this.fetchData();
    this.showProperties();
  }
  public toggleClick(event) {
    switch (event.target.id) {
      case "properties":
        {
          this.showProperties();

          break;
        }
      case "generate":
        {
          this.generateRequest = true;
          this.loadLicense = false;
          this.showProp = false;
          break;
        }
      case "activate":
        {
          this.generateRequest = false;
          this.loadLicense = true;
          this.showProp = false;
          break;
        }
    }
  }


}
