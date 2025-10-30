import { Component, OnInit } from '@angular/core';
import { UsersApiService } from 'src/app/services/users-api.service';
import { OTPApiService } from 'src/app/services/otp-api.service';

@Component({
  selector: 'app-qr-code',
  templateUrl: './qr-code.component.html',
  styleUrls: ['./qr-code.component.css']
})
export class QrCodeComponent implements OnInit {

  public image : string ="";
  constructor(public otpApiService: OTPApiService) { }

  ngOnInit(): void {
    this.otpApiService.ReadQRCode().subscribe(
      (data)=>{
        this.image = data.image
      },
      (error)=>{
        console.log("error qr code");
      }
    );
  }

}
