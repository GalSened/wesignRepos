import { Component, EventEmitter, Output } from '@angular/core';
import { SelfSignApiService } from '@services/self-sign-api.service';
import { SharedService } from '@services/shared.service';

@Component({
  selector: 'sgn-smart-card-signing-alert',
  templateUrl: './smart-card-signing-alert.component.html',
  styles: [
  ]
})
export class SmartCardSigningAlertComponent {

  @Output() public hide = new EventEmitter<any>();
  @Output() public sign = new EventEmitter<any>();
  constructor(private sharedService: SharedService,
    private selfSignApiService: SelfSignApiService) { }
    
  public smartCardSigning() {
    this.sign.emit();
    this.closePopUp();
  }

  public closePopUp() {
    this.hide.emit();
  }

  public downloadSmartCardInstaller() {
    // this.isBusy = true;
    this.selfSignApiService.downloadSmartCardDesktopClientInstaller().subscribe(
      (data) => {
        let fn = data.headers.get("x-file-name");
        const filename = decodeURIComponent(fn) ? decodeURIComponent(fn) : "setup";
        const blob = new Blob([data.body], { type: "application/octet-stream" });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;
        a.target = "_blank";
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        // this.isBusy = false;
      },
      (err) => {
        //TODO if failed to download, redirect to another page 
        // this.isBusy = false;
        let ex = this.sharedService.convertArrayBufferToErrorsObject(err.error);
        this.sharedService.setErrorAlert(ex);
        this.sharedService.setBusy(false);
      });;
  }
}