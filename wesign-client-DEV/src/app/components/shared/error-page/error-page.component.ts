import { Component, OnInit } from '@angular/core';
import {environment} from "../../../../environments/environment";

@Component({
  selector: 'sgn-error-page',
  templateUrl: './error-page.component.html',
  styles: []
})
export class ErrorPageComponent implements OnInit {
  public tiny =  environment.tiny;
  
  constructor() { }

  ngOnInit() {
  }

}
