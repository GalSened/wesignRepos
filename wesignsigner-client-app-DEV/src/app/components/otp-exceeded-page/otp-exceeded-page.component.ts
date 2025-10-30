import { Component } from '@angular/core';
import { AppConfigService } from 'src/app/services/app-config.service';

@Component({
  selector: 'app-otp-exceeded-page',
  templateUrl: './otp-exceeded-page.component.html',
  styleUrls: ['./otp-exceeded-page.component.scss']
})
export class OtpExceededPageComponent {
  registerUrl = "";
  year = 2025;
  
  constructor(private appConfigService: AppConfigService) { }

  ngOnInit(): void {
    this.year = new Date().getFullYear();
    this.registerUrl = this.appConfigService.registerUrl;
  }
}