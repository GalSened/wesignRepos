import { Component, OnInit } from "@angular/core";
import { AppConfigService } from '@services/app-config.service';
import { VersionCheckService } from '@services/version-check.service';

@Component({
  selector: "sgn-root",
  styles: [],
  templateUrl: "app.component.html",
})
export class AppComponent implements OnInit {

  public title = "WeSign";

  constructor(private appConfigService :AppConfigService,
    private versionCheckService : VersionCheckService) {
    
  }
  ngOnInit(): void {
    this.versionCheckService.initVersionCheck(`${this.appConfigService.userUrl}/version.json`, 1000 * 60 * this.appConfigService.checkVersionInMinutes);
  }

}
