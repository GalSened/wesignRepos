import { AfterViewInit, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { group } from '@models/managment/groups/management-groups.model';

@Component({
  selector: 'sgn-swith-group-modal',
  templateUrl: './switch-group-modal.component.html',
  styles: [
  ]
})
export class SwitchGroupModalComponent implements OnInit, AfterViewInit {
  ngAfterViewInit(): void {
    this.selectedGroupId = null;
  }
  ngOnInit(): void {
    this.selectedGroupId = null;
  }
  
@Input()
groups : group[];
@Input()
show:boolean = false


@Output() public cancelEvent = new EventEmitter<any>();
@Output() public submitEvent = new EventEmitter<any>(); 

@Input()
public selectedGroupId : any = null;

public confirm()
{
  
  this.submitEvent.emit(this.selectedGroupId);
 
}
public cancel()
{
  this.cancelEvent.emit();
  this.selectedGroupId = null;
}

}
