import { Component, OnInit } from '@angular/core';
import { StateService } from 'src/app/services/state.service';

@Component({
  selector: 'app-loader',
  templateUrl: './loader.component.html',
  styleUrls: ['./loader.component.scss']
})
export class LoaderComponent implements OnInit {

  get showLoader(): boolean {
    return this.stateService.showLoader;
  }

  constructor(private stateService: StateService) { }

  ngOnInit(): void {
  }
}