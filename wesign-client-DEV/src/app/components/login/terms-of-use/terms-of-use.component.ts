import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import { environment } from "../../../../environments/environment"

@Component({
  selector: 'sgn-terms-of-use',
  templateUrl: './terms-of-use.component.html',
  styles: []
})
export class TermsOfUseComponent implements OnInit {

  public tiny = environment.tiny;
  
  public links: { [key: string]: string[] } = {
    'REGISTER.PRIVACY': ['/privacy'],
    'REGISTER.TERMS': ['/terms']
  }

  constructor(private router: Router, private location: Location) { }

  ngOnInit() { }

  public back(){
    this.router.navigate(["dashboard"]);

  }

}
