import { Component, OnInit } from "@angular/core";
import { environment } from '../../../../environments/environment';


@Component({
    selector: "sgn-action-header",
    templateUrl: "action-header.component.html",
})

export class ActionHeaderComponent implements OnInit {
    
    public tiny =  environment.tiny;

    constructor() {        
        /* TODO */
     }

    public ngOnInit() {
        /* TODO */
     }
}
