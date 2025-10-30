import { Component, OnInit } from '@angular/core';
import { environment } from "../../../environments/environment"

@Component({
    selector: 'sgn-thankyou',
    templateUrl: 'thankyou.component.html'
})

export class ThankyouComponent implements OnInit {
    public tiny = environment.tiny;
    constructor() { }

    ngOnInit() { }
}