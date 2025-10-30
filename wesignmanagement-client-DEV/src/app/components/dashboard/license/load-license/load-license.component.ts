import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { LicenseService } from 'src/app/services/license-api.service';
import { ActivateLicense } from 'src/app/models/activate-license';
import { LicenseStatus } from 'src/app/models/license-status.enum';


@Component({
  selector: 'app-load-license',
  templateUrl: './load-license.component.html',
  styleUrls: ['./load-license.component.css']
})

export class LoadLicenseComponent implements OnInit {
  @ViewChild('input') myTextArea: any;
  @Output() public activeLicenseSucceedEvent = new EventEmitter<boolean>();
  public failedActivate: boolean;
  public isBusy: boolean = false;

  constructor(private licenseApiService: LicenseService) { }

  ngOnInit(): void {
  }

  public activate() {
    let license = new ActivateLicense();
    license.license = this.myTextArea.nativeElement.value;
    this.isBusy = true;

    this.licenseApiService.update(license).subscribe(
      (event) => {
        this.activeLicenseSucceedEvent.emit(true)
      },
      (err) => {
        this.failedActivate = true;
        this.isBusy = false;
      },
      () => {
        this.isBusy = false;
      }
    );
  }
}
