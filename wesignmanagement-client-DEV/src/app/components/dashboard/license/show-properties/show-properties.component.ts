import { Component, OnInit } from '@angular/core';
import { tap, map } from 'rxjs/operators';
import { LicenseService } from 'src/app/services/license-api.service';
import { Observable } from 'rxjs';

import { LicenseProperties } from 'src/app/models/license-properties';
import { WeSignLicense } from 'src/app/models/wesign-license';

@Component({
  selector: 'app-show-properties',
  templateUrl: './show-properties.component.html',
  styleUrls: ['./show-properties.component.css']
})
export class ShowPropertiesComponent implements OnInit {

  public wesignlicense: WeSignLicense;
  public licneseActivted :boolean;
  public licenseExpiredInDays:number;

  constructor(private licenseApiService: LicenseService) {    
    this.fetchData();

  }

  ngOnInit(): void {
  }

  fetchData() {
    this.licenseApiService.read().subscribe(
      (event) => {       
          this.wesignlicense = event;
          var currDate = new Date();
          this.licenseExpiredInDays= this.calculateDiff(event.licenseLimits.expirationTime);
          this.licneseActivted = true;
      },
      (err) => {
        this.licneseActivted = false;
      },
      () => {
      }
    );
  }
  calculateDiff(dateInput: Date){
    let currentDate = new Date();
    dateInput = new Date(dateInput);
  
    return Math.abs(Math.floor((Date.UTC(currentDate.getFullYear(), currentDate.getMonth(), currentDate.getDate()) 
    - Date.UTC(dateInput.getFullYear(), dateInput.getMonth(), dateInput.getDate()) ) /(1000 * 60 * 60 * 24)));
  }
}

