import { Component, OnInit } from '@angular/core';
import { Location } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-privacy-policy',
  templateUrl: './privacy-policy.component.html',
  styleUrls: ['./privacy-policy.component.scss']
})
export class PrivacyPolicyComponent implements OnInit {

  isPrivacySelected:boolean;

  constructor(private router: Router,
    private location : Location) { }

  ngOnInit(): void {
    this.isPrivacySelected = this.router.url == "/privacy";    
  }

  back(){
    this.location.back();
  }
}