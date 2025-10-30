import { Component, OnInit,Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-eula',
  templateUrl: './eula.component.html',
  styleUrls: ['./eula.component.css']
})

export class EulaComponent implements OnInit {
@Output() submit = new EventEmitter();

  public toggled: boolean = false;
 
  constructor() { }

  ngOnInit(): void {
  }

  public submitClick(){
    this.submit.emit();
  }
}
