import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DashboardMode } from 'src/app/enums/dashboard-mode.enum';
import { IAppState, AppState } from 'src/app/state/app-state.interface';
import { Store } from '@ngrx/store';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {

  public dashboardMode: DashboardMode;
  public appState: AppState;

  constructor(private route: ActivatedRoute,
              private store: Store<IAppState>) {
    this.route.params.subscribe(params => {
      if (params.view != null) {
        this.dashboardMode = (<any>DashboardMode)[params.view.toString().toUpperCase()];
      }
    });
    this.store.select<any>('appstate').subscribe((state: any) => {
      this.appState = state;
  });
  }

  ngOnInit(): void {

  }
}
