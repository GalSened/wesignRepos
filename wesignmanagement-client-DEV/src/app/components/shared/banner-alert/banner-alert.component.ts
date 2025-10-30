import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'app-banner-alert',
  templateUrl: './banner-alert.component.html',
  styleUrls: ['./banner-alert.component.css']
})
export class BannerAlertComponent implements OnInit {

  public showBanner : boolean = false;
  public bannerMessage : string = ""; 
  public alertType : AlertType;

  constructor() { }

  ngOnInit(): void {
  }

  public showBannerAlert(message : string, type: AlertType) {
    this.bannerMessage = message;
    this.alertType = type;
    if (this.showBanner) { // if the alert is visible return
      return;
    }
    this.showBanner = true;
    setTimeout(() => this.showBanner = false, 5500); // hide the alert after 5.5s
  }

  public hideBannerAlert(){
    this.showBanner = false;
  }

}

export enum AlertType{
  SUCCESS = 1,
  FAILED = 2
}
