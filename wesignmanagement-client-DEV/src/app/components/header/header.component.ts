import { UsersApiService } from 'src/app/services/users-api.service';
import { Component, OnInit } from '@angular/core';
import { AppState, IAppState } from 'src/app/state/app-state.interface';
import { Store } from '@ngrx/store';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit {

  public showHeaderMenu: boolean = false;
  appState: any;

  constructor(private userApiService: UsersApiService,
    private store: Store<IAppState>) { }

  ngOnInit(): void {
    this.store.select<any>('appstate').subscribe((state: AppState) => {
      this.appState = state;
    });
  }

  public logout(){
    this.userApiService.logout();
  }
}
