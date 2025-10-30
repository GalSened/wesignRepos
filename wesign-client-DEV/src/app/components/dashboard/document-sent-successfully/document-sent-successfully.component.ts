import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'sgn-document-sent-successfully',
  templateUrl: './document-sent-successfully.component.html',
  styles: []
})
export class DocumentSentSuccessfullyComponent implements OnInit {
  isSelfSign: boolean;

  constructor( private router: Router,) { }

  ngOnInit() {
    let parts = this.router.url.split('/');
    this.isSelfSign = parts[parts.length-1] == "selfsign";
  }

  public goToSentItems()
  {
    this.router.navigate(["/dashboard", "documents"]);  
  }
  public goToDashboard()
  {
    this.router.navigate(["/dashboard", "main"]);
  }

}
