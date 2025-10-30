import { Component, ElementRef, EventEmitter, OnInit, Output, ViewChild } from '@angular/core';
import { Tablet } from '@models/configuration/tablets-configuration.model';
import { ConfigurationApiService } from '@services/configuration-api.service';
import { TabletsSupportAction } from '@state/actions/app.actions';
import { fromEvent } from 'rxjs';
import { map, debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'sgn-select-tablet',
  templateUrl: './select-tablet.component.html',
  styles: []
})
export class SelectTabletComponent implements OnInit {

  @Output() public hide = new EventEmitter<any>();
  @ViewChild('searchInput', { static: true }) searchInput: ElementRef;
  @Output() public sendTablet = new EventEmitter<Tablet>();
  tablets: Tablet[];
  //key: string;

  constructor(private configurationApiService : ConfigurationApiService) { }

  ngOnInit() {
    this.updateData("");
    fromEvent(this.searchInput.nativeElement, 'keyup').pipe(
      // get value
      map((event: any) => {
        return event.target.value;
      })
      // if character length greater then 2
      //,filter(res => res.length > 2)
      //Search function will not get called when the field is empty
      //,filter(Boolean)
      // Time in milliseconds between key events
      , debounceTime(1000)
      // If previous query is diffent from current   
      , distinctUntilChanged()
    ).subscribe((text: string) => {
      this.updateData(text);
    });
  }

  updateData(text) {
    this.configurationApiService.readTablets(text).subscribe(
      (data: Tablet[]) => {
        this.tablets = data
          
        }

      );
    
  }
  
  public close(){
    this.hide.emit();
  }

  public sendTabletToParent(tablet){
    this.sendTablet.emit(tablet);
  }

}
