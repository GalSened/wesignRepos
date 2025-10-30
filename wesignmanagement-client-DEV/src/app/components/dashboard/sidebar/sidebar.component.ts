import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AppState, IAppState } from 'src/app/state/app-state.interface';
import { Store } from '@ngrx/store';
import { HideCompanyFormAction, HideProgramFormAction, HideUserFormAction } from 'src/app/state/app.action';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css']
})
export class SidebarComponent implements OnInit {

  public appState: AppState;

  constructor(private router: Router,
              private store: Store<IAppState>) { }


  ngOnInit(): void {
    this.store.select<any>('appstate').subscribe((state: any) => {
      this.appState = state;
    });
  }

  public selectView(view: string) {
    let array = this.router.url.split("/");
    if(array[array.length-1] == view){
      if(view == "companies") {
        this.store.dispatch(new HideCompanyFormAction());
      }
      if(view == "users") {
        this.store.dispatch(new HideUserFormAction());
      }
      if(view == "programs") {
        this.store.dispatch(new HideProgramFormAction());
      }
    }
    this.router.navigate(["/dashboard", view]);
  }

}
