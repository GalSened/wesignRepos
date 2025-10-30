import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { Store } from '@ngrx/store';
import { StateProcessService } from '@services/state-process.service';
import * as selectActions from "@state/actions/selection.actions";
import { AppState, IAppState } from '@state/app-state.interface';
import { X } from 'angular-feather/icons';

@Component({
  selector: 'sgn-template-select',
  templateUrl: './template-select.component.html',
  styles: []
})
export class TemplateSelectComponent implements OnInit {

  @Input() public name : string;
  @Input() public index : number;
  @Output() public moveTemplate = new EventEmitter<any>();
  templatesLength:number;
  constructor(
    private stateService: StateProcessService
  ) { }

  ngOnInit() {
    this.stateService.getState().subscribe(
      (x : AppState)=>{
        this.templatesLength = x.SelectedTemplates.length;
      }
    )
  }

  move(index, direction){    
    this.moveTemplate.emit({index: index, direction: direction})
  }
}
