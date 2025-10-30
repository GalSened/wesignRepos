import { Component, OnInit } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { AppConfigService } from './services/app-config.service';
import { VersionCheckService } from './services/version-check.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'wesign-signer-client-app';

  constructor(private translate: TranslateService, private appConfigService: AppConfigService, private versionCheckService: VersionCheckService) {
    this.translate.setDefaultLang('en');
  }

  ngOnInit(): void {
    this.versionCheckService.initVersionCheck(`${this.appConfigService.signerUrl}/version.json`, 1000 * 60 * this.appConfigService.checkVersionInMinutes);
  }
}