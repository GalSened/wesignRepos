import { Component, OnInit } from '@angular/core';
import { Location } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-terms-of-use',
  templateUrl: './terms-of-use.component.html',
  styleUrls: ['./terms-of-use.component.scss']
})
export class TermsOfUseComponent implements OnInit {

  isPrivacySelected: boolean;

  constructor(private router: Router, private location: Location) { }

  ngOnInit(): void {
    this.isPrivacySelected = this.router.url == "/privacy";
  }

  back() {
    this.location.back();
  }
}