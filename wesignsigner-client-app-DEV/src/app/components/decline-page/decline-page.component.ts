import { Component, OnInit } from '@angular/core';
import { AppConfigService } from 'src/app/services/app-config.service';

@Component({
  selector: 'app-decline-page',
  templateUrl: './decline-page.component.html',
  styleUrls: ['./decline-page.component.scss']
})
export class DeclinePageComponent implements OnInit {
  public registerUrl : string = "";
  public year = 2025;
  constructor(private appConfigService: AppConfigService) { }

  ngOnInit(): void {
    this.year = new Date().getFullYear();
      this.registerUrl = this.appConfigService.registerUrl;
  }

  moveToRegister(){
    window.open(this.registerUrl, "_blank");
  }
}