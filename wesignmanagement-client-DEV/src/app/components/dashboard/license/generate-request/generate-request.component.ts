import { Component, OnInit } from '@angular/core';
import { UserInfo } from 'src/app/models/user-info.model';
import { LicenseService } from 'src/app/services/license-api.service';
import { saveAs } from 'file-saver';
import { LicenseStatus } from 'src/app/models/license-status.enum';

import { Errors } from 'src/app/models/error/errors.model';
import { SharedService } from 'src/app/services/shared.service';

@Component({
  selector: 'app-generate-request',
  templateUrl: './generate-request.component.html',
  styleUrls: ['./generate-request.component.css']
})
export class GenerateRequestComponent implements OnInit {

  public userInfo = new UserInfo();
  public message: string = "";
  public submited: boolean = false;
  public showEula: boolean;
  public isBusy: boolean = false;

  constructor(private licenseApiService: LicenseService,
              private sharedService : SharedService) { }

  ngOnInit(): void { 
    this.showEula = true;

  }
  public generateLicense() {
    this.submited = true;
    this.isBusy = true;
    this.licenseApiService.generateLicense(this.userInfo).subscribe(
      (event) => {
        
        if (event.licenseStatus == LicenseStatus.Succsess) //success but didnt updated the dmz
        {
          this.message = event.license;
          var data = new Blob([this.message], { type: 'text/plain' });
          const fileName = 'request.txt';
          saveAs(data, fileName);
        }
        if(event.licenseStatus == LicenseStatus.SentToDMZ){
          this.message = "License request sent successfully...\n" + event.license;
        }
      },
      (err) => {
        let errorMessage = this.sharedService.getErrorMessage(new Errors(err.error));
        this.message = errorMessage;
        this.isBusy = false;
      },
      () => {
        this.submited = false;
        this.isBusy = false;
      }
    );
  }

  public onSubmitClicked()
  {
    console.log("submit clicked");
    this.showEula = false;
  }
}
