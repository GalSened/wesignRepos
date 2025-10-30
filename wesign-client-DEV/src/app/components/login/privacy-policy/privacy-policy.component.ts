import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import { environment } from "../../../../environments/environment"

@Component({
  selector: 'sgn-privacy-policy',
  templateUrl: './privacy-policy.component.html',
  styles: []
})
export class PrivacyPolicyComponent implements OnInit {
  public tiny = environment.tiny;;
  public links: { [key: string]: string[] } = {
    'REGISTER.PRIVACY': ['/privacy'],
    'REGISTER.TERMS': ['/terms']
  }

  constructor(private router: Router) { }

  ngOnInit( ) {   }

  public back(){
    this.router.navigate(["dashboard"]);

  }

}
